using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomWaterfall")]
    [Tracked(false)]
    public class CustomWaterfall : Entity
    {
        private readonly FlagList renderFlag;
        private readonly FlagList displacementFlag;
        private readonly FlagList audioFlag;
        
        private readonly bool passThrough;
        private float height;
        private Water water;
        private Solid solid;
        private SoundSource loopingSfx;
        private SoundSource enteringSfx;
        public CustomWaterfall(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -9999;
            Tag = Tags.TransitionUpdate;
            renderFlag = data.FlagList("renderFlag", "invertRenderFlag");
            displacementFlag = data.FlagList("displacementFlag", "invertDisplacementFlag");
            audioFlag = data.FlagList("audioFlag", "invertAudioFlag");
            passThrough = data.Bool("goesThroughSolids");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = Scene as Level;
            bool flag = false;
            height = 8f;
            while (Y + height < level.Bounds.Bottom
                 && (water = Scene.CollideFirst<Water>(new Rectangle((int)X, (int)(Y + height), 8, 8))) == null
                 && ((solid = Scene.CollideFirst<Solid>(new Rectangle((int)X, (int)(Y + height), 8, 8))) == null
                 || !solid.BlockWaterfalls
                 || passThrough)
                 )
            {
                height += 8f;
                solid = null;
            }
            Add(loopingSfx = new SoundSource());
            Add(enteringSfx = new SoundSource());
            loopingSfx.Play("event:/env/local/waterfall_small_main");
            enteringSfx.Play(flag ? "event:/env/local/waterfall_small_in_deep" : "event:/env/local/waterfall_small_in_shallow");
            enteringSfx.Position.Y = height;
            loopingSfx.Pause();
            enteringSfx.Pause();

            if (audioFlag)
            {
                loopingSfx.Resume();
                enteringSfx.Resume();
            }
            Add(new DisplacementRenderHook(RenderDisplacement));
        }
        public override void Update()
        {
            Vector2 position = (Scene as Level).Camera.Position;
            if (renderFlag)
            {
                if (audioFlag)
                {
                    loopingSfx.Pause();
                    enteringSfx.Pause();
                }
                else
                {
                    loopingSfx.Resume();
                    enteringSfx.Resume();
                }
                loopingSfx.Position.Y = Calc.Clamp(position.Y + 90f, Y, height);
                if (water != null && Scene.OnInterval(0.3f))
                {
                    water.TopSurface.DoRipple(new Vector2(X + 4f, water.Y), 0.75f);
                }
                if (water != null || solid != null)
                {
                    Vector2 position2 = new(X + 4f, Y + height + 2f);
                    (Scene as Level).ParticlesFG.Emit(Water.P_Splash, 1, position2, new Vector2(8f, 2f), new Vector2(0f, -1f).Angle());
                }
            }
            else
            {
                loopingSfx.Pause();
                enteringSfx.Pause();
            }
            base.Update();
        }
        public void RenderDisplacement()
        {
            if (displacementFlag && renderFlag)
            {
                Draw.Rect(X, Y, 8f, height, new Color(0.5f, 0.5f, 0.8f, 1f));
            }
        }
        public override void Render()
        {
            if (renderFlag)
            {
                if (water == null || water.TopSurface == null)
                {
                    Draw.Rect(X + 1f, Y, 6f, height, Water.FillColor);
                    Draw.Rect(X - 1f, Y, 2f, height, Water.SurfaceColor);
                    Draw.Rect(X + 7f, Y, 2f, height, Water.SurfaceColor);
                    return;
                }
                Water.Surface topSurface = water.TopSurface;
                float num = height + water.TopSurface.Position.Y - water.Y;
                for (int i = 0; i < 6; i++)
                {
                    Draw.Rect(X + i + 1f, Y, 1f, num - topSurface.GetSurfaceHeight(new Vector2(X + 1f + i, water.Y)), Water.FillColor);
                }
                Draw.Rect(X - 1f, Y, 2f, num - topSurface.GetSurfaceHeight(new Vector2(X, water.Y)), Water.SurfaceColor);
                Draw.Rect(X + 7f, Y, 2f, num - topSurface.GetSurfaceHeight(new Vector2(X + 8f, water.Y)), Water.SurfaceColor);
            }
        }
    }
}