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
        private const int edgeDist = 1920 / 6;
        public struct Data
        {
            public string Main, Red, Green, Blue;
            public Data(string dialogID)
            {
                string d = Dialog.Get(dialogID);
                string[] array = d.Split("{break}", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string s in array)
                {
                    if (s.StartsWith("m:"))
                    {
                        Main = s.Substring(2).Trim();
                    }
                    else if (s.StartsWith("r:"))
                    {
                        Red = s.Substring(2).Trim();
                    }
                    else if (s.StartsWith("g:"))
                    {
                        Green = s.Substring(2).Trim();
                    }
                    else if (s.StartsWith("b:"))
                    {
                        Blue = s.Substring(2).Trim();
                    }
                }
            }
        }
        public enum Voices
        {
            Main,
            Red,
            Green,
            Blue
        }
        public class Text : GraphicsComponent
        {
            private const float counterTime = 0.06f;
            public Voices Voice;
            public class Node
            {
                public int Index;
                public Voices Voice;
                public Color Color;
                public ExtraFancyText.Node Ref;
                public ExtraFancyText.Char CharRef;
                public float SinOffset;
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
                private struct colorOffset
                {
                    public colorOffset() { }
                    public Vector2 Offset;
                    private double mult = 0.0099999997764825821;
                    private double multmult = 1;
                    public Vector2 Target = Calc.Random.ShakeVector() * 6;
                    public void Update()
                    {
                        if (Offset != Target)
                        {
                            Offset = PianoUtils.RubberbandApproach(Offset, Target, 0.2f, mult * multmult);
                        }
                        else
                        {
                            Target = Calc.Random.ShakeVector() * 6;
                            multmult = Calc.Random.NextFloat();
                        }
                    }
                }

                private colorOffset[] offsets = new colorOffset[3];
                public Node(int index, ExtraFancyText.Node node)
                {
                    Index = index;
                    Ref = node;
                }
                public Node(int index, ExtraFancyText.Char charReference, float introOffsetY, float sinDistanceY, float sinAdvance, Voices voice) : this(index, charReference)
                {
                    CharRef = charReference;
                    maxIntroOffsetY = IntroOffsetY = introOffsetY;
                    SinOffsetMax = sinDistanceY;
                    SinOffset = sinAdvance;
                    Voice = voice;
                    Color = Voice switch
                    {
                        Voices.Main => Color.White,
                        Voices.Red => Color.Red,
                        Voices.Green => Color.Lime,
                        Voices.Blue => Color.Blue
                    };
                }
                public void Draw(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
                {
                    position += Offset;
                    if (Voice == Voices.Main)
                    {
                        var blend = Engine.Graphics.GraphicsDevice.BlendState;
                        Engine.Graphics.GraphicsDevice.BlendState = BlendState.Additive;
                        CharRef.Draw(font, baseSize, position + offsets[0].Offset, scale, alpha * Alpha * 0.34f, Color.Red);
                        CharRef.Draw(font, baseSize, position + offsets[1].Offset, scale, alpha * Alpha * 0.33f, Color.Lime);
                        CharRef.Draw(font, baseSize, position + offsets[2].Offset, scale, alpha * Alpha * 0.33f, Color.Blue);
                        Engine.Graphics.GraphicsDevice.BlendState = blend;
                    }
                    else
                    {
                        CharRef.Draw(font, baseSize, position, scale, alpha * Alpha, color);
                    }
                }
                public void Update(float sinOffset)
                {
                    if (CharRef != null)
                    {
                        if (Enabled)
                        {
                            if (Voice == Voices.Main)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    offsets[i].Update();
                                }
                            }
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
                        float sin = (float)(Math.Sin(Engine.Scene.TimeActive + SinOffset + sinOffset) * SinOffsetMax);
                        Offset.Y = IntroOffsetY + sin;
                        CharRef.Alpha = Alpha;
                    }
                }
            }
            public string RawText;
            public ExtraFancyText.Text FancyText;

            public Node[] Data;
            public float SinTimeOffset;
            public int Counter;
            private float counterTimer = counterTime;
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
            public Text(Vector2 position, string text, int maxLineWidth, Voices voice, float scale = 1) : base(true)
            {
                Position = position;
                RawText = text;
                Color = voice switch
                {
                    Voices.Main => Color.White,
                    Voices.Blue => Color.Blue,
                    Voices.Green => Color.Lime,
                    Voices.Red => Color.Red
                };
                FancyText = ExtraFancyText.Parse(text, maxLineWidth, 100, Vector2.Zero, 1, Color);
                FancyText.BaseSize *= scale;
                Data = new Node[FancyText.Nodes.Count];
                for (int i = 0; i < Data.Length; i++)
                {
                    var node = FancyText.Nodes[i];
                    if (node is ExtraFancyText.Char)
                    {
                        Data[i] = new Node(i, node as ExtraFancyText.Char, -FancyText.BaseSize, FancyText.BaseSize / 2f, i * Engine.DeltaTime, Voice);
                    }
                    else
                    {
                        Data[i] = new Node(i, node);
                    }
                }
            }
            private void advanceNode()
            {
                Data[Counter].Enabled = true;
                Data[Counter].Reverse = reverse;
                Counter++;
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
                                while (Counter < Data.Length && Data[Counter].CharRef == null)
                                {
                                    advanceNode();
                                }
                                advanceNode();
                            }
                        }
                    }
                    for (int i = 0; i < Data.Length; i++)
                    {
                        Data[i].Update(SinTimeOffset);
                    }
                }
                if (Idle) TimeMult = 1;
            }
            public IEnumerator Routine()
            {
                while (!Idle)
                {
                    ScrollUpdate();
                    yield return null;
                }
            }
            public bool ScrollUpdate()
            {
                bool pressed = false;
                if (Input.MenuConfirm.Pressed)
                {
                    pressed = true;
                }
                if (pressed)
                {
                    TimeMult = Calc.Approach(TimeMult, 2, Engine.DeltaTime * 5);
                }
                return pressed;
            }
            public override void Render()
            {
                base.Render();
                Draw(RenderPosition, new Vector2(0.5f, 0), Vector2.One, 1, Color, 0, Counter);
            }

            public void Draw(Vector2 position, Vector2 justify, Vector2 scale, float alpha, Color color, int start = 0, int end = int.MaxValue)
            {
                int num = Math.Min(Data.Length, end);
                int num2 = 0;
                float num3 = 0f;
                float num4 = 0f;

                PixelFontSize pixelFontSize = FancyText.Font.Get(FancyText.BaseSize);
                for (int i = start; i < num; i++)
                {
                    if (Data[i].Ref is ExtraFancyText.NewLine)
                    {
                        if (num3 == 0f)
                        {
                            num3 = 1f;
                        }

                        num4 += num3;
                        num3 = 0f;
                    }
                    else if (Data[i].CharRef != null)
                    {
                        num2 = Math.Max(num2, (int)Data[i].CharRef.LineWidth);
                        num3 = Math.Max(num3, Data[i].CharRef.Scale);
                    }
                    else if (Data[i].Ref is ExtraFancyText.NewPage)
                    {
                        break;
                    }
                }

                num4 += num3;
                position -= justify * new Vector2(num2, num4 * (float)pixelFontSize.LineHeight) * scale;
                num3 = 0f;
                for (int j = start; j < num && !(Data[j].Ref is ExtraFancyText.NewPage); j++)
                {
                    if (Data[j].Ref is ExtraFancyText.NewLine)
                    {
                        if (num3 == 0f)
                        {
                            num3 = 1f;
                        }

                        position.Y += (float)pixelFontSize.LineHeight * num3 * scale.Y;
                        num3 = 0f;
                    }
                    if (Data[j].CharRef != null)
                    {
                        Data[j].Draw(FancyText.Font, FancyText.BaseSize, position, scale, alpha, color);
                        num3 = Math.Max(num3, Data[j].CharRef.Scale);
                    }
                }
            }
        }
        public class TextGroup
        {
            public Vector2 Position;
            public float Width;
            public float LineHeight;
            public Text Main, Red, Green, Blue;
            public bool Finished;
            public TextGroup(Vector2 position, int width, Data data)
            {
                Position = position;
                Width = width;
                float lineHeight = ActiveFont.LineHeight;
                Main = new Text(position, data.Main, width / 3, Voices.Main);
                Red = new Text(position + new Vector2(-position.X / 2, lineHeight * Main.FancyText.Lines + 1), data.Red, width / 3, Voices.Red);
                Blue = new Text(position + new Vector2(position.X / 2, lineHeight * Main.FancyText.Lines + 1), data.Blue, width / 3, Voices.Blue);
                Green = new Text(position + new Vector2(0, lineHeight * (Main.FancyText.Lines + Math.Max(Red.FancyText.Lines, Blue.FancyText.Lines) + 1)), data.Green, width / 3, Voices.Green);

            }
            public IEnumerator Routine(Entity entity)
            {
                Finished = false;
                entity.Add(Main, Red, Green, Blue);
                Main.Enabled = true;
                yield return Main.Routine();
                yield return 0.25f;
                while (!Input.MenuConfirm.Pressed) yield return null;
                yield return 0.25f;
                Red.Enabled = Blue.Enabled = Green.Enabled = true;
                Coroutine[] coroutines = new Coroutine[] { new(Red.Routine()), new(Green.Routine()), new(Blue.Routine()) };
                entity.Add(coroutines);
                foreach (var c in coroutines)
                {
                    if (!c.Finished) yield return null;
                }
                yield return 0.6f;
                while (!Input.MenuConfirm.Pressed) yield return null;
                yield return 0.25f;
                /*                yield return 0.15f;
                                Red.Reverse();
                                Green.Reverse();
                                Blue.Reverse();
                                yield return 0.6f;
                                Main.Reverse();*/
                while (!Main.Idle)
                {
                    yield return null;
                }
                Finished = true;
            }
        }
        public TextGroup A, B, C;

        public bool Finished;
        public HeavenlyText(bool blue) : base()
        {
            Tag |= TagsExt.SubHUD;
            string prefix = (blue ? "Blue" : "Green") + "HeavenlyText";
            Vector2 topPosition = new Vector2(1920 / 2f, ActiveFont.LineHeight * 1.5f);
            A = new TextGroup(topPosition, 1920, new Data(prefix + "A"));
            B = new TextGroup(topPosition + Vector2.UnitY * ActiveFont.LineHeight * 4, 1920, new Data(prefix + "B"));
            C = new TextGroup(topPosition + Vector2.UnitY * ActiveFont.LineHeight * 8, 1920, new Data(prefix + "C"));
            Add(new Coroutine(routine()));
            /* camera pans up
 * text appears at the top. for every line spoken by the main voice, 3 other lines are spoken by smaller voices.
 * 1. Little bird... 
 * Mimic the pointed crystals...
 * 1a. (haven't I done enough?) (maddy)
 * 1b. (follow their path) (rani)
 * 1c. (unlock their knowledge) (calidus)
 * 
 * 2. Little bird...
 * Find the lost village...
 * 2a. (bring light back to the winding halls) (rani)
 * 2b. (don't leave me in the dark) (maddy)
 * 2c. (restore the core and enter the vessel) (calidus)
 * 
 * 3. Little bird... 
 * Ascend, once again...
 * 3a. (the infant, the replicant) (rani)
 * 3b. (you'll take me with you... right?) (maddy)
 * 3c. (my resting place) (calidus)
 * camera pans back
 */
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(0, 0, 50, 50, Color.Red);
            if (A.Main.Idle)
            {
                Draw.Rect(50, 50, 10, 10, Color.Lime);
            }
            ActiveFont.Draw(A.Main.Counter.ToString(), Vector2.UnitY * 50, Color.Magenta);
            ActiveFont.Draw(A.Main.Data.Length.ToString(), Vector2.UnitY * (50 + ActiveFont.LineHeight), Color.Magenta);
        }
        private IEnumerator routine()
        {
            yield return A.Routine(this);
            //yield return B.Routine(this);
            //yield return C.Routine(this);
            Finished = true;
            RemoveSelf();
        }
    }
}