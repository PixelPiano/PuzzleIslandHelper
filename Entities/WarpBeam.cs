using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    public class WarpBeam : Entity
    {
        public WarpCapsule Parent;
        public Image Glow;
        private float scaleMult = 0;
        public Vector2 Scale;
        public Vector2 End
        {
            get
            {
                Level level = Scene as Level;
                return new Vector2(Position.X, Math.Min(level.Camera.Top - 16, level.Bounds.Top - 16));
            }
        }
        public float YOffset;
        public bool Sending = true;
        public Vector2 Start => Sending ? Position + Vector2.UnitY * (4 + YOffset) : Position;
        public int Thickness = 0;
        public int MaxThickness = 10;
        private float scaleTime = 0.3f;
        public bool ReadyForScale;
        public bool Finished;
        public float scaleTargetMult = 1;
        public List<PulseRing> Rings = new();
        public WarpBeam(WarpCapsule parent)
        {
            Depth = -100000;
            Parent = parent;
            Glow = new Image(GFX.Game["objects/PuzzleIslandHelper/digiWarpReceiver/glow"], true);
            Glow.CenterOrigin();
            Glow.Scale = Vector2.Zero;
            Add(Glow);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(ScaleRoutine()));
            Position = Parent.TopCenter;
        }
        public void AddPulses()
        {
            float dist = Vector2.Distance(Position, End);

            for (int i = 8; i < dist; i += 8)
            {
                EmitPulse(i, Calc.LerpClamp(80, 40, i / dist), 16, 1.1f - (i / dist) * 0.5f);
            }
        }
        public void EmitBeam(int beforeTeleportThickness, int afterTeleportThickness, WarpCapsule.WarpBack cutscene)
        {
            Add(new Coroutine(Cutscene(beforeTeleportThickness, afterTeleportThickness, cutscene)));
        }
        public IEnumerator Cutscene(int beforeTeleportThickness, int afterTeleportThickness, WarpCapsule.WarpBack cutscene)
        {
            Thickness = beforeTeleportThickness;
            Glow.Visible = false;
            while (!cutscene.Teleported) yield return null;
            Thickness = afterTeleportThickness;
            yield return 0.4f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.7f)
            {
                Thickness = (int)Calc.LerpClamp(afterTeleportThickness, 0, Ease.SineIn(i));
                yield return null;
            }
            Finished = true;
            RemoveSelf();
        }
        public override void Update()
        {
            base.Update();
            scaleTime = Calc.Approach(scaleTime, 0.05f, Engine.DeltaTime * 2);
            scaleMult = Calc.Approach(scaleMult, ReadyForScale ? 3 : 1, Engine.DeltaTime * 2);
            Glow.Position.Y = YOffset;
            Glow.Scale = Scale * scaleMult;
        }
        public void EmitPulse(float dist, float width, float height, float rate)
        {
            PulseRing ring = new PulseRing(Calc.Approach(Start, End, dist), width, height, rate);
            Rings.Add(ring);
            Scene.Add(ring);
        }
        public override void Render()
        {
            if (Thickness > 0)
            {
                Draw.Line(Start, End, Color.White, Thickness);
            }
            base.Render();
        }
        private IEnumerator ScaleRoutine()
        {
            int passes = 0;
            while (true)
            {
                ReadyForScale = passes > 15;
                for (float i = 0; i < 1;)
                {
                    Scale.X = Calc.LerpClamp(0.25f, 1f, i);
                    Scale.Y = Calc.LerpClamp(1f, 0.25f, i);
                    i += Engine.DeltaTime / scaleTime;
                    yield return null;
                }
                Scale.X = 1f;
                Scale.Y = 0.5f;
                yield return null;
                for (float i = 0; i < 1;)
                {
                    Scale.X = Calc.LerpClamp(1f, 0.25f, i);
                    Scale.Y = Calc.LerpClamp(0.25f, 1f, i);
                    i += Engine.DeltaTime / scaleTime;
                    yield return null;
                }
                Scale.X = 0.5f;
                Scale.Y = 1;
                passes++;
                yield return null;
            }
        }
    }
    [Tracked(false)]
    public class PulseRing : Entity
    {
        public VirtualRenderTarget Target;
        public float Amplitude;
        public float Rate;
        public PulseRing(Vector2 position, float width, float height, float rate = 1) : base(position)
        {
            Depth = -100000;
            Target = VirtualContent.CreateRenderTarget("PulseRing", (int)width, (int)height);
            Collider = new Hitbox(width, height, -width / 2, -height / 2);
            Add(new BeforeRenderHook(BeforeRender));
            Rate = rate;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
        public override void Update()
        {
            base.Update();
            Amplitude = Calc.Approach(Amplitude, 1, Engine.DeltaTime * Rate);
            if (Amplitude >= 1) RemoveSelf();
        }
        public void BeforeRender()
        {
            Target?.SetAsTarget(Color.Transparent);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || Amplitude <= 0) return;
            Effect effect = ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/pulseRing");
            if (effect != null)
            {
                effect.ApplyCameraParams(level);
                effect.Parameters["Amplitude"]?.SetValue(Amplitude);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.StandardBegin(level.Camera.Matrix, effect);
                Draw.SpriteBatch.Draw(Target, Collider.AbsolutePosition, Color.White);
                Draw.SpriteBatch.End();
                GameplayRenderer.Begin();
            }
        }
    }
}