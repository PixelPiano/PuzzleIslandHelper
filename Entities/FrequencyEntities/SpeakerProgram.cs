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
            public Instructions(UIProgram program, Color color, Color outline) : base()
            {
                this.program = program;
                Tag |= TagsExt.SubHUD;
                Color = color;
                this.outline = outline;
                confirmTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineInOut, 0.6f);
                cancelTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineInOut, 0.6f);
                confirmTween.OnStart = (Tween t) => { confirmLerp = 1; };
                confirmTween.OnUpdate = (Tween t) => { confirmLerp = 1 - t.Eased; };
                confirmTween.OnComplete = (Tween t) => { confirmLerp = 0; };
                cancelTween.OnStart = (Tween t) => { cancelLerp = 1; };
                cancelTween.OnUpdate = (Tween t) => { cancelLerp = 1 - t.Eased; };
                cancelTween.OnComplete = (Tween t) => { cancelLerp = 0; };
                Add(confirmTween, cancelTween);
            }
            public override void Update()
            {
                base.Update();
                if (program.InControl)
                {
                    if (Input.MenuConfirm.Pressed && !confirmWasPressed)
                    {
                        confirmTween.Start();
                    }
                    if (Input.MenuCancel.Pressed && !cancelWasPressed)
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
                Vector2 position = Vector2.One * 10;
                Color cancelButtonColor = Color.Lerp(Color.White, Color.Black, cancelLerp * 0.7f);
                Color confirmButtonColor = Color.Lerp(Color.White, Color.Black, confirmLerp * 0.7f);
                float h = ButtonUIExt.Height("Check", Input.MenuConfirm);
                ButtonUIExt.RenderOutline(position + Vector2.UnitY * (h / 2) * (1 + confirmLerp), "Check", Input.MenuConfirm, Color.White, confirmButtonColor, 6, 1, 0);
                float h2 = ButtonUIExt.Height("Exit", Input.MenuCancel);
                ButtonUIExt.RenderOutline(position + Vector2.UnitY * (h + (h2 / 2) * (1 + cancelLerp)), "Exit", Input.MenuCancel, Color.White, cancelButtonColor, 6, 1, 0);
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
        }
        public override void Update()
        {
            base.Update();
            float[] rates = FrequencyData.GetRates(Scene);
            float space = (width * 0.8f) / rates.Length;
            float rateX = x + (width * 0.2f);
            for (int i = 0; i < realRates.Count && i < rates.Length; i++)
            {
                Vector2 position = new Vector2(rateX, y + height - (height * (rates[i] / FrequencyData.Max)));
                realRates[i].Position = position * 6;
                rateX += space;
            }
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
            if (FrequencyCodeComponent.CalculatePercent(Scene, Rates) == 1)
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
            float space = (width * 0.8f) / rates.Length;
            float rateX = x + (width * 0.2f);

            for (int i = 0; i < rates.Length; i++)
            {
                Vector2 position = new Vector2(rateX, y + height - ((height / 10) * (rates[i] / 12f)) - (height / 5));
                GlitchCircle circle = new GlitchCircle(position * 6, 30, color, color2);
                circles.Add(circle);
                rateX += space;
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
                Vector2 start = new Vector2(x, y + (height / 4f) * i);
                Vector2 end = new Vector2(x + width, start.Y);
                GlitchLine line = new(start * 6, end * 6);
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
            return entities;
        }
    }

}
