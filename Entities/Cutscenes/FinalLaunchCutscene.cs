using Celeste.Mod.Entities;
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
using static Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs.Osc;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    public class EndingA : CutsceneEntity
    {
        public Player Player;
        public SingularityActor Singularity;
        public Tower.Eye Eye;
        public Vector2 GroundPosition;

        //lovingly copied from CS10_FinalLaunch because brain bad atm
        public Vector2 cameraWaveOffset;
        public Vector2 cameraOffset;
        public Vector2 cameraPosition;
        public Coroutine wave, spiral, launch, orb;
        private bool flying;

        public bool OrbSineLooping;
        public bool OrbSineActive;
        public float OrbSineEndDistance = 20;
        private float OrbSineTimerMult = 2;

        private Vector2 spiralStart;
        private float spiralYOffset;
        private float spiralRadius;
        private float spiralWaveLength = 120;
        private float spiralSpeed;
        private bool inSpiral;
        private AscendPattern pattern;
        public AscendManager.Streaks streaks;

        public EndingA(Player player, SingularityActor singularity)
        {
            Player = player;
            Singularity = singularity;
        }

        public override void OnBegin(Level level)
        {
            pattern = level.Tracker.GetEntity<AscendPattern>();
            if (pattern != null)
            {
                pattern.SwapClipMode();
            }
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

        private IEnumerator orbRoutine()
        {
            float from = Singularity.Sprite.Distance;
            OrbSineActive = true;
            OrbSineLooping = true;
            while (OrbSineLooping)
            {
                if (from > 20)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime * OrbSineTimerMult)
                    {
                        Singularity.Sprite.Distance = Calc.LerpClamp(from, 20, Ease.SineInOut(i));
                        yield return null;
                    }
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * OrbSineTimerMult)
                {
                    Singularity.Sprite.Distance = Calc.LerpClamp(20, 36, Ease.SineInOut(i));
                    yield return null;
                }
                from = 36;
                yield return null;
            }
            if (Singularity.Sprite.Distance != OrbSineEndDistance)
            {
                from = Singularity.Sprite.Distance;
                for (float i = 0; i < 1; i += Engine.DeltaTime * OrbSineTimerMult)
                {
                    Singularity.Sprite.Distance = Calc.LerpClamp(from, OrbSineEndDistance, Ease.SineInOut(i));
                    yield return null;
                }
                Singularity.Sprite.Distance = OrbSineEndDistance;
            }
            OrbSineActive = false;
        }
        private IEnumerator spiralStartRoutine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                float e = Ease.SineOut(i);
                spiralWaveLength = Calc.LerpClamp(120, 80f, e);
                spiralSpeed = -120f * e;
                spiralRadius = 40f * e;
                yield return null;
            }
            yield return 0.8f;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                spiralSpeed = Calc.LerpClamp(-120f, -70f, i);
                spiralRadius = Calc.LerpClamp(40f, 70f, i);
                yield return null;
            }
        }

        private IEnumerator spiralEndRoutine()
        {
            yield return PianoUtils.Lerp(Ease.SineInOut, 1f, f =>
            {
                cameraOffset.Y = Calc.LerpClamp(4, -60, f);
                spiralRadius = Calc.LerpClamp(70f, 0f, f);
            }, true);

            yield return PianoUtils.Lerp(Ease.BackIn, 1f, f =>
            {
                cameraOffset.Y = Calc.LerpClamp(-60, 45f, f);
            });
        }
        public IEnumerator Cutscene()
        {
            cameraOffset = new Vector2(0f, -20f);
            Singularity.Active = true;
            Player.EnforceLevelBounds = true;
            yield return null;
            Add(orb = new Coroutine(orbRoutine()));
            Singularity.Sprite.Distance = 36;
            Singularity.SpawnAfterImages = true;
            Singularity.AfterImageSpeed = Vector2.UnitY * 120f;
            #region Fly
            cameraOffset.Y = 4;
            //Level.Camera.Y = Singularity.Y - 90f;
            spiralStart = Singularity.Position;
            inSpiral = true;
            yield return new SwapImmediately(spiralStartRoutine());



            yield return new SwapImmediately(spiralEndRoutine());
            inSpiral = false;
            Singularity.MoveToX(spiralStart.X);
            Player.MoveToX(spiralStart.X);

            OrbSineLooping = false;
            while (OrbSineActive)
            {
                OrbSineTimerMult += Engine.DeltaTime * 5;
                yield return null;
            }
            #endregion

            Singularity.StartShaking(-1);
            //hit concave surface, quicktime event to break it

            //Glass shakes and cracks form
            Singularity.StopShaking();
            Singularity.TimeMod.Multiplier = 0.2f;
            yield return 0.4f;
            Singularity.TimeMod.Multiplier = 1;
            //singularity.Position = Calc.Approach(singularity.Position, Eye.Center);
            //Player.Position = singularity.Position + Vector2.UnitY * 24;


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
        public override void Update()
        {
            base.Update();
            manageAreas();
            Level.CameraOffset = cameraOffset + cameraWaveOffset;
            if (inSpiral)
            {
                Singularity.MoveToX(spiralStart.X + (float)(double)(spiralRadius * Math.Sin((spiralStart.Y + spiralYOffset) * Math.PI / spiralWaveLength)));

                Player.MoveToX(Singularity.X);
                Player.MoveToY(Singularity.Y);
                spiralYOffset += spiralSpeed * Engine.DeltaTime;
            }
            Singularity.Orbit += (spiralSpeed * Engine.DeltaTime / 10f);
            pattern.TotalDistance += spiralSpeed * 2 * Engine.DeltaTime;
        }
        public override void OnEnd(Level level)
        {

        }
    }
}
