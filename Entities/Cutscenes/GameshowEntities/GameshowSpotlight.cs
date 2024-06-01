using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [CustomEntity("PuzzleIslandHelper/GameshowSpotlight")]
    [Tracked]
    public class GameshowSpotlight : Entity
    {

        public Vector2 Anchor;
        public Vector2 Target;
        public float Alpha = 1;
        public Color Color;
        private static int[] indices = new int[] { 0, 1, 2 };
        private static Color[] colors = new Color[] { Color.Blue, Color.Yellow, Color.Red, Color.Cyan, Color.Green, Color.Magenta };
        public Image Circle;
        public Image Led;
        public Image Light;
        private float circleScale = 0.3f;
        private float partyTimer;
        public bool On;
        public VertexPositionColor[] Vertices = new VertexPositionColor[3];
        public enum Mode
        {
            Default,
            Party,
            Evil
        }
        public Mode LightMode;
        private Color origColor;
        private int colorIndex;
        public Entity Track;
        public float CircleAlpha = 0.3f;
        public string ID;
        public Vector2 Offset;
        public GameshowSpotlight(EntityData data, Vector2 offset) : this(data.Position + offset, data.HexColor("color"), data.Attr("spotlightId"))
        {
        }
        public void Freakout(Vector2 topRight, float xRange, float yRange, Entity trackAfter = null)
        {
            Add(new Coroutine(FreakoutRoutine(topRight, xRange, yRange, trackAfter)));
        }
        public IEnumerator FreakoutRoutine(Vector2 topRight, float xRange, float yRange, Entity trackAfter = null)
        {
            Track = null;
            xRange = Math.Abs(xRange);
            yRange = Math.Abs(yRange);
            Vector2 speed = Vector2.Zero;
            Vector2 newTarget = topRight + new Vector2(Calc.Random.Range(-xRange, xRange), Calc.Random.Range(-yRange, yRange));
            for (int i = 0; i < 4; i++)
            {
                if (i > 2)
                {
                    xRange /= 2;
                    yRange /= 2;
                }
                for (float j = 0; j < 1; j += Engine.DeltaTime / 0.2f)
                {
                    MoveTowards(newTarget, j * 0.8f);
                    Target.Y += speed.Y;
                    if (i > 4)
                    {
                        speed.Y -= 1;
                    }
                    yield return null;
                }
                newTarget = topRight + new Vector2(Calc.Random.Range(-xRange, xRange), Calc.Random.Range(-yRange, yRange));
            }
            float[] wait = new float[] { 0.2f, 0.2f, 0.05f, 0.1f, 0.3f, 0.1f };
            On = false;
            foreach (float f in wait)
            {
                yield return f;
                On = !On;
            }
            wait.Reverse();
            yield return 4f;
            foreach (float f in wait)
            {
                yield return f;
                On = !On;
            }
            On = true;
            if (trackAfter != null)
            {
                yield return ApproachEntity(trackAfter, 0.4f, Vector2.Zero, true);
            }
        }
        public void MoveTowards(Vector2 position, float maxMove)
        {
            Target = Vector2.Lerp(Target, position, maxMove);
        }
        public static GameshowSpotlight GetSpotlight(Scene scene, string id)
        {
            if (scene is Level level)
            {
                foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
                {
                    if (light.ID == id) return light;
                }
            }
            return null;
        }
        public GameshowSpotlight(Vector2 position, Color color = default, string id = null) : base(position)
        {

            On = true;
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
            Circle.Color = color * CircleAlpha;
            Circle.Scale = Vector2.One * circleScale;

            Add(Circle, Led, Light);
            UpdateVertices();

            Tag |= Tags.TransitionUpdate;
            ID = id;
        }
        public void TargetEntity(Entity entity, float time, Vector2 offset, bool track)
        {
            Add(new Coroutine(ApproachEntity(entity, time, offset, track)));
        }
        public void TargetPosition(Vector2 position, float time)
        {
            Add(new Coroutine(ApproachPosition(position, time)));
        }
        public IEnumerator ApproachEntity(Entity entity, float time, Vector2 offset, bool track = false)
        {
            Vector2 prev = Target;
            Track = null;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Target = Vector2.Lerp(prev, entity.Center + offset, i);
                yield return null;
            }
            Target = entity.Center + offset;
            if (track) Track = entity;
        }
        public IEnumerator ApproachPosition(Vector2 position, float time)
        {
            Vector2 prev = Target;
            Track = null;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Target = Vector2.Lerp(prev, position, i);
                yield return null;
            }
            Target = position;
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
        public void ApproachColor(Color newColor)
        {
            Add(new Coroutine(FadeColor(newColor, 0.7f)));
        }
        private IEnumerator FadeColor(Color newColor, float time)
        {
            Color color = Color;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Color = Color.Lerp(color, newColor, i);
                yield return null;
            }
        }
        public void StartFollowing(Entity entity)
        {
            Track = entity;
        }
        public void RenderCircle(Color? color = null, Vector2? offset = null)
        {
            if (!On) return;
            Color prevCircle = Circle.Color;
            if (color.HasValue)
            {
                Circle.Color = color.Value;
            }
            if (offset.HasValue)
            {
                Circle.RenderPosition += offset.Value;
            }
            Circle.Render();
            if (color.HasValue)
            {
                Circle.Color = prevCircle;
            }
            if (offset.HasValue)
            {
                Circle.RenderPosition -= offset.Value;
            }

        }
        public void RenderLight(Matrix matrix, Color? color = null, Vector2? offset = null)
        {
            if (!On) return;
            Color[] prev = new Color[3];
            for (int i = 0; i < 3; i++)
            {
                if (color.HasValue)
                {
                    prev[i] = Vertices[i].Color;
                    Vertices[i].Color = color.Value;
                }
                if (offset.HasValue)
                {
                    Vertices[i].Position.X += offset.Value.X;
                    Vertices[i].Position.Y += offset.Value.Y;
                }
            }
            GFX.DrawIndexedVertices(matrix, Vertices, 3, indices, 1);
            for (int i = 0; i < 3; i++)
            {
                if (color.HasValue)
                {
                    Vertices[i].Color = prev[i];
                }
                if (offset.HasValue)
                {
                    Vertices[i].Position.X -= offset.Value.X;
                    Vertices[i].Position.Y -= offset.Value.Y;
                }
            }
        }
        public void RenderLed()
        {
            Led.DrawSimpleOutline();
            Led.Render();
            Light.Render();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoUtils.SeekController<GSRenderer>(scene) == null)
            {
                scene.Add(new GSRenderer());
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
            if (LightMode == Mode.Party)
            {
                partyTimer += Engine.DeltaTime;
                if (partyTimer > 1)
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
            if (Track is not null)
            {
                Target = Track.Center;
            }
            Circle.RenderPosition = Target;
            Circle.Color = Color * CircleAlpha;
            Light.Color = On ? Color : Color.Black;
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
        public void DrawVertices(Vector2 offset, Matrix matrix)
        {
            if (!On) return;
            for (int i = 0; i < 3; i++)
            {
                Vertices[i].Position += new Vector3(offset, 0);
            }
            GFX.DrawIndexedVertices(matrix, Vertices, 3, indices, 1);
            for (int i = 0; i < 3; i++)
            {
                Vertices[i].Position -= new Vector3(offset, 0);
            }
        }
    }
}
