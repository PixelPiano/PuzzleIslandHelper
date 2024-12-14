using Celeste;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class FizzleOut : Entity
    {
        public VirtualRenderTarget Buffer;
        private Action drawAtVectorZero;
        private Effect Shader;
        private float StartFade;
        private float EndFade;
        private float Amplitude = 1;
        private float Size;
        public float Percent;
        public FizzleOut(Action drawAtVectorZero, Collider collider, float duration) : this(drawAtVectorZero, collider.AbsolutePosition, collider.Width, collider.Height, duration) { }
        public FizzleOut(Action drawAtVectorZero, Rectangle bounds, float duration) : this(drawAtVectorZero, bounds.Location.ToVector2(), bounds.Width, bounds.Height, duration) { }
        public FizzleOut(Action drawAtVectorZero, Vector2 position, float width, float height, float duration) : base(position)
        {
            Tag |= Tags.Persistent | Tags.TransitionUpdate;
            Depth = -10003;
            this.drawAtVectorZero = drawAtVectorZero;
            int w = (int)width;
            int h = (int)height;
            Size = Calc.Max(w, h);
            Buffer = VirtualContent.CreateRenderTarget("bitrailabsorb", w, h);
            Collider = new Hitbox(w, h);
            StartFade = 0f;
            EndFade = 1f;
            Tween tween = Tween.Create(Tween.TweenMode.Looping, Ease.CubeOut, duration, true);
            tween.OnUpdate = t =>
            {
                Amplitude = 1 - t.Eased;
                Percent = t.Eased;
            };
            tween.OnComplete = t =>
            {
                Percent = 1;
                RemoveSelf();
            };
            Add(tween);
            Shader = ShaderHelper.TryGetEffect("bitrailAbsorb");
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void ApplyParameters(Level level, Matrix matrix)
        {
            Shader.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            Shader.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            Shader.Parameters["CamPos"]?.SetValue(level.Camera.Position);
            Shader.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * HDlesteCompat.Scale);
            Shader.Parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            Shader.Parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            Shader.Parameters["ViewMatrix"]?.SetValue(matrix);
        }
        private void BeforeRender()
        {
            if (Scene is not Level level) return;

            Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            ApplyParameters(level, Matrix.Identity);
            Shader.Parameters["Center"]?.SetValue(Vector2.One * 0.5f);
            Shader.Parameters["EndFade"]?.SetValue(EndFade);
            Shader.Parameters["StartFade"]?.SetValue(StartFade);
            Shader.Parameters["Amplitude"]?.SetValue(Amplitude);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, Shader, Matrix.Identity);
            drawAtVectorZero.Invoke();
            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            Draw.SpriteBatch.Draw(Buffer, Position, Color.White);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Buffer?.Dispose();
            Buffer = null;
            Shader?.Dispose();
            Shader = null;
        }
    }
}
[Tracked]
public class BitrailAbsorb : Entity
{
    public ParticleSystem pSystem;
    public List<ParticleData> Data = new();
    public class ParticleData : Component
    {
        public Particle particle;
        public float amount;
        private float duration;
        private Vector2 from;
        public bool Finished;
        private Func<Vector2> GetTarget;
        public ParticleData(Particle particle, Vector2 from, float duration, Func<Vector2> getTarget) : base(true, true)
        {
            GetTarget = getTarget;
            this.particle = particle;
            this.from = from;
            this.duration = duration;

        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add(new Coroutine(routine()));
        }
        private IEnumerator routine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                particle.Position = Vector2.Lerp(from, GetTarget.Invoke(), Ease.CubeIn(i));
                yield return null;
            }
            particle.Color = Color.White * 0;
            Finished = true;
            RemoveSelf();
        }
        public override void Render()
        {
            base.Render();
            particle.Render();
        }
    }
    public Func<Vector2> GetTarget;
    public bool Finished;
    public bool Disabled;
    private ParticleType Particle = new()
    {
        Size = 1,
        SizeRange = 2,
        ColorMode = ParticleType.ColorModes.Choose,
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.5f,
        LifeMax = 1.5f,
    };
    public FizzleOut Fizzle;
    public void EmitParticle()
    {
        if (!Disabled)
        {
            Vector2 from = Center + Vector2.UnitX * Calc.Random.Range(-Width, Width) / 2 * (1 - Fizzle.Percent);
            from.Y += Calc.Random.Range(0, 3);

            Particle p2 = new();
            Particle p = Particle.Create(ref p2, from, Calc.Random.Choose(Color.Green, Color.LightGreen));
            ParticleData data = new ParticleData(p, from, 0.2f, GetTarget);
            Add(data);
            Data.Add(data);
        }
    }
    public BitrailAbsorb(Action draw, Collider collider, Color color1, Color color2, Entity approach) : this(draw, collider.AbsolutePosition, collider.Width, collider.Height, color1, color2, delegate { return approach.Center; })
    {

    }
    public BitrailAbsorb(Action drawAtVectorZero, Vector2 position, float width, float height, Color color1, Color color2, Func<Vector2> getTarget) : base(position)
    {
        Tag |= Tags.Persistent | Tags.TransitionUpdate;
        GetTarget = getTarget;
        Depth = -10002;
        Fizzle = new FizzleOut(drawAtVectorZero, position, width, height, 1.3f);

        Particle.Color = color1;
        Particle.Color2 = color2;
        int w = (int)width;
        int h = (int)height;
        Collider = new Hitbox(w, h);
    }
    public override void Update()
    {
        base.Update();
        if (Fizzle.Percent >= 1)
        {
            foreach (ParticleData data in Data)
            {
                if (!data.Finished) return;
            }
            Finished = true;
            RemoveSelf();
        }
        if (Fizzle.Percent < 0.85f)
        {
            EmitParticle();
        }
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);

    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(Fizzle);
        pSystem = new ParticleSystem(Depth, 100);
        scene.Add(pSystem);
    }
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        pSystem.RemoveSelf();
        Fizzle.RemoveSelf();
    }
}
