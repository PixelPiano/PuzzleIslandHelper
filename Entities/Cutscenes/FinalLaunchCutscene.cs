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
            public float Alpha = 1;
            public float ColorLerp;
            public float ScaleMult = 1;
            public float Decay;
            public float Increment;
            private float slider;
            private float max;
            public float Percent => slider / max;
            private Action onPress;
            private Action<float> onUpdate;
            private Coroutine pressCoroutine;
            public bool Finished;
            private VirtualButton button;
            private Color color;
            private bool intro = true;
            private float currentAmount = 1;
            public QuickAction(Presets preset, Action onPress, Action<float> onUpdate, float max, float increment, float decay) : base()
            {
                this.onUpdate = onUpdate;
                this.max = max;
                this.onPress = onPress;
                Increment = increment;
                Decay = decay;
                Tag |= TagsExt.SubHUD;

                Add(pressCoroutine = new Coroutine(false));
                button = preset switch
                {
                    Presets.Red => Input.MenuLeft,
                    Presets.Green => Input.MenuDown,
                    Presets.Blue => Input.MenuRight,
                    Presets.Alpha => Input.Dash,
                };
                color = preset switch
                {
                    Presets.Red => Color.Red,
                    Presets.Green => Color.Green,
                    Presets.Blue => Color.Blue,
                    _ => Color.White
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
                Color color = Color.Lerp(this.color, ColorLerp < 0 ? Color.Black : Color.White, Math.Abs(ColorLerp));
                if (Input.GuiInputController(Input.PrefixMode.Latest))
                {
                    Input.GuiButton(button, Input.PrefixMode.Latest).DrawJustified(position, new Vector2(0.5f), color * Alpha, 6 * ScaleMult);
                }
                else
                {
                    ActiveFont.DrawOutline(Input.FirstKey(button).ToString().ToUpper(), position, new Vector2(0.5f), new Vector2(6 * ScaleMult), color * Alpha, 3f + (2 * -(1 - ScaleMult)), Color.Black * Alpha);
                }
            }
            public override void Update()
            {
                base.Update();
                if (!Finished && !intro)
                {
                    slider = Math.Max(0, slider + Decay * Engine.DeltaTime);
                    if (button.Pressed)
                    {
                        button.ConsumePress();
                        Press();
                    }
                    if (onUpdate != null)
                    {
                        onUpdate.Invoke(Percent);
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
            public void SetAmount(float amount)
            {
                currentAmount = amount = Math.Clamp(amount, 0, 2);
                if (amount >= 1)
                {
                    ScaleMult = Calc.LerpClamp(1f, 1.2f, amount - 1);
                    ColorLerp = Calc.LerpClamp(0, 0.5f, amount - 1);
                }
                else
                {
                    ScaleMult = Calc.LerpClamp(1f, 0.7f, 1 - amount);
                    ColorLerp = Calc.LerpClamp(0, -0.5f, 1 - amount);
                }
            }
            private IEnumerator pressRoutine(bool final)
            {
                SetAmount(2);
                yield return null;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.05f)
                {
                    SetAmount(Ease.CubeIn(i));
                    yield return null;
                }

                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    SetAmount(1 + Ease.CubeOut(i));
                    yield return null;
                }
                Ease.Easer e = Ease.Follow(Ease.CubeIn, Ease.SineOut);
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
                {
                    SetAmount(2 - e(i));
                    if (final)
                    {
                        Alpha = 1 - Ease.SineIn(i);
                    }
                    yield return null;
                }
                if (final)
                {
                    RemoveSelf();
                }
            }
        }
        public Player Player;
        public SingularityActor Singularity;
        public Tower.Eye Eye;
        public Vector2 GroundPosition;

        public Vector2 cameraWaveOffset;
        public Vector2 cameraOffset;
        public Vector2 cameraPosition;

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
            distanceLoopTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 0.5f);
            distanceLoopTween.OnUpdate = t =>
            {
                Singularity.Sprite.Distance = Calc.LerpClamp(20f, endDist, t.Eased);
                if (endDist != 36f)
                {
                    t.Duration = Math.Max(t.Duration * 0.97f, 0.1f);
                    if (Singularity.Sprite.Distance == endDist)
                    {
                        t.Stop();
                    }
                }
            };
            Add(distanceLoopTween);
        }

        private float barrierPitchLerp;
        private float barrierYOffset;
        public override void OnBegin(Level level)
        {
            //pattern = level.Tracker.GetEntity<AscendPattern>();
            /*            if (pattern != null)
                        {
                            //pattern.SwapClipMode();
                        }*/
            Barrier = new AscendBarrier(level.Camera.Position);
            level.Add(Barrier);
            singularityOrig = Singularity.Position;
            Barrier.Pitch = Barrier.PitchFrom;
            Barrier.VertexOffset.Y = -93;
            barrierPitchLerp = 0;
            barrierYOffset = 0;
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
            Tween.Set(this, Tween.TweenMode.Looping, 1, Ease.Linear, t => Barrier.Pitch = MathHelper.Pi * t.Eased);
            Add(new Coroutine(Cutscene()));
        }
        private Vector2 singularityOrig;
        private bool applyCharacterOffset;
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
                float y = Singularity.Y;
                Player.MoveToX(x);
                //pattern.TotalDistance += spiralSpeed * 2 * Engine.DeltaTime;
            }
            else
            {
                //pattern.TotalDistance += pushSpeed * Engine.DeltaTime;
            }
            if (applyCharacterOffset)
            {
                Player.MoveToY(singularityOrig.Y + characterYOffset);
                Singularity.MoveToY(singularityOrig.Y + characterYOffset);
            }
            if (Barrier != null)
            {
                Barrier.VertexOffset.Y = -93 + barrierYOffset - Player.Height;
                Barrier.Pitch = Calc.LerpClamp(Barrier.PitchFrom, Barrier.PitchTo, barrierPitchLerp);
                Barrier.Push = Math.Abs(characterYOffset);
            }
            Singularity.Orbit += (spiralSpeed * Engine.DeltaTime / 10f);
        }
        public override void OnEnd(Level level)
        {

        }
        public void ReloadBarrier(float shake, int size)
        {
            Level.Shake(shake);
            Barrier.CreateVertices(size);
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

        private IEnumerator orbQuicktimeRoutine()
        {
            while (!PianoUtils.TryRubberbandApproach(Singularity.Sprite.Distance, 20, out float output, 1))
            {
                Singularity.Sprite.Distance = output;
                yield return null;
            }
            Singularity.Sprite.Distance = 20;
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
            yield return new SwapImmediately(PianoUtils.Lerp(Ease.Linear, 1f,
                (f) => spiralRadius = maxSpiralRadius * (1 - f), true));
        }

        private IEnumerator slingshotRoutine()
        {
            float speed = 100f;
            Singularity.Wiggle();
            Singularity.OrbitRateMult = 2;
            while (speed != 0)
            {
                Singularity.Sprite.Distance += speed * Engine.DeltaTime;
                speed = Calc.Approach(speed, 0, 600f * Engine.DeltaTime);
                yield return null;
            }
            yield return 0.05f;
            float from = Singularity.Sprite.Distance - 1;
            while (Singularity.Sprite.Distance != 1)
            {
                Singularity.OrbitRateMult = PianoUtils.RubberbandApproach(Singularity.OrbitRateMult, 0, 0.05f);
                Singularity.Sprite.Distance = Calc.Approach(Singularity.Sprite.Distance, 1, speed * Engine.DeltaTime);
                float dist = Singularity.Sprite.Distance - 1;
                cameraOffset.Y = Calc.LerpClamp(4, -70, Ease.CubeOut(1 - (dist / from)));
                speed = Calc.Approach(speed, 200f, 400f * Engine.DeltaTime);
                yield return null;
            }
            cameraOffset.Y = -70;
            foreach (var v in Singularity.Sprite.ActiveOrbs) v.ShakeFor(1);
            yield return 1;
            Singularity.Wiggle();
            Singularity.Sprite.Pitch = MathHelper.PiOver4;
            while (Singularity.Sprite.Distance != 20 || Singularity.OrbitRateMult != 1)
            {
                Singularity.OrbitRateMult = PianoUtils.RubberbandApproach(Singularity.OrbitRateMult, 1, 0.05f);
                float speedMult = 1;
                if (Singularity.Sprite.Distance >= 10)
                {
                    speedMult = 1 - ((Singularity.Sprite.Distance - 10) / 10 * 0.7f);
                }
                Singularity.Sprite.Distance = Calc.Approach(Singularity.Sprite.Distance, 20, 320f * Engine.DeltaTime * speedMult);
                cameraOffset.Y = Calc.LerpClamp(-70, -30, Ease.CubeOut(1 - speedMult));
                yield return null;
            }
            cameraOffset.Y = -30;
            //distanceLoopTween.Start();
        }
        private float endDist = 36;
        private float angleVelocity;
        private Tween distanceLoopTween;
        private float characterYOffset;
        public void ApplyZWobble(float chance, float duration, float frequency)
        {
            foreach (AscendBarrier.Window w in Barrier.Windows)
            {
                if (Calc.Random.Chance(chance))
                {
                    w.Wobble(this, duration, frequency);
                }
            }
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

            //yield return new SwapImmediately(spiralRoutine());
            inSpiral = false;
            Singularity.MoveToX(spiralStart.X);
            Player.MoveToX(spiralStart.X);
            //barrier.Shake (todo: add this)
            //(todo: add player shaker)
            yield return 0.5f;
            //yield return 1f;
            float dist = Player.Y - Level.Camera.Y;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                barrierPitchLerp = Ease.CubeIn(i);
                barrierYOffset = 93 * Ease.CubeIn(i);
                yield return null;
            }
            barrierPitchLerp = 1;
            barrierYOffset = 93;
            Singularity.StartShakingSeperate(-1);
            //ALL BELOW IS WIP AND DEPENDS ON THE BARRIER EXISTING
            orbCoroutine.Replace(orbQuicktimeRoutine());
            Singularity.Sprite.AutoHandlePositions = true;
            float[] targets = [MathHelper.Pi, MathHelper.PiOver2, 0];
            for (int i = 0; i < Singularity.Sprite.ActiveOrbs.Count; i++)
            {
                Add(new Coroutine(orbAngleRoutine(i, targets[i], 1)));
            }

            #endregion
            //hit concave surface, quicktime event to break it
            //on each input set completed, shake the screen and barrier, also freeze
            applyCharacterOffset = true;
            Player.ForceCameraUpdate = false;
            int[] sizes = [64, 44, 24];
            Action onPress = () =>
            {
                Level.Shake(0.1f);
                //spawn glass shards
            };
            Action<float> onUpdate = (f) =>
            {

            };
            for (int i = 0; i < 3; i++)
            {

                QuickAction action = new QuickAction((QuickAction.Presets)i, onPress, onUpdate, 8, 1f, -0.7f);
                Scene.Add(action);
                while (!action.Finished)
                {
                    yield return null;
                }
                ReloadBarrier(0.3f, sizes[i]);
                ApplyZWobble(0.1f + i * 0.2f, 0.15f, 3);
            }
            for (int i = 0; i < 4; i++)
            {
                QuickAction action = new QuickAction((QuickAction.Presets)i, onPress, onUpdate, 8, 1f, -0.7f);
                Scene.Add(action);
                while (!action.Finished)
                {
                    yield return null;
                }
                float from = characterYOffset;
                for (float l = 0; l < 1; l += Engine.DeltaTime / 0.1f)
                {
                    characterYOffset = Calc.LerpClamp(from, from - 8, Ease.CubeIn(l));
                    yield return null;
                }
                characterYOffset = from - 8;
                Barrier.StartShakingBulge();
            }
            Barrier.Shatter();
            while (!Barrier.Shattered) yield return null;
            Singularity.StopShaking();
            Barrier.StopShakingBulge();
            SetSpeedForFallingWindows(40f);
            Singularity.TimeMod.Multiplier = 0.2f;
            yield return 0.4f;
            Singularity.TimeMod.Multiplier = 1;
            SetSpeedForFallingWindows(80f);

            //both fly past it, revealing top of tower behind glass
            //singularity enters eye, maddy is launched slightly above it and begins floating as minigame starts
        }
        public void SetSpeedForFallingWindows(float ySpeed)
        {
            foreach (AscendBarrier.Window window in Barrier.Windows)
            {
                if (!window.Detached)
                {
                    window.Speed.Y = ySpeed;
                }
            }
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
