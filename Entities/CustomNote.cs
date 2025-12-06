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
        public bool OnlyOnce;
        public FlagList FlagsOnFinish;
        public FlagList RequiredFlags;
        public bool OnlyOncePerSession;
        public EntityID ID;

        private bool talker;
        public Glimmer Glimmer;
        public CustomNote(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = data.Int("depth", 1);
            talker = data.Bool("talker", true);
            Add(Glimmer = new Glimmer(new Vector2(data.Float("shineOffsetX"), data.Float("shineOffsetY")), Color.White, data.Float("size",8), 8, data.Float("lineLengthOffset"), data.Float("rotateRate"))
            {
                Position = new Vector2(data.Float("shineOffsetX"), data.Float("shineOffsetY")),
                FadeX = data.Bool("fadeX"),
                FadeY = data.Bool("fadeY"),
                Flashes = data.Bool("flashes"),
                FlashAttack = data.Float("flashAttack"),
                FlashRelease = data.Float("flashRelease"),
                FlashSustain = data.Float("flashSustain"),
                FlashWait = data.Float("flashOffTime"),
                FlashIntensity = data.Float("flashIntensity"),
                FlashDelay = data.Float("flashDelay"),
                MinAngle = data.Int("minAngle"),
                MaxAngle = data.Int("maxAngle", 360),
                RotationInterval = data.Float("rotateUpdateInterval", -1),
                BaseAlpha = data.Float("alpha", 1),
                FadeThresh = data.Float("fadeDistance"),
                Scale = Vector2.One,

            });
            ID = id;
            OnlyOnce = data.Bool("onlyOnce");
            OnlyOncePerSession = data.Bool("oncePerSession");
            FlagsOnFinish = data.FlagList("flagsOnFinish");
            RequiredFlags = data.FlagList("requiredFlags");
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
            Talk.Enabled = RequiredFlags && !Glimmer.Blocked;

        }
        public override void Render()
        {
            if (RequiredFlags)
            {
                base.Render();
            }

        }

        public class DialogCutscene : CutsceneEntity
        {
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