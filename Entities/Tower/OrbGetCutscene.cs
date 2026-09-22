using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    [Tracked]
    public class OrbGetCutscene : CutsceneEntity
    {
        private bool advancePortalAtEnd = true;
        public MemoryOrb Orb;
        public Player Player;
        public Eye Eye;
        public OrbGetCutscene(Eye eye, Player player) : base(false, false)
        {
            Eye = eye;
            Orb = eye.Orb;
            Player = player;
        }
        public override void OnBegin(Level level)
        {
            Player.DisableMovement();
            Add(new Coroutine(Routine()));
        }
        public IEnumerator Routine()
        {
            List<VertexOrb.Shiver> shivers = [];
            float maxIntensity = 6;
            float minIntensity = 2;
            void updateShiverIntensities(float prevRadius)
            {
                foreach (VertexOrb.Shiver shiver in shivers)
                {
                    float factor = Orb.Orb.Radius / prevRadius;
                    shiver.TargetMinIntensity = minIntensity * factor;
                    shiver.TargetMaxIntensity = maxIntensity * factor;
                }
            }
            ////////IF: 3d model of eye is feasible
            //t-blocks activate
            //eye is spinning, eventually settling on the flat side pointing to the camera
            //everything stops, a ring of colored light expands from the center of the eye before collapsing back into the orb, giving it color.
            //the orb floats out of the eye
            //yield return 0.4f;
            //eye shakes
            ////////

            yield return Player.DummyWalkTo(Eye.CenterX);
            //orb flies up
            Orb.ShakeFor(1f);
            yield return 0.7f;
            Orb.Orb.FlashColorMod(Color.White, 0.025f, 0f, 0);
            yield return 0.05f;
            Color fillInvert = Orb.Color.Invert();
            Color centerInvert = Orb.CenterColor.Invert();
            Color edgeInvert = Orb.EdgeColor.Invert();
            Orb.Orb.FlashColorMod(centerInvert, fillInvert, edgeInvert, 0.05f, 0.05f, 2);
            yield return 0.2f;
            Orb.Orb.ColorB = fillInvert;
            Orb.Orb.CenterColorB = centerInvert;
            Orb.Orb.EdgeColorB = edgeInvert;
            Orb.ShakeMult = Vector2.Zero;
            Add(new Coroutine(PianoUtils.Lerp(Ease.CubeOut, 1, f => Orb.Y = Calc.LerpClamp(Eye.CenterY, Eye.CenterY - (Player.TopCenter - Eye.Center).Length(), f), true)));
            yield return 0.5f;
            Tween.Set(Orb, Tween.TweenMode.Oneshot, 1f, Ease.SineIn, t =>
            {
                Orb.Orb.FillColorLerp = Orb.Orb.EdgeColorLerp = Orb.Orb.CenterColorLerp = 1 - t.Eased;
            }, t => { Orb.Orb.FillColorLerp = Orb.Orb.EdgeColorLerp = Orb.Orb.CenterColorLerp = 0; });
            yield return 1.6f;


            //orb circles around to right in front of maddy, maddy steps back
            const float spaceFromCenter = 10;
            Vector2 orbStartPosition = Orb.Position - Eye.Center;
            float angle = MathHelper.Pi + MathHelper.PiOver2;
            float targetAngle = MathHelper.TwoPi + MathHelper.PiOver2;
            float radius = (Orb.Position - Eye.Center).Length();
            bool movedBack = false;
            //maddy learns to backup earlier as she experiences meeting new orbs
            float awareness = Eye.gate.State == Portal.StOneOrb ? 0.5f : 0.2f;
            //overshoot
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
            {
                float nextAngle = angle + (targetAngle - angle) * Ease.SineInOut(i);
                Orb.Position = Eye.Center + Calc.AngleToVector(nextAngle, radius);
                Orb.SineDist = Calc.LerpClamp(4, 2, Ease.SineInOut(i));
                if (i > (1 - awareness) && !movedBack)
                {
                    Add(new Coroutine(Player.DummyWalkTo(Player.X - spaceFromCenter, true, 2)));
                    movedBack = true;
                }
                yield return null;
            }
            //orb backs away a little
            yield return null;
            Vector2 from = Orb.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                Orb.Position = from + Vector2.UnitX * 6 * Ease.SineInOut(i);
                yield return null;
            }
            yield return 2.4f;
            Vector2 orbTarget = Player.TopRight + Vector2.UnitX * (Orb.Orb.Radius + 3);
            Vector2 orbFrom = Orb.Position;
            //if first time, maddy approaches. otherwise, she lets the orb come to her by itself
            if (Eye.gate.State < Portal.StOneOrb)
            {
                //maddy inches forwards
                Add(new Coroutine(Player.DummyWalkTo(Player.CenterX + 12, false, 0.7f)));
                yield return 0.2f;
                Orb.ShakeMult = Vector2.UnitX;
                Orb.ShakeFor(0.3f);
                Orb.AddShiver(0.6f, 0, 8, 0, MathHelper.Pi, Ease.SineInOut, Tween.TweenMode.Oneshot, 0.2f);
                float xFrom = Orb.X;
                float yFrom = Orb.Y;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
                {
                    Orb.MoveToY(Calc.LerpClamp(yFrom, yFrom - 16, Ease.SineOut(i)));
                    Orb.MoveToX(Calc.LerpClamp(xFrom, xFrom + 24, Ease.SineOut(i)));
                    yield return null;
                }
                //maddy hesitates
                yield return 1;

                Coroutine moveRoutine;
                Add(moveRoutine = new Coroutine(Player.DummyWalkTo(Player.X - 8, true, 0.2f)));
                while (!moveRoutine.Finished)
                {
                    Player.Sprite.Rate = 0.3f;
                    yield return null;
                }
                Player.Sprite.Rate = 1;
                yield return 0.4f;
                orbFrom = Orb.Position;
                //orb hesitantly approaches maddy
                for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
                {
                    Orb.MoveToX(Calc.LerpClamp(orbFrom.X, orbTarget.X, Ease.SineInOut(i * 0.3f)));
                    Orb.MoveToY(Calc.LerpClamp(orbFrom.Y, orbTarget.Y, Ease.SineInOut(i * 0.3f)));
                    yield return null;
                }
                //orb pauses
                orbFrom = Orb.Position;
                yield return 0.6f;
            }
            int prevOrbDepth = Orb.Depth;

            //orb fully approaches maddy
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                Orb.MoveToX(Calc.LerpClamp(orbFrom.X, orbTarget.X, Ease.SineInOut(i)));
                Orb.MoveToY(Calc.LerpClamp(orbFrom.Y, orbTarget.Y, Ease.SineOut(i)));
                yield return null;
            }
            yield return 0.5f;
            //orb flutters twice
            Orb.AddShiver(0.8f, 0, 6, 2, MathHelper.PiOver2, Ease.CubeIn, Tween.TweenMode.Oneshot, 0.1f);
            yield return 0.1f;
            Orb.PulseColorMod(0.2f, 0.2f, Ease.CubeIn, Ease.SineInOut, 0, (c, i, f) =>
            {
                return Color.Lerp(c, Color.White, f * 0.5f);
            });
            yield return 0.8f;
            Orb.AddShiver(0.8f, 0, 6, 2, MathHelper.PiOver2, Ease.CubeIn, Tween.TweenMode.Oneshot, 0.1f);
            yield return 0.1f;
            Orb.PulseColorMod(0.2f, 0.2f, Ease.CubeIn, Ease.SineInOut, 0, (c, i, f) =>
            {
                return Color.Lerp(c, Color.White, f * 0.5f);
            });
            yield return 1;
            //orb moves to exact position of maddy
            orbFrom = Orb.Position;
            orbTarget = Player.Center;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Vector2 pos = Vector2.Lerp(orbFrom, orbTarget, Ease.SineInOut(i));
                Orb.MoveToX(pos.X);
                Orb.MoveToY(pos.Y);
                yield return null;
            }
            for (int i = 0; i < 4; i++)
            {
                shivers.Add(Orb.AddShiver(1f, 0, maxIntensity, minIntensity, MathHelper.PiOver2, Ease.Linear, Tween.TweenMode.Looping, 0.3f, Ease.ExpoInOut));
                yield return 0.25f;
            }

            bool playerFloating = false;
            float floatThresh = MathHelper.Distance(Orb.Y, Player.Bottom + 4);
            float prevRadius = Orb.Orb.Radius;
            yield return OrbGrow(Orb, 260, f =>
            {
                updateShiverIntensities(prevRadius);
                if (f * 200f > floatThresh && !playerFloating)
                {
                    playerFloating = true;
                    Player.DummyAutoAnimate = false;
                    Player.Sprite.Play(PlayerSprite.FallSlow);
                    Player.MuffleLanding = true;
                    Player.Collidable = false;
                }
            }, () =>
            {
                Orb.Depth = int.MinValue + 3;
                Player.Depth = int.MinValue + 2;
            });
            updateShiverIntensities(prevRadius);
            //heavenlytext sequence
            ///HeavenlyText t = new();
            ///yield return t.Sequence();
            ///t.RemoveSelf();

            //orb shrinks back to regular size
            yield return new SwapImmediately(OrbShrink(Orb, prevRadius, f =>
            {
                updateShiverIntensities(prevRadius);
                if ((1 - f) * 200f < floatThresh && playerFloating)
                {
                    playerFloating = true;
                    Player.DummyAutoAnimate = true;
                    Player.MuffleLanding = false;
                    Player.Collidable = true;
                }
            }));
            updateShiverIntensities(prevRadius);
            Orb.Depth = prevOrbDepth;
            Player.Depth = 0;
            //remains of eye collapse into itself
            //yield return Eye.CollapseIntoPortal();
            //portal updates and orb flies into it
            yield return Eye.AdvancePortalState();
            advancePortalAtEnd = false;
            EndCutscene(Level);
        }
        public IEnumerator OrbGrow(MemoryOrb orb, float targetRadius, Action<float> onGrowUpdate, Action onShrinkEnd = null)
        {
            float prevSineDist = orb.SineDist;
            float prevRadius = orb.Orb.Radius;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                orb.SineDist = Calc.LerpClamp(prevSineDist, 0, Ease.SineInOut(i));
                orb.Orb.Radius = Calc.LerpClamp(prevRadius, prevRadius * 0.7f, Ease.SineIn(i));
                yield return null;
            }
            orb.SineDist = 0;
            onShrinkEnd?.Invoke();
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                orb.Orb.Radius = Calc.LerpClamp(prevRadius * 0.7f, targetRadius, Ease.SineInOut(i));
                onGrowUpdate?.Invoke(Ease.SineInOut(i));
                yield return null;
            }
        }
        public IEnumerator OrbShrink(MemoryOrb orb, float radius, Action<float> onShrinkUpdate)
        {
            float prevRadius = orb.Orb.Radius;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                orb.Orb.Radius = Calc.LerpClamp(prevRadius, radius, Ease.SineIn(i));
                onShrinkUpdate?.Invoke(Ease.SineIn(i));
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.8f)
            {
                orb.Orb.Radius = radius + (float)Math.Sin(i * 4) * (1 - i) * 4;
                yield return null;
            }
            orb.Orb.Radius = radius;
        }
        public override void OnEnd(Level level)
        {
            level.EnableMovement();
            if (Scene.Tracker.GetEntity<BlueMemoryOrb>() is BlueMemoryOrb blue)
            {
                blue.StopShaking();
                blue.Orb.CenterColorLerp = blue.Orb.FillColorLerp = blue.Orb.EdgeColorLerp = 0;
                blue.Orb.RemoveShivers();
                OrbFlags.BlueCollected = true;
            }
            if (Scene.Tracker.GetEntity<GreenMemoryOrb>() is GreenMemoryOrb green)
            {
                green.StopShaking();
                green.Orb.CenterColorLerp = green.Orb.FillColorLerp = green.Orb.EdgeColorLerp = 0;
                green.Orb.RemoveShivers();
                OrbFlags.GreenCollected = true;
            }
            if (advancePortalAtEnd && Eye.gate.State != Portal.StDead)
            {
                Eye.gate.State = Eye.gate.State + 1;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (Player != null)
            {
                Player.Depth = 0;
            }
        }
    }
}
