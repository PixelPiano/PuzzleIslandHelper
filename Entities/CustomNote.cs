using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomNote")]
    [Tracked]
    public class CustomNote : Entity
    {
        public DotX3 Talk;
        public Vector2 ShineOffset;
        public bool OnlyOnce;
        public FlagList FlagsOnFinish;
        public FlagList RequiredFlags;
        public bool OnlyOncePerSession;
        public EntityID ID;
        private float rotation;
        private float rotationRate;
        private float shineSize;
        private float baseAlpha = 1;
        private float alpha => (baseAlpha * fadeMult + flashIntensity * flashMult) * blockMult;
        private float fadeThresh;
        private float rotateUpdateInterval;
        private float flashMult = 0;
        private float fadeMult = 1;
        private float offsetMax = 4;
        private int minAngle = 0;
        private int maxAngle = 360;
        private bool fadeX;
        private bool fadeY;
        private bool flashes;
        private float flashIntensity;
        private float flashDelay;
        private float flashAttack;
        private float flashSustain;
        private float flashRelease;
        private float flashWait;
        private bool talker;
        public CustomNote(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = data.Int("depth", 1);
            talker = data.Bool("talker", true);
            fadeX = data.Bool("fadeX");
            fadeY = data.Bool("fadeY");
            flashes = data.Bool("flashes");
            flashAttack = data.Float("flashAttack");
            flashRelease = data.Float("flashRelease");
            flashSustain = data.Float("flashSustain");
            flashWait = data.Float("flashOffTime");
            flashIntensity = data.Float("flashIntensity");
            flashDelay = data.Float("flashDelay");
            if (flashes)
            {
                Add(new Coroutine(flashRoutine()));
            }

            minAngle = data.Int("minAngle");
            maxAngle = data.Int("maxAngle", 360);
            rotateUpdateInterval = data.Float("rotateUpdateInterval", -1);
            rotationRate = data.Float("rotateRate");
            offsetMax = data.Float("lineLengthOffset");
            shineSize = data.Float("size");
            baseAlpha = data.Float("alpha", 1);
            fadeThresh = data.Float("fadeDistance");
            ID = id;
            OnlyOnce = data.Bool("onlyOnce");
            OnlyOncePerSession = data.Bool("oncePerSession");
            FlagsOnFinish = data.FlagList("flagsOnFinish");
            RequiredFlags = data.FlagList("requiredFlags");
            ShineOffset = new Vector2(data.Float("shineOffsetX"), data.Float("shineOffsetY"));
            Collider = new Hitbox(data.Width, data.Height);
            FlagList? scrambleFlag = null;
            if (data.Bool("scramble"))
            {
                scrambleFlag = new FlagList(data.Attr("scrambleFlag"));
            }
            Talk = new DotX3(Collider, player =>
            {
                Scene.Add(new DialogCutscene(player, data.Attr("text"), data.Bool("useDialog"), FlagsOnFinish, scrambleFlag, data.Float("scrambleInterval", 0.1f)));
                if (OnlyOncePerSession || OnlyOnce)
                {
                    RemoveSelf();
                    if (OnlyOncePerSession)
                    {
                        SceneAs<Level>().Session.DoNotLoad.Add(ID);
                    }
                }
            });
            if (talker)
            {
                Add(Talk);
            }
        }
        private bool blocked;
        private float blockMult = 1;
        public override void Update()
        {
            base.Update();
            blocked = false;
            foreach (BlockerComponent blocker in Scene.Tracker.GetComponents<BlockerComponent>())
            {
                if (blocker.Check(Position + ShineOffset))
                {
                    blocked = true;
                    break;
                }
            }
            Talk.Enabled = RequiredFlags && !blocked;
            blockMult = Calc.Approach(blockMult, blocked ? 0 : 1, 5 * Engine.DeltaTime);
            if (fadeX || fadeY)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    bool fade = false;
                    if (fadeX && fadeY)
                    {
                        fade = Vector2.Distance(player.Center, Position + ShineOffset) >= fadeThresh;
                    }
                    else if (fadeY)
                    {
                        fade = Math.Abs(player.CenterY - Y + ShineOffset.Y) >= fadeThresh;
                    }
                    else if (fadeX)
                    {
                        fade = Math.Abs(player.CenterX - X + ShineOffset.X) >= fadeThresh;
                    }
                    fadeMult = Calc.Approach(fadeMult, fade ? 0 : 1, Engine.DeltaTime);
                }
            }
            if (rotateUpdateInterval <= 0 || Scene.OnInterval(rotateUpdateInterval * Engine.DeltaTime))
            {
                rotation += rotationRate;
                rotation %= 360;
            }
        }
        public override void Render()
        {
            base.Render();
            if (alpha > 0 && RequiredFlags)
            {
                Vector2 center = Position + ShineOffset;
                for (int i = 0; i < 8; i++)
                {
                    float deg = ((360f / 8 * i) + rotation) % 360;
                    if (deg >= minAngle && deg <= maxAngle)
                    {
                        float angle = deg.ToRad();
                        float size = shineSize;
                        if (i % 2 == 0) size += offsetMax;
                        Vector2 end = center + Calc.AngleToVector(angle, size);
                        Vector2 start = center;
                        float length = (end - center).Length();
                        for (int j = 0; j < length; j++)
                        {
                            float lerp = 1f / length * j;
                            Draw.Point(Vector2.Lerp(start, end, lerp), Color.White * alpha * (1 - lerp));
                        }
                    }
                }
            }
        }
        private IEnumerator flashRoutine()
        {
            flashMult = 0;
            if (flashDelay > 0) yield return flashDelay;
            while (true)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime / flashAttack)
                {
                    flashMult = i;
                    yield return null;
                }
                flashMult = 1;
                yield return flashSustain;
                for (float i = 0; i < 1; i += Engine.DeltaTime / flashRelease)
                {
                    flashMult = 1 - i;
                    yield return null;
                }
                flashMult = 0;
                yield return flashWait;
            }
        }

        public class DialogCutscene : CutsceneEntity
        {
            private class fakeNode : FancyText.Node
            {

            }
            public Player Player;
            public string Text;
            private Textbox textbox;
            private FlagList FlagsOnEnd;
            private FancyText.Text text;
            public FlagList? scramble;
            private float interval;
            public DialogCutscene(Player player, string text, bool dialog, FlagList onEnd, FlagList? scramble = null, float scrambleInterval = 0.1f)
            {
                Player = player;
                Text = Dialog.Get(text);
                FlagsOnEnd = onEnd;
                textbox = new Textbox(Text);
                textbox.text = FancyText.Parse(Text, (int)textbox.maxLineWidth, textbox.linesPerPage, 0f, null, null);
                this.text = FancyText.Parse(Text, (int)textbox.maxLineWidth, textbox.linesPerPage, 0f, null, null);
                this.scramble = scramble;
                interval = scrambleInterval;
            }
            public override void OnBegin(Level level)
            {
                Player.DisableMovement();
                Add(new Coroutine(cutscene(level)));
            }
            public void ScrambleTextbox()
            {
                int count = 0;
                int startIndex = 0;
                List<FancyText.Char> word = [];
                PixelFontSize size = text.Font.Get(text.BaseSize);
                float lastY = 0;
                while (count < text.Nodes.Count)
                {
                    FancyText.Node node = text.Nodes[count];
                    if (node is FancyText.Char c && !c.IsPunctuation && c.Character != ' ')
                    {
                        word.Add(c);
                    }
                    else
                    {
                        if (word.Count > 0)
                        {
                            float lastX = 0;
                            float startX = word[0].Position;
                            word.Shuffle();
                            for (int i = 0; i < word.Count && startIndex + i < textbox.Nodes.Count; i++)
                            {
                                if (textbox.Nodes[i + startIndex] is FancyText.Char c2)
                                {
                                    if (lastY < c2.YOffset)
                                    {
                                        lastX = 0;
                                    }
                                    c2.Character = word[i].Character;
                                    c2.Position = startX + lastX;
                                    lastX += size.Get(c2.Character).XAdvance;
                                    lastY = c2.YOffset;
                                }

                            }
                            word.Clear();
                        }
                        startIndex = count + 1;
                    }
                    count++;
                }

            }
            private IEnumerator cutscene(Level level)
            {
                level.Add(textbox);
                while (textbox.Opened)
                {
                    if (scramble.HasValue && Scene.OnInterval(interval) && scramble.Value)
                    {
                        ScrambleTextbox();
                    }
                    yield return null;
                }
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                if (WasSkipped)
                {
                    if (textbox != null)
                    {
                        textbox.Close();
                    }
                }
                Player.EnableMovement();
                FlagsOnEnd.State = true;
            }
        }
    }
}