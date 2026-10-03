using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.Singularity;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using static Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs.Osc;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{

    public class EndingA : CutsceneEntity
    {
        public class QuickAction : Entity
        {
            public enum Presets
            {
                Red,
                Green,
                Blue,
                Alpha
            }
            public float Alpha = 0;
            public float ColorLerp;
            public float ScaleMult;
            public float Decay;
            public float Increment;
            private float slider;
            private float max;
            public float Percent => max / slider;
            private Action onPress;
            private Coroutine pressCoroutine;
            public bool Finished;
            private VirtualButton button;
            private Color color;
            private bool intro = true;
            public QuickAction(Presets preset, Action onPress, float max, float increment, float decay) : base()
            {
                this.max = max;
                this.onPress = onPress;
                Increment = increment;
                Decay = decay;
                Tag |= TagsExt.SubHUD;
                Add(pressCoroutine = new Coroutine(false));
                button = preset switch
                {
                    Presets.Red => Input.MenuConfirm,
                    Presets.Green => Input.Jump,
                    Presets.Blue => Input.Grab,
                    Presets.Alpha => Input.Dash,
                };
                color = preset switch
                {
                    Presets.Red => Color.Red,
                    Presets.Green => Color.Green,
                    Presets.Blue => Color.Blue,
                    Presets.Alpha => Color.White
                };
                Tween.Set(this, Tween.TweenMode.Oneshot, 0.7f, Ease.CubeInOut, t =>
                {
                    ScaleMult = t.Eased;
                    ColorLerp = Calc.LerpClamp(0.5f, 1, t.Eased);
                }, t =>
                {
                    ScaleMult = 1;
                    ColorLerp = 1;
                    intro = false;
                });
            }
            public override void Render()
            {
                base.Render();
                Vector2 position = new Vector2(160 * 6, 45 * 6);
                Color color = this.color;
                if (ColorLerp > 0)
                {
                    color = Color.Lerp(color, Color.White, ColorLerp);
                }
                else
                {
                    color = Color.Lerp(color, Color.Black, -ColorLerp);
                }
                BlendState b = Engine.Graphics.GraphicsDevice.BlendState;
                Engine.Graphics.GraphicsDevice.BlendState = BlendState.Additive;
                if (Input.GuiInputController(Input.PrefixMode.Latest))
                {
                    Input.GuiButton(button, Input.PrefixMode.Latest).DrawJustified(position, new Vector2(0.5f), color * Alpha, 6 * ScaleMult);
                }
                else
                {
                    ActiveFont.DrawOutline(Input.FirstKey(button).ToString().ToUpper(), position, new Vector2(0.5f), new Vector2(6 * ScaleMult), color * Alpha, 2f, Color.Black * Alpha);
                }
                Engine.Graphics.GraphicsDevice.BlendState = b;
            }
            public override void Update()
            {
                base.Update();
                if (!Finished && !intro)
                {
                    slider = Math.Max(0, slider + Decay);
                    if (button.Pressed)
                    {
                        button.ConsumePress();
                        Press();
                    }
                }
            }
            public void Press()
            {
                onPress?.Invoke();
                slider += Increment;
                if (slider >= max)
                {
                    pressCoroutine.Replace(pressRoutine(true));
                    Finished = true;
                }
                else
                {
                    pressCoroutine.Replace(pressRoutine(false));
                }
            }
            private IEnumerator pressRoutine(bool final)
            {
                float multFrom = ScaleMult;
                float lerpFrom = ColorLerp;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.05f)
                {
                    ScaleMult = Calc.LerpClamp(multFrom, 1.2f, Ease.SineOut(i));
                    ColorLerp = Calc.LerpClamp(lerpFrom, 0.5f, Ease.SineOut(i));
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    ScaleMult = Calc.LerpClamp(1.2f, 0.7f, Ease.ElasticIn(i));
                    ColorLerp = Calc.LerpClamp(0.5f, -0.5f, Ease.ElasticIn(i));
                    yield return null;
                }
                if (final)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                    {
                        Alpha = 1 - i;
                        yield return null;
                    }
                    RemoveSelf();
                }
                else
                {
                    yield return 0.05f;
                    for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
                    {
                        ScaleMult = Calc.LerpClamp(0.7f, 1f, Ease.CubeIn(i));
                        ColorLerp = Calc.LerpClamp(-0.5f, 0, Ease.CubeIn(i));
                        yield return null;
                    }
                    ColorLerp = 0;
                    ScaleMult = 1;
                }
            }
        }
        public Player Player;
        public SingularityActor Singularity;
        public Tower.Eye Eye;
        public Vector2 GroundPosition;

        //lovingly copied from CS10_FinalLaunch because brain bad atm
        public Vector2 cameraWaveOffset;
        public Vector2 cameraOffset;
        public Vector2 cameraPosition;
        public Coroutine wave, spiral, launch, orb;

        public float OrbSineEndDistance = 20;
        private float OrbSineTimerMult = 2;

        private Vector2 spiralStart;
        private float spiralYOffset;
        private float spiralRadius;
        private float spiralWaveLength = 120;
        private float spiralSpeed;
        private bool inSpiral;
        private const int maxSpiralRadius = 30;
        //private AscendPattern pattern;
        public AscendManager.Streaks streaks;
        public AscendBarrier Barrier;
        public float Push;
        private float pushSpeed;
        private float barrierSpeed;
        private Coroutine orbCoroutine;
        public EndingA(Player player, SingularityActor singularity)
        {
            Player = player;
            Singularity = singularity;
            Add(orbCoroutine = new Coroutine(false));
        }


        [Command("test_", "")]
        public static void Test()
        {
            if (Engine.Scene != null)
            {
                AscendBarrier b = new AscendBarrier(Vector2.Zero) { VertexOffset = Vector3.UnitY * -180 };
                Engine.Scene.Add(b);
                PuzzleIslandHelper.Components.KeyComponent.Wasd(entity: b,
                    left: () => b.VertexOffset.Y -= 5, right: () => b.VertexOffset.Y += 5);
            }
        }
        public override void OnBegin(Level level)
        {
            //pattern = level.Tracker.GetEntity<AscendPattern>();
            /*            if (pattern != null)
                        {
                            //pattern.SwapClipMode();
                        }*/
            Barrier = new AscendBarrier(Vector2.Zero) { VertexOffset = Vector3.UnitY * -180 };
            level.Add(Barrier);
            Barrier.Pitch = MathHelper.Pi;
            cameraPosition = level.Camera.Position;
            Player.StateMachine.State = Player.StDummy;
            Player.Speed = default;
            Player.DummyAutoAnimate = false;
            Player.DummyFriction = false;
            Player.DummyGravity = false;
            Player.Collidable = false;
            Player.ForceCameraUpdate = true;
            Singularity.State = SingularityActor.StDummy;
            Eye = level.Tracker.GetEntity<Tower.Eye>();
            Add(new Coroutine(Cutscene()));
        }
        public override void Update()
        {
            base.Update();
            manageAreas();
            Level.CameraOffset = cameraOffset;
            if (inSpiral)
            {
                float x = spiralStart.X + (float)(double)(spiralRadius * Math.Sin((spiralStart.Y + spiralYOffset) * Math.PI / spiralWaveLength));
                spiralYOffset += spiralSpeed * Engine.DeltaTime;
                Singularity.MoveToX(x);
                Player.MoveToX(Singularity.X);
                Player.MoveToY(Singularity.Y);
                //pattern.TotalDistance += spiralSpeed * 2 * Engine.DeltaTime;
            }
            else
            {
                //pattern.TotalDistance += pushSpeed * Engine.DeltaTime;
            }
            if (Barrier != null)
            {
                Barrier.VertexOffset = Vector3.UnitY * -180 + barrierVertexOffset;
                Vector2 renderPos = Barrier.RenderPosition + Barrier.VertexOffset.XY();
                if (!Barrier.Shattered && renderPos.Y - 90 > Player.Y)
                {
                    Barrier.Push = renderPos.Y - 90 - Player.Y;
                }
            }
            Singularity.Orbit += (spiralSpeed * Engine.DeltaTime / 10f);
            pushSpeed = Calc.Approach(pushSpeed, 0, 900f * Engine.DeltaTime);
        }
        public float BarrierY;
        public float BarrierOffset;
        public override void OnEnd(Level level)
        {

        }
        public void ReloadBarrier(float shake, int size)
        {
            Level.Shake(shake);
            Barrier.CreateVertices(size);
        }
        private IEnumerator orbDistanceLoopRoutine()
        {
            float from = Singularity.Sprite.Distance;
            while (true)
            {
                if (from != 20)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime * OrbSineTimerMult)
                    {
                        Singularity.Sprite.Distance = Calc.LerpClamp(from, 20, Ease.SineInOut(i));
                        yield return null;
                    }
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * OrbSineTimerMult)
                {
                    Singularity.Sprite.Distance = Calc.LerpClamp(20f, 36, Ease.SineInOut(i));
                    yield return null;
                }
                from = 36;
                yield return null;
            }
        }
        private float inputSlider;


        private IEnumerator waitForInputs(int count)
        {
            while (inputSlider < count)
            {
                yield return null;
            }
        }

        private IEnumerator orbAngleRoutine(int index, float target, float time)
        {
            var orb = Singularity.Sprite.ActiveOrbs[index];
            orb.OrbitAngle += orb.OrbitOffset;
            orb.OrbitAngle %= MathHelper.TwoPi;
            orb.OrbitOffset = 0;
            float from = orb.OrbitAngle;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                orb.OrbitAngle = Calc.LerpClamp(from, target, Ease.CubeOut(i));
                yield return null;
            }


        }
        private Vector3 barrierVertexOffset;
        private IEnumerator orbQuicktimeRoutine()
        {
            while (!PianoUtils.TryRubberbandApproach(Singularity.Sprite.Distance, 40, out float output, 1))
            {
                Singularity.Sprite.Distance = output;
                yield return null;
            }
            Singularity.Sprite.Distance = 40;
        }
        [Command("sl", "")]
        public static void SetSlider(string name, float value)
        {
            if (Engine.Scene is Level level)
            {
                level.Session.SetSlider(name, value);
            }
        }
        [Command("ct", "")]
        public static void SetCounter(string name, int value)
        {
            if (Engine.Scene is Level level)
            {
                level.Session.SetCounter(name, value);
            }
        }
        private static Coroutine testCoroutine;
        [Command("test_a", "")]
        public static void wiggle(int state)
        {
            if (Engine.Scene.Tracker.GetEntity<SingularityActor>() is SingularityActor a)
            {
                switch (state)
                {
                    case 0:
                        a.Wiggle();
                        a.StopShaking();
                        break;
                    case 1:
                        a.StartShaking(-1);
                        break;
                    case 2:
                        a.StopShaking();
                        a.Distance = 8;
                        testCoroutine?.RemoveSelf();
                        a.Add(testCoroutine = new Coroutine(r(Engine.Scene, a)));
                        break;
                }
            }
        }
        private static IEnumerator r(Scene scene, SingularityActor a)
        {
            float from = a.Distance;
            Level level = scene as Level;
            float speed = 100f;
            a.Wiggle();
            a.OrbitRateMult = 2;
            while (speed != 0)
            {
                a.Sprite.Distance += speed * Engine.DeltaTime;
                speed = Calc.Approach(speed, 0, 600f * Engine.DeltaTime);
                yield return null;
            }
            yield return 0.05f;
            while (a.Sprite.Distance != 1)
            {
                a.OrbitRateMult = PianoUtils.RubberbandApproach(a.OrbitRateMult, 0, 0.05f);
                a.Sprite.Distance = Calc.Approach(a.Sprite.Distance, 1, speed * Engine.DeltaTime);
                speed = Calc.Approach(speed, 200f, 400f * Engine.DeltaTime);
                yield return null;
            }
            foreach (var v in a.Sprite.ActiveOrbs) v.ShakeFor(1);
            yield return 1;
            a.Wiggle();
            while (a.Sprite.Distance != 20 || a.OrbitRateMult != 1)
            {
                a.OrbitRateMult = PianoUtils.RubberbandApproach(a.OrbitRateMult, 1, 0.05f);
                float speedMult = 1;
                if (a.Sprite.Distance >= 10)
                {
                    speedMult = 1 - ((a.Sprite.Distance - 10) / 10 * 0.7f);
                }
                a.Sprite.Distance = Calc.Approach(a.Sprite.Distance, 20, 320f * Engine.DeltaTime * speedMult);
                yield return null;
            }
        }
        private bool Huddled;
        public void BeginHuddle(SingularityActor actor, float interval)
        {
            Huddled = true;
            float delay = 0;
            foreach (var a in actor.Sprite.ActiveOrbs)
            {
                beginSpawnOrbHuddleAfterImageLoop(actor, a, interval, delay);
                delay += interval / actor.Sprite.ActiveOrbs.Count;
            }
        }
        private void beginSpawnOrbHuddleAfterImageLoop(SingularityActor actor, SingularityOrb orb, float interval, float delay)
        {
            if (delay > 0)
            {
                Alarm.Set(this, delay, () => spawnOrbHuddledAfterImage(actor, orb, interval));
            }
            else
            {
                spawnOrbHuddledAfterImage(actor, orb, interval);
            }
        }
        private void spawnOrbHuddledAfterImage(SingularityActor actor, SingularityOrb orb, float interval)
        {
            if (!Huddled) return;
            orb.CreateAfterImage(actor.AfterImages, 0.5f, 0.5f, 1f, 1f, 3f, Calc.Random.ShakeVector());
            if (Huddled)
            {
                Alarm.Set(this, interval, () => spawnOrbHuddledAfterImage(actor, orb, interval));
            }
        }
        private void wiggleSingularity()
        {
            foreach (var v in Singularity.Sprite.ActiveOrbs)
            {
                v.Wiggle();
            }
        }
        private IEnumerator spiralRoutine()
        {
            Add(new Coroutine(PianoUtils.Lerp(Ease.SineInOut, 1.5f,
                (f) => spiralRadius = maxSpiralRadius * f, true)));
            yield return PianoUtils.Lerp(Ease.SineOut, 1f, (f) => spiralSpeed = -120 * f, true);
            yield return 0.8f;
            yield return PianoUtils.Lerp(Ease.SineInOut, 1f, (f) => spiralSpeed = Calc.LerpClamp(-120f, -80f, f), true);
            yield return 1;
            Add(new Coroutine(slingshotRoutine()));


            float from = Singularity.Sprite.Distance;
            wiggleSingularity();
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)//0.6f)
            {
                Singularity.Sprite.Distance = Calc.LerpClamp(from, from + 16, Ease.CubeOut(i));
                yield return null;
            }
            yield return 0.1f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)//0.7f)
            {
                cameraOffset.Y = Calc.LerpClamp(4, -70, Ease.CubeOut(i));
                Singularity.Sprite.Distance = Calc.LerpClamp(from + 16, 1, Ease.SineIn(i));
                yield return null;
            }
            foreach (var v in Singularity.Sprite.ActiveOrbs)
            {
                v.ShakeFor(1f);
            }
            yield return 1f;
            foreach (var v in Singularity.Sprite.ActiveOrbs)
            {
                v.Wiggle();
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)// 0.3f)
            {
                cameraOffset.Y = Calc.LerpClamp(-70, -30, Ease.CubeOut(i));
                Singularity.Sprite.Distance = Calc.LerpClamp(1, 20f, Math.Min(1, Ease.CubeOut(i * 2)));
                yield return null;
            }


            yield return new SwapImmediately(PianoUtils.Lerp(Ease.Linear, 1f,
                (f) => spiralRadius = maxSpiralRadius * (1 - f)));


            /*


                        //spiral start
                        for (float i = 0; i < 1; i += Engine.DeltaTime)
                        {
                            float e = Ease.SineOut(i);

                            spiralWaveLength = Calc.LerpClamp(120, 80f, e);
                            spiralSpeed = -120f * e;
                            spiralRadius = maxSpiralRadius * 0.7f * e;
                            yield return null;
                        }
                        yield return 0.8f;
                        for (float i = 0; i < 1; i += Engine.DeltaTime)
                        {
                            float ease = Ease.SineInOut(i);
                            spiralSpeed = Calc.LerpClamp(-120f, -70f, ease);
                            spiralRadius = Calc.LerpClamp(maxSpiralRadius * 0.7f, maxSpiralRadius, ease);
                            yield return null;
                        }

                        //spiral end
                        yield return PianoUtils.Lerp(Ease.SineInOut, 1f, f =>
                        {
                            cameraOffset.Y = Calc.LerpClamp(4, -60, f);
                            spiralRadius = Calc.LerpClamp(maxSpiralRadius, 0f, f);
                        }, true);*/
        }
        private IEnumerator slingshotRoutine()
        {
            float from = Singularity.Sprite.Distance;
            foreach (var v in Singularity.Sprite.ActiveOrbs)
            {
                v.Wiggle();
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)//0.6f)
            {
                Singularity.Sprite.Distance = Calc.LerpClamp(from, from + 16, Ease.CubeOut(i));
                yield return null;
            }
            yield return 0.1f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)//0.7f)
            {
                cameraOffset.Y = Calc.LerpClamp(4, -70, Ease.CubeOut(i));
                Singularity.Sprite.Distance = Calc.LerpClamp(from + 16, 1, Ease.SineIn(i));
                yield return null;
            }
            foreach (var v in Singularity.Sprite.ActiveOrbs)
            {
                v.ShakeFor(1f);
            }
            yield return 1f;
            foreach (var v in Singularity.Sprite.ActiveOrbs)
            {
                v.Wiggle();
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)// 0.3f)
            {
                cameraOffset.Y = Calc.LerpClamp(-70, -30, Ease.CubeOut(i));
                Singularity.Sprite.Distance = Calc.LerpClamp(1, 20f, Math.Min(1, Ease.CubeOut(i * 2)));
                yield return null;
            }
            Add(new Coroutine(orbDistanceLoopRoutine()));
        }
        public IEnumerator Cutscene()
        {
            cameraOffset = new Vector2(0f, -20f);
            Singularity.Active = true;
            Player.EnforceLevelBounds = true;
            yield return null;
            //routine for orbs swirling around player
            //Singularity.Sprite.Distance = 36;
            Singularity.SpawnAfterImages = true;
            Singularity.AfterImageSpeed = Vector2.UnitY * 120f;
            #region Fly
            cameraOffset.Y = 4;
            //Level.Camera.Y = Singularity.Y - 90f;
            spiralStart = Singularity.Position;
            inSpiral = true;

            spiralWaveLength = 80f;

            yield return new SwapImmediately(spiralRoutine());
            inSpiral = false;
            Singularity.MoveToX(spiralStart.X);
            Player.MoveToX(spiralStart.X);
            //Barrier.VertexOffset.Y = from + dist;
            //barrier.Shake (todo: add this)
            //(todo: add player shaker)
            yield return 1f;
            float from = Barrier.VertexOffset.Y;
            float dist = Player.Y - Level.Camera.Y;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Barrier.Pitch = Calc.LerpClamp(90f, 65f, Ease.SineIn(i)).ToRad();
                barrierVertexOffset.Y = dist * Ease.SineIn(i);
                yield return null;
            }
            yield break;
            Singularity.StartShaking(-1);
            //ALL BELOW IS WIP AND DEPENDS ON THE BARRIER EXISTING
            orbCoroutine.Replace(orbQuicktimeRoutine());
            Singularity.Sprite.AutoHandleOrbit = false;
            Singularity.Sprite.AutoHandlePositions = true;
            float[] targets = [MathHelper.Pi, MathHelper.PiOver2, 0];
            for (int i = 0; i < Singularity.Sprite.ActiveOrbs.Count; i++)
            {
                Add(new Coroutine(orbAngleRoutine(i, targets[i], 1)));
            }
            #endregion
            //hit concave surface, quicktime event to break it
            //on each input set completed, shake the screen and barrier, also freeze


            for (int i = 0; i < 3; i++)
            {
                Action onPress = () =>
                {
                    Level.Shake(0.1f);
                    //spawn glass shards
                };
                QuickAction action = new QuickAction((QuickAction.Presets)i, onPress, 8, 1f, -0.7f);
                Scene.Add(action);
                while (!action.Finished)
                {
                    yield return null;
                }
                Barrier.CreateVertices(Barrier.WindowSize - 8);
            }

            Barrier.StartShakingBulge();
            for (int i = 0; i < 4; i++)
            {
                Action onPress = () =>
                {
                    Level.Shake(0.1f);
                    //spawn glass shards
                };
                QuickAction action = new QuickAction((QuickAction.Presets)i, onPress, 8, 1f, -0.7f);
                Scene.Add(action);
                while (!action.Finished)
                {
                    yield return null;
                }
                pushSpeed = 200f;
            }
            Singularity.StopShaking();
            Singularity.TimeMod.Multiplier = 0.2f;
            barrierSpeed = 40f;
            yield return 0.4f;
            Singularity.TimeMod.Multiplier = 1;
            barrierSpeed *= 5f;


            //both fly past it, revealing top of tower behind glass
            //singularity enters eye, maddy is launched slightly above it and begins floating as minigame starts
        }
        public IEnumerator StreakRoutine()
        {
            Level.Add(streaks = new AscendManager.Streaks(null));
            float p2;
            for (p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 12f)
            {
                //fadeToWhite = p2;
                streaks.Alpha = p2;
                foreach (Parallax item in Level.Foreground.GetEach<Parallax>("blackhole"))
                {
                    item.FadeAlphaMultiplier = 1f - p2;
                }
                yield return null;
            }
        }
        private List<ShiftArea> areaTracker = [];
        public void SpawnShiftAreas(float maxLength, char bgfrom = 'R', char bgto = 'Y', char fgfrom = 'W', char fgto = 'Z')
        {
            Vector2 start = new Vector2(Level.Bounds.X, Level.Camera.Y - maxLength);
            Vector2 end = new Vector2(Level.Bounds.X + 320, Level.Camera.Y - maxLength);

            while (start != end)
            {
                float length = Calc.Random.Range(maxLength * 0.5f, maxLength);
                Vector2[] nodes = new Vector2[3];
                float angleOffset = Calc.Random.NextAngle();
                for (int i = 0; i < 3; i++)
                {
                    nodes[i] = start + Calc.AngleToVector(MathHelper.TwoPi / 3f * i + angleOffset, length);
                }

                ShiftArea shiftArea = new ShiftArea(start - Level.Bounds.Location.ToVector2(), Level.Bounds.Location.ToVector2(), bgfrom, bgto, fgfrom, fgto, nodes, [0, 1, 2]);
                shiftArea.Speed = Vector2.UnitY * Calc.Random.Range(100, 120f);
                shiftArea.RotationRate = Calc.Random.Range(1f, 5f) * Engine.DeltaTime * Calc.Random.Sign();
                Scene.Add(shiftArea);
                areaTracker.Add(shiftArea);
            }
        }

        private List<ShiftArea> areasToRemove = [];
        private void manageAreas()
        {
            foreach (var s in areaTracker)
            {
                bool removeArea = true;
                foreach (var v in s.Vertices)
                {
                    Vector2 p = v.Position.XY();
                    if (Level.IsInCamera(p, 0))
                    {
                        removeArea = false;
                        break;
                    }
                }
                if (removeArea)
                {
                    areasToRemove.Add(s);
                }
            }
            foreach (var s in areasToRemove)
            {
                s.RemoveSelf();
                areaTracker.Remove(s);
            }
            areaTracker.Clear();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Barrier?.RemoveSelf();
        }
    }
}
