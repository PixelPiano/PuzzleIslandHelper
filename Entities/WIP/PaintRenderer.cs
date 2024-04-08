using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP.PianoEntities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [Tracked]
    public class PaintRenderer : Entity
    {
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("PaintTarget", 320, 180);
        public int Size = 16;
        public Cursor Mouse;
        public PaintRenderer() : base(Vector2.Zero)
        {
            Depth = -10001;
            Tag |= Tags.TransitionUpdate | Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
            Collider = new Hitbox(320, 180);

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Mouse = new Cursor("objects/PuzzleIslandHelper/piano/Cursor"));
            
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            player.StateMachine.State = 11;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            Draw.Circle(Mouse.WorldPosition - Vector2.One * (Size/2), Size, Color.White, 32);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            Position = level.Camera.Position;
            Matrix matrix = Matrix.Identity;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            if (Cursor.RightClicked)
            {
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            }
            if (Cursor.LeftClicked)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
                Draw.Circle((Cursor.MousePosition / 6) - Vector2.One * (Size / 2), Size / 2, Color.Red,Size,Size);
                Draw.SpriteBatch.End();
            }
        }
        public static void Unload()
        {
            _Target?.Dispose();
            _Target = null;
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }
        public static void Load()
        {
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }

        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new PaintRenderer());
        }
    }
}
