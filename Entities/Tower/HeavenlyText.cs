using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using TAS;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FlowTexture;
using static Celeste.MoonGlitchBackgroundTrigger;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class HeavenlyText : Entity
    {
        public struct Data
        {
            public string Main, Red, Green, Blue;
            public Data(string dialogID)
            {
                string d = Dialog.Get(dialogID);
                string[] array = d.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string s in array)
                {
                    if (s[0] == '+' && s.Length > 2 && s[3] == '+')
                    {
                        char second = s[1];
                        if (char.IsUpper(second)) second = char.ToLower(second);
                        switch (second)
                        {
                            case 'r':
                                Red = s.Substring(3).Trim();
                                break;
                            case 'g':
                                Green = s.Substring(3).Trim();
                                break;
                            case 'b':
                                Blue = s.Substring(3).Trim();
                                break;
                            default:
                                Main = s.Substring(3).Trim();
                                break;
                        }

                    }
                }
            }
        }
        public class Text : GraphicsComponent
        {
            public class CharData
            {
                public ExtraFancyText.Char CharReference;
                public float SinAdvance;
                public Vector2 Offset;
                public float SinOffsetMax;
                public float SinOffsetMult;
                public float IntroOffsetY;
                private float maxIntroOffsetY;
                public float Alpha;
                public bool Enabled;
                private float timer;
                public bool AtTarget => Enabled && (Reverse ? timer <= 0 : timer >= 1);
                public bool Reverse;
                public CharData(ExtraFancyText.Char charReference, float introOffsetY, float sinDistanceY, float sinAdvance)
                {
                    CharReference = charReference;
                    maxIntroOffsetY = IntroOffsetY = introOffsetY;
                    SinOffsetMax = sinDistanceY;
                    SinAdvance = sinAdvance;
                }
                public void Update(float sin)
                {
                    if (Enabled)
                    {
                        int target = Reverse ? 0 : 1;
                        float prev = timer;
                        if (timer != target)
                        {
                            timer = Calc.Approach(timer, target, Engine.DeltaTime / 0.5f);
                        }
                        if (timer != prev)
                        {
                            float eased = Ease.BackInOut(timer);
                            Alpha = eased;
                            SinOffsetMult = eased;
                            IntroOffsetY = maxIntroOffsetY * (1 - eased);
                        }
                    }
                    float sinOffset = ((sin + SinAdvance + 1) % 2 - 1) * SinOffsetMax;
                    Offset.Y = IntroOffsetY + sinOffset * SinOffsetMult;
                    CharReference.CharOffset = Offset;
                    CharReference.Alpha = Alpha;
                }
            }
            public string RawText;
            public ExtraFancyText.Text FancyText;
            public CharData[] Data;
            public float SinTimeOffset;
            public int Counter;
            private float counterTime = 0.15f, counterTimer;
            public bool Enabled;
            private bool reverse;
            public float TimeMult = 1;
            public bool Idle => Enabled && Data[^1].AtTarget;
            public void Reverse()
            {
                reverse = true;
                Counter = 0;
            }
            public void Reset()
            {
                reverse = false;
                Counter = 0;
            }
            public Text(Vector2 position, string text, int maxLineWidth, Color color, float scale = 1) : base(true)
            {
                Position = position;
                RawText = text;
                FancyText = ExtraFancyText.Parse(text, maxLineWidth, 100, Vector2.Zero, 1, color);
                FancyText.BaseSize *= scale;
                List<ExtraFancyText.Node> chars = [.. FancyText.Nodes.Where(item => item is ExtraFancyText.Char)];
                Data = new CharData[chars.Count];
                for (int i = 0; i < Data.Length; i++)
                {
                    Data[i] = new CharData(chars[i] as ExtraFancyText.Char, -FancyText.BaseSize, FancyText.BaseSize / 2f, i * Engine.DeltaTime);
                }
            }
            public override void Update()
            {
                base.Update();
                if (Enabled)
                {
                    if (Counter != Data.Length)
                    {
                        if (counterTimer > 0)
                        {
                            counterTimer -= Engine.DeltaTime * TimeMult;
                            if (counterTimer <= 0)
                            {
                                counterTimer = counterTime;
                                Data[Counter].Enabled = true;
                                Data[Counter].Reverse = reverse;
                                Counter++;
                            }
                        }
                    }
                    float sin = (float)Math.Sin((Scene.TimeActive + SinTimeOffset));
                    for (int i = 0; i < Data.Length; i++)
                    {
                        Data[i].Update(sin);
                    }
                }
                if (Idle) TimeMult = 1;
            }
        }
        public Text MainText, RedText, GreenText, BlueText;
        public bool Finished;
        public HeavenlyText(string main, string red, string green, string blue) : base()
        {
            Tag |= TagsExt.SubHUD;
            int edgeDist = 1920 / 6;
            MainText = new Text(new Vector2(1920 / 2, 1080 / 8), main, 1080 / 3, Color.White);
            RedText = new Text(new Vector2(edgeDist, 1080 / 2), red, edgeDist * 2, Color.Red);
            GreenText = new Text(new Vector2(1920 / 2, 1080 / 3 * 2), green, edgeDist * 2, Color.Green);
            BlueText = new Text(new Vector2(1920 - edgeDist, 1080 / 2), blue, edgeDist * 2, Color.Blue);
            Add(MainText, RedText, GreenText, BlueText);
            Add(new Coroutine(routine()));
        }
        private IEnumerator routine()
        {
            MainText.Enabled = true;
            bool pressed = false;
            while (!MainText.Idle)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    pressed = true;
                }
                if (pressed)
                {
                    MainText.TimeMult = Calc.Approach(MainText.TimeMult, 2, Engine.DeltaTime * 5);
                }
                yield return null;
            }
            yield return 0.25f;
            while (!Input.MenuConfirm.Pressed) yield return null;
            yield return 0.25f;
            pressed = false;
            RedText.Enabled = BlueText.Enabled = GreenText.Enabled = true;
            while (!RedText.Idle || !BlueText.Idle || !GreenText.Idle)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    pressed = true;
                }
                if (pressed)
                {
                    RedText.TimeMult = Calc.Approach(RedText.TimeMult, 2, Engine.DeltaTime * 5);
                    GreenText.TimeMult = Calc.Approach(GreenText.TimeMult, 2, Engine.DeltaTime * 5);
                    BlueText.TimeMult = Calc.Approach(BlueText.TimeMult, 2, Engine.DeltaTime * 5);
                }
                yield return null;
            }
            yield return 0.15f;
            RedText.Reverse();
            GreenText.Reverse();
            BlueText.Reverse();
            yield return 0.6f;
            MainText.Reverse();
            while (!MainText.Idle)
            {
                yield return null;
            }
            Finished = true;
            RemoveSelf();
        }
    }
    public class Cutscene : CutsceneEntity
    {
        public Entity Parent;
        public Player Player;
        public Cutscene(Entity parent, Player player) : base()
        {
            Parent = parent;
            Player = player;
        }
        public override void OnBegin(Level level)
        {
            //Parent.OnBegin(level);
            Add(new Coroutine(routine()));
        }
        public bool GroundBeneathPlayerFound(Player player)
        {
            float yLimit = Level.Bounds.Bottom;
            int downCheck = 1;
            while (!player.OnGround(downCheck) && player.Bottom + downCheck < yLimit)
            {
                downCheck++;
            }
            return player.Bottom + downCheck < yLimit;
        }
        private IEnumerator routine()
        {
            while (Player != null && !Player.Dead && !GroundBeneathPlayerFound(Player))
            {
                yield return null;
            }
            if (Player == null || Player.Dead)
            {
                EndCutscene(Level);
                yield break;
            }

            Player.StateMachine.State = Player.StDummy;
            while (!Player.OnGround()) yield return null;
            //yield return new SwapImmediately(Parent.Routine());
            EndCutscene(Level);
            /* camera pans up
             * text appears at the top. for every line spoken by the main voice, 3 other lines are spoken by smaller voices.
             * 1. Little bird... Mimic the crystals...
             * 1a. (follow their path) (system)
             * 1b. (just as i once did) (rani)
             * 1c. (unlock their knowledge) (calidus)
             * 
             * 2. Little bird... Find the lost village...
             * 2a. (bring back the light) (rani)
             * 2b. (enter the vessel) (system)
             * 2c. (discover a new world) (calidus)
             * 
             * 3. Little bird... Ascend, once again...
             * 3a. (the tower of jade) (system)
             * 3b. (the infant, the replicant) (rani)
             * 3c. (my resting place) (calidus)
             * camera pans back
             */
        }
        public override void OnEnd(Level level)
        {
            //Parent.OnEnd(level, WasSkipped);
        }
    }
}