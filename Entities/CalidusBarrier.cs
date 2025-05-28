using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.SecurityLaser
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CalidusBarrier")]
    [Tracked]
    public class CalidusBarrier : Entity
    {
        private float LaserOpacity = 1;
        private float LaserWidth = 2;
        private Sprite NodeSprite;
        private Sprite BaseSprite;
        private Vector2 Node;
        private Vector2 Start;
        private Vector2 End;
        public FlagData Flag;
        private Color BadColor = Color.Magenta;
        public Color Color => Color.Lerp(Color.Violet, BadColor, dangerColorLerp);
        private float colorLerp;
        private bool On => Flag.State;
        private int randomize;
        private VertexLight Light;
        private Color origBadColor = Color.Purple;
        private List<Vector2> Points = [];
        private int range;
        private float dangerColorLerp;
        public CalidusBarrier(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            BadColor = data.HexColor("color", Color.Magenta);
            Tag |= Tags.TransitionUpdate;
            Node = data.Nodes[0] + offset;
            Depth = -10001;
            Light = new VertexLight(Vector2.One * 4, Color.White, Visible ? 1 : 0, (int)LaserWidth, (int)LaserWidth + 5);
            Flag = data.Flag();
            Add(BaseSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/securityLaser/"));
            Add(NodeSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/securityLaser/"));
            BaseSprite.AddLoop("idle", "emitter", 0.1f);
            NodeSprite.AddLoop("idle", "emitter", 0.1f);
            float angle = (float)Math.Atan2(Y - Node.Y, X - Node.X);
            BaseSprite.Rotation = angle - MathHelper.PiOver2;
            NodeSprite.Rotation = angle + MathHelper.PiOver2;
            BaseSprite.Visible = NodeSprite.Visible = false;
            BaseSprite.CenterOrigin();
            NodeSprite.CenterOrigin();
            BaseSprite.Play("idle");
            NodeSprite.Play("idle");
            BaseSprite.Position = (NodeSprite.Position += -Vector2.One * 4);

            Collider = new Hitbox(Width, Height);
            Start = Position + Vector2.One * 4;
            End = Node + Vector2.One * 4;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Tween LightTween = Tween.Create(Tween.TweenMode.Looping, Ease.SineInOut, 2);
            LightTween.OnUpdate = (t) =>
            {
                Light.Position = Calc.LerpSnap(Start, End, t.Eased) - Position;
            };
            Tween ColorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1);
            ColorTween.OnUpdate = (t) =>
            {
                BadColor = Color.Lerp(origBadColor, Color.White, t.Eased / 4f);
            };
            Add(Light);
            Add(LightTween, ColorTween);
            LightTween.Start();
            ColorTween.Start();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            Tween colorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Follow(Ease.SineInOut, Ease.CubeInOut), 0.8f);
            colorTween.OnUpdate = (t) =>
            {
                colorLerp = Calc.LerpClamp(0.1f, 0.4f, t.Eased);
            };
            Add(colorTween);
            colorTween.Start();
            Collider = new Hitbox(8, 8);
        }
        private List<Vector2> GetPoints(Vector2 start, Vector2 end, int range)
        {
            List<Vector2> list = new();
            int points = (int)Vector2.Distance(start, end);
            for (int i = 0; i < points / 4; i++)
            {
                Vector2 position = Vector2.Lerp(start, end, i / (float)points * 4);
                int xVar = Calc.Random.Range(-range, range + 1);
                int yVar = Calc.Random.Range(-range, range + 1);
                list.Add((position + new Vector2(xVar, yVar)).Floor());
            }

            return list;
        }

        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            dangerColorLerp = Calc.Random.Choose(1, 0.5f, 0.3f, 0.8f, 0);
            NodeSprite.Color = Color.Lerp(Color, Color.Black, 0.3f);
            BaseSprite.Color = Color.Lerp(Color, Color.White, 0.6f);
            if (On && player.CollideLine(Start, End))
            {
                player.Die(Vector2.Zero);
            }
        }
        public override void Render()
        {
            base.Render();
            Color color = Color;
            BaseSprite.DrawOutline(color * colorLerp);
            NodeSprite.DrawOutline(color * colorLerp);
            if (On)
            {
                Draw.Line(Start, End, Color.Lerp(color, Color.Black, 0.5f) * LaserOpacity * 0.2f, LaserWidth + 4);
                Draw.Line(Start, End, Color.Lerp(color, Color.Black, 0.3f) * LaserOpacity * 0.2f, LaserWidth + 2);
                Draw.Line(Start, End, color * LaserOpacity, LaserWidth);

                if (randomize == 0)
                {
                    range = Calc.Random.Range(1, 4);
                    Points = GetPoints(Start, End + Vector2.One, range);
                    randomize = 3;
                }
                randomize--;
                for (int i = 1; i < Points.Count; i++)
                {
                    float Opacity = Calc.Random.Range(1f, 0.4f);
                    float Lerp = Calc.Random.Range(0, 1f);
                    int thicc = Calc.Random.Range(1, 3);
                    Draw.Line(Points[i], Points[i - 1], Color.Lerp(Color.OrangeRed, Color.Yellow, Lerp) * Opacity, thicc);
                }
            }
            else
            {
                randomize = 0;
            }
            BaseSprite.RenderAt(Position + BaseSprite.Position);
            NodeSprite.RenderAt(Position + NodeSprite.Position);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(Start, End, Color.Yellow);
        }

    }
}