using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    public class SpeakerProgram : UIProgram
    {
        public class Instructions : Entity
        {
            public readonly static Vector2 BottomCenterScreen = new Vector2(160, 180) * 6;
            public const int ButtonOffset = 320 / 4 * 6;
            public readonly static Vector2 ButtonSize = Vector2.Max(ButtonUIExt.Size("Check", Input.MenuConfirm), ButtonUIExt.Size("Exit", Input.MenuCancel));
            public static float MaxTransitionOffset = ButtonSize.Y * 5;
            public bool InControl;
            public bool Finished;
            public Color Color;
            private Color outline;
            public Image Image;
            public bool Selected = true;
            private float lerp;
            private float confirmLerp, cancelLerp;
            private bool confirmWasPressed;
            private bool cancelWasPressed;
            private Tween confirmTween;
            private Tween cancelTween;
            private UIProgram program;
            private bool cancelDisabled, confirmDisabled;
            private Tween transitionTween;
            private Vector2 transitionOffset;
            private VertexPositionColor[] gradient = new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(0,160,0) * 6, Color.Transparent),
                new VertexPositionColor(new Vector3(320,160,0) * 6, Color.Transparent),
                new VertexPositionColor(new Vector3(0,180,0) * 6, Color.Black),
                new VertexPositionColor(new Vector3(320,180,0) * 6, Color.Black),
            };
            private int[] indices = [0, 1, 2, 2, 1, 3];
            public Instructions(UIProgram program, Color color, Color outline) : base()
            {
                this.program = program;
                Tag |= TagsExt.SubHUD;
                Color = color;
                this.outline = outline;
                transitionTween = new Tween();

                confirmTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineInOut, 0.6f);
                cancelTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineInOut, 0.6f);
                confirmTween.OnStart = (Tween t) => { confirmLerp = 1; cancelDisabled = true; };
                confirmTween.OnUpdate = (Tween t) => { confirmLerp = 1 - t.Eased; };
                confirmTween.OnComplete = (Tween t) => { confirmLerp = 0; cancelDisabled = false; };
                cancelTween.OnStart = (Tween t) => { cancelLerp = 1; confirmDisabled = true; };
                cancelTween.OnUpdate = (Tween t) => { cancelLerp = 1 - t.Eased; };
                cancelTween.OnComplete = (Tween t) => { cancelLerp = 0; cancelDisabled = false; };
                Add(transitionTween, confirmTween, cancelTween);
            }
            public void Intro()
            {
                InControl = false;
                float from = MaxTransitionOffset;
                transitionTween.OnUpdate = (Tween t) =>
                {
                    transitionOffset.Y = from * (1 - (Ease.SineInOut(t.Eased)));
                };
                transitionTween.OnComplete = (Tween t) => { InControl = true; };
            }
            public void Outro()
            {
                InControl = false;
                transitionTween.OnUpdate = (Tween t) =>
                {
                    transitionOffset.Y = MaxTransitionOffset * (Ease.SineInOut(t.Eased));
                };
                transitionTween.OnComplete = (Tween t) => { Finished = true; };
            }
            public override void Update()
            {
                base.Update();
                if (program.InControl && InControl)
                {
                    if (Input.MenuConfirm.Pressed && !confirmWasPressed && !confirmDisabled)
                    {
                        confirmTween.Start();
                    }
                    if (Input.MenuCancel.Pressed && !cancelWasPressed && !cancelDisabled)
                    {
                        cancelTween.Start();
                    }
                    confirmWasPressed = Input.MenuConfirm.Pressed;
                    cancelWasPressed = Input.MenuCancel.Pressed;
                }
            }
            public override void Render()
            {
                base.Render();
                Color cancelButtonColor = Color.Lerp(Color.White, Color.Black, cancelDisabled ? 0.7f : cancelLerp * 0.7f);
                Color cancelTextColor = Color.Lerp(Color.White, Color.Black, cancelDisabled ? 0.7f : 0);
                Color confirmButtonColor = Color.Lerp(Color.White, Color.Black, confirmDisabled ? 0.7f : confirmLerp * 0.7f);
                Color confirmTextColor = Color.Lerp(Color.White, Color.Black, confirmDisabled ? 0.7f : 0);
                ButtonUIExt.RenderOutline(BottomCenterScreen + new Vector2(-ButtonOffset, -ButtonSize.Y * (2 - confirmLerp)) + transitionOffset, "Check", Input.MenuConfirm, confirmTextColor, confirmButtonColor, 6, 1, 0.5f);
                ButtonUIExt.RenderOutline(BottomCenterScreen + new Vector2(ButtonOffset, -ButtonSize.Y * (2 - cancelLerp)) + transitionOffset, "Exit", Input.MenuCancel, cancelTextColor, cancelButtonColor, 6, 1, 0.5f);
                GFX.DrawIndexedVertices(Matrix.Identity, gradient, 4, indices, 2);
            }

        }
        public float[] Rates;
        private Instructions button;
        private List<GlitchCircle> realRates = [];
        private List<GlitchCircle> targetRateCircles = [];
        private List<GlitchLine> lines = [];
        private int width, x, y, height;
        public SpeakerProgram(float[] rates, bool start = false) : base(new Color(38, 255, 58), Color.Black, Color.Lime, start)
        {
            Rates = rates;
            Rates = [24, 48, 72, 96];
        }
        public override void Update()
        {
            base.Update();
            UpdatePositions();
            Engine.Commands.Log("yeah");
        }
        public void UpdatePositions()
        {
            float[] rates = FrequencyData.GetRates(Scene);
            for (int i = 0; i < realRates.Count && i < rates.Length; i++)
            {
                realRates[i].Position = GetRatePosition(rates[i], i, new Vector2(x, y), width, height, 0.1f) * 6;
            }
            for (int i = 0; i < targetRateCircles.Count; i++)
            {
                targetRateCircles[i].Position = GetRatePosition(Rates[i], i, new Vector2(x, y), width, height, 0.1f) * 6;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Position = lines[i].From = GetRatePosition(FrequencyData.Interval * i, 0, new Vector2(x, y), width, height) * 6;
                lines[i].To = lines[i].From + Vector2.UnitX * width * 6;
            }
        }
        public static Vector2 GetRatePosition(float rate, int index, Vector2 offset, float width, float height, float padPercent = 0)
        {
            offset.X += (width * padPercent);
            width *= (1 - padPercent * 2);
            return offset + new Vector2(width * (index * 0.25f), height * (1 - (rate / FrequencyData.Max)));
        }
        public override void OnCancel()
        {
            base.OnCancel();
            End();
        }
        public override void OnConfirm()
        {
            base.OnConfirm();
            InControl = false;
            if (FrequencyReceiver.CalculatePercent(Rates, FrequencyData.GetRates(Scene)) == 1)
            {
                Scene.Add(new Cutscene(this));
            }
            else
            {
                Add(new Coroutine(incorrectRoutine()));
            }
        }
        private class Cutscene : CutsceneEntity
        {
            private SpeakerProgram program;
            private ForkAmpSpeaker speaker;
            private ForkAmpTabletBox box;
            public Cutscene(SpeakerProgram program) : base()
            {
                this.program = program;
            }
            public override void OnBegin(Level level)
            {
                if (level.Tracker.GetEntity<ForkAmpSpeaker>() is ForkAmpSpeaker speaker && level.Tracker.GetEntity<ForkAmpTabletBox>() is ForkAmpTabletBox box)
                {
                    this.speaker = speaker;
                    this.box = box;
                    if (level.GetPlayer() is Player p)
                    {
                        p.StateMachine.Locked = true;
                        Add(new Coroutine(routine(p)));
                    }
                }
            }
            private IEnumerator routine(Player player)
            {
                yield return 0.7f;
                Color fromA = Color.Lime, fromB = Color.Green;
                Color toA = Color.Lerp(fromA, Color.White, 0.8f);
                Color toB = Color.Lerp(fromB, Color.White, 0.8f);
                for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                {
                    float ease = Ease.SineInOut(i);
                    Color a = Color.Lerp(fromA, toA, 1 - ease);
                    Color b = Color.Lerp(fromB, toB, 1 - ease);
                    foreach (var c in program.targetRateCircles)
                    {
                        c.Color = a;
                        c.Color2 = b;
                    }
                    yield return null;
                }
                foreach (var c in program.targetRateCircles)
                {
                    c.Color = fromA;
                    c.Color2 = fromB;
                }
                yield return 0.5f;
                program.End();
                while (speaker.Running)
                {
                    yield return null;
                }
                player.Face(box);
                box.Shake();
                yield return 0.7f;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.Locked = false;
                    player.EnableMovement();
                }
                if (!ForkAmpTabletBox.boxShook)
                {
                    ForkAmpTabletBox.boxShook.State = true;
                    if (box != null)
                    {
                        box.PrepareForTalk();
                    }
                }
                speaker.SetUsed();

            }
        }
        private IEnumerator incorrectRoutine()
        {
            foreach (GlitchCircle c in realRates)
            {
                c.Visible = true;
                c.Alpha = 0;
            }
            for (int i = 0; i < 3; i++)
            {
                for (float l = 0; l < 1; l += Engine.DeltaTime / 0.3f)
                {
                    foreach (GlitchCircle c in realRates)
                    {
                        c.Alpha = l;
                    }
                    yield return null;
                }
                for (float l = 0; l < 1; l += Engine.DeltaTime / 0.3f)
                {
                    foreach (GlitchCircle c in realRates)
                    {
                        c.Alpha = 1 - l;
                    }
                    yield return null;
                }
            }
            foreach (GlitchCircle c in realRates)
            {
                c.Visible = false;
                c.Alpha = 0;
            }
            InControl = true;
        }
        public static List<GlitchCircle> CirclesFromRates(float x, float y, float width, float height, Color color, Color color2, params float[] rates)
        {
            List<GlitchCircle> circles = [];

            for (int i = 0; i < rates.Length; i++)
            {
                Vector2 position = GetRatePosition(rates[i], i, new Vector2(x, y), width, height, 0.1f);
                GlitchCircle circle = new GlitchCircle(position * 6, 30, color, color2);
                circles.Add(circle);
            }
            return circles;
        }
        public override List<Entity> CreateEntities(Scene scene)
        {
            List<Entity> entities = [];
            lines = [];
            height = (int)(180 * 0.5f);
            width = (int)(320 * 0.5f);
            y = (180 / 2) - (height / 2);
            x = (320 / 2) - (width / 2);

            for (int i = 0; i < 4; i++)
            {
                GlitchLine line = new(Vector2.Zero, Vector2.Zero);
                entities.Add(line);
                lines.Add(line);
            }
            GlitchLine line2 = new(new Vector2(x, y + height) * 6, new Vector2(x + width, y + height) * 6);
            entities.Add(line2);
            lines.Add(line2);
            realRates = CirclesFromRates(x, y, width, height, Color.Red, Color.DarkRed, FrequencyData.GetRates(Scene));
            targetRateCircles = CirclesFromRates(x, y, width, height, Color.Lime, Color.Green, Rates);
            entities.AddRange(targetRateCircles);
            entities.AddRange(realRates);
            foreach (GlitchCircle c in realRates)
            {
                c.Alpha = 0;
                c.Visible = false;
            }
            button = new Instructions(this, FGColor, Color.BlueViolet);
            entities.Add(button);
            UpdatePositions();
            return entities;
        }
    }

}
