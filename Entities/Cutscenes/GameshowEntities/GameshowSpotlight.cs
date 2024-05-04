using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/GameshowSpotlight")]
    [Tracked]
    public class GameshowSpotlight : Entity
    {

        [Tracked]
        public class GSRenderer : Entity
        {
            public static BlendState BlendState = BlendState.Additive;
            public GSRenderer() : base(Vector2.Zero)
            {
                Tag |= Tags.Global | Tags.TransitionUpdate;
                Depth = -100001;
                Add(new BeforeRenderHook(BeforeRender));
            }
            public void BeforeRender()
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                if (Scene is Level level && level.Tracker.GetEntities<GameshowSpotlight>().Count > 0)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState);
                    GameshowSpotlight cache = null;
                    foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
                    {
                        light.DrawVertices(level);
                        cache = light;
                    }
                    cache.Circle.RenderPosition -= level.Camera.Position;
                    cache.Circle.Render();
                    cache.Circle.RenderPosition += level.Camera.Position;
                    Draw.SpriteBatch.End();
                }
            }

            public override void Render()
            {
                base.Render();
                if (Scene is Level level)
                {
                    Draw.SpriteBatch.Draw(Buffer, level.Camera.Position, Color.White);
                    foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
                    {
                        light.Led.DrawSimpleOutline();
                        light.Led.Render();
                        light.Light.Render();
                    }
                }
            }
        }
        public Vector2 Anchor;
        public Vector2 Target;
        public float Alpha = 1;
        public Color Color;
        public static int[] indices = new int[] { 0, 1, 2 };
        private Image Circle;
        private Image Led;
        private Image Light;
        private float circleScale = 0.3f;
        private Color[] colors = new Color[]{Color.Blue,  Color.Yellow, Color.Red, Color.Cyan, Color.Green, Color.Magenta};
        private float partyTimer;
        private VertexPositionColor[] Vertices = new VertexPositionColor[3];
        public enum Mode
        {
            Default,
            Party,
            Evil
        }
        public Mode LightMode;
        private Color origColor;
        private int colorIndex;
        private static VirtualRenderTarget _Buffer;
        public static VirtualRenderTarget Buffer => _Buffer ??= VirtualContent.CreateRenderTarget("GameshowSpotlightBuffer", 320, 180);

        public GameshowSpotlight(EntityData data, Vector2 offset) : this(data.Position + offset, data.HexColor("color"))
        {
        }
        public GameshowSpotlight(Vector2 position, Color color = default) : base(position)
        {
            origColor = Color = color;
            Anchor = Position.Round();
            Vertices[0] = new();
            Vertices[1] = new();
            Vertices[2] = new();

            Circle = new Image(GFX.Game["utils/PuzzleIslandHelper/circle"]);
            Led = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshowSpotlight/led"]);
            Light = new Image(GFX.Game["objects/PuzzleIslandHelper/gameshowSpotlight/light"]);

            Circle.CenterOrigin();
            Led.CenterOrigin();
            Light.CenterOrigin();
            Circle.Visible = false;
            Led.Visible = false;
            Light.Visible = false;
            Circle.Color = color * 0.3f;
            Circle.Scale = Vector2.One * circleScale;
            //Light.RenderPosition = Led.RenderPosition = Anchor;

            Add(Circle, Led, Light);
            UpdateVertices();

            Tag |= Tags.TransitionUpdate;
            Add(new Coroutine(ColorSwitch()));
        }
        private IEnumerator ColorSwitch()
        {
            Color[] colors = new Color[]
            {
                Color.Blue,  Color.Yellow, Color.Red, Color.Cyan, Color.Green, Color.Magenta
            };
            while (true)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    Color = colors[i];
                    yield return 0.8f;
                }
            }
        }
        public void RenderLed()
        {
            Led.Render();
            Light.Render();
        }
        public static void Unload()
        {
            _Buffer?.Dispose();
            _Buffer = null;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoUtils.SeekController<GSRenderer>(scene) == null)
            {
                scene.Add(new GSRenderer());
            }
            if (scene.GetPlayer() is Player player)
            {
                Target = player.Center;
                Circle.RenderPosition = Target;
            }
        }
        public void UpdateVertices()
        {
            if (Scene is not Level level) return;
            float angle = Calc.Angle(Anchor, Target);
            Light.Rotation = Led.Rotation = angle;
            Vector2 reference = Target + Calc.AngleToVector(angle, Circle.Width / 2 * circleScale);
            Vector2 pos1 = PianoUtils.RotatePoint(reference, Target, 90);
            Vector2 pos2 = PianoUtils.RotatePoint(reference, Target, -90);
            Vertices[0].Position = new Vector3(Anchor, 0);
            Vertices[1].Position = new Vector3(pos1, 0);
            Vertices[2].Position = new Vector3(pos2, 0);
            Vertices[0].Color = Color;
            Vertices[1].Color = Vertices[2].Color = Color.Transparent;
        }
        public override void Update()
        {
            base.Update();
            if(LightMode == Mode.Party)
            {
                partyTimer += Engine.DeltaTime;
                if(partyTimer > 1)
                {
                    colorIndex++;
                    colorIndex %= colors.Length;
                    partyTimer = 0;
                }
            }
            Color = LightMode switch
            {
                Mode.Default => origColor,
                Mode.Party => colors[colorIndex],
                Mode.Evil => Color.Red,
                _ => Color.White
            };
            if (Scene is Level level && level.GetPlayer() is Player player)
            {
                Target = player.Center;
            }
            Circle.RenderPosition = Target;
            Circle.Color = Color * 0.3f;
            Light.Color = Color;
            UpdateVertices();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(Vertices[0].Position.XY(), Vertices[1].Position.XY(), Color);
            Draw.Line(Vertices[1].Position.XY(), Vertices[2].Position.XY(), Color);
            Draw.Line(Vertices[2].Position.XY(), Vertices[0].Position.XY(), Color);
        }
        public void DrawVertices(Level level)
        {
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, 3, indices, 1);
        }
    }
}
