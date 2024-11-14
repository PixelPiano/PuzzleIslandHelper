
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/WorldShiftFlicker")]
    [Tracked]
    public class WorldShiftFlicker : Trigger
    {
        private static VirtualRenderTarget _Buffer;
        public static VirtualRenderTarget Buffer => _Buffer ??= VirtualContent.CreateRenderTarget("WorldShiftFlickerBuffer", 320, 180);

        private Vector2 at;
        private float alpha;
        private List<Entity> entities = new();
        private bool activated;
        public WorldShiftFlicker(EntityData data, Vector2 offset) : base(data, offset)
        {
            at = data.NodesOffset(offset)[0];
            Add(new BeforeRenderHook(BeforeRender));
            Visible = false;
            Depth = -105010;
            Tag |= Tags.TransitionUpdate;
        }
        public void Activate()
        {
            activated = true;
            Add(new Coroutine(Sequence()));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Entity entity in scene.Entities)
            {
                if (entity.Position.X >= at.X - 8 && entity.Position.Y >= at.Y - 8)
                {
                    entities.Add(entity);
                }
            }
            Activate();
        }
        private IEnumerator Sequence()
        {
            Visible = true;
            float from;
            for (int i = 0; i < 7; i++)
            {
                Calc.PushRandom();
                float duration = Calc.Random.Range(0.05f, 0.1f);
                float target = Calc.Random.Range(0f, 0.19f);
                Ease.Easer ease = PianoUtils.RandomEaser();
                Ease.Easer ease2 = PianoUtils.RandomEaser();
                Calc.PopRandom();
                from = alpha;
                for (float j = 0; j < 1; j += Engine.DeltaTime / duration)
                {
                    alpha = Calc.LerpClamp(from, target, Ease.Follow(ease, ease2)(j));
                    yield return null;
                }
            }
            from = alpha;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
            {
                alpha = Calc.LerpClamp(from, 0, i);
                yield return null;
            }
            RemoveSelf();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _Buffer?.Dispose();
            _Buffer = null;
        }
        private void BeforeRender()
        {
            if (!Visible || !activated || Scene is not Level level) return;
            Vector2 offset = at - level.LevelOffset;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
            null, level.Camera.Matrix);
            level.BgTiles.Tiles.Position -= offset;
            level.BgTiles.Tiles.Render();
            level.BgTiles.Tiles.Position += offset;
            foreach (Entity e in entities)
            {
                if (e is BackgroundTiles or SolidTiles or WorldShiftFlicker) continue;
                e.Position -= offset;
                e.Render();
                e.Position += offset;
            }
            level.SolidTiles.Tiles.Position -= offset;
            level.SolidTiles.Tiles.Render();
            level.SolidTiles.Tiles.Position += offset;
            Draw.SpriteBatch.End();
        }
        public override void Render()
        {
            base.Render();
            if (!activated || Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Buffer, level.Camera.Position, Color.White * alpha);
        }
    }
}
