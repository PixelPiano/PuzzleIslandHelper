using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
// PuzzleIslandHelper.GrapherEntity
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/GrapherEntity")]
    public class GrapherEntity : Entity
    {
        public float timeMod = 0.2f;
        public float piMod = 32;
        public float step;
        public bool State = false; //overall visibility override
        public string ColorGradeName = "PianoBoy/invertFlag";
        public float k = 0;
        public float lineWidth = 1f;
        public static float size = 10f;
        public static bool sizeMult = true;
        public string luaColor = "ffffff";
        public float colorAlpha = 1.0f;
        public Player player;
        private float tempK;
        private Vector2 st;
        private Vector2 end;
        public Color color;
        public static float Alpha;
        public GrapherEntity(Vector2 position)
            : this(position, 0.2f, "PianoBoy/Inverted", 0, 0, 0.3f, Color.Black)
        {
            State = false;
        }
        public GrapherEntity(Vector2 position, float timeMod, string colorgrade, float lineWidth, float size, float alpha, Color color)
        : base(position)
        {
            this.timeMod = timeMod;
            step = (float)Math.PI / piMod;
            tempK = k;
            ColorGradeName = colorgrade;
            this.lineWidth = lineWidth;
            GrapherEntity.size = size;
            colorAlpha = alpha;
            this.color = color;
        }
        public GrapherEntity(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Float("timeMod", 0.2f),
                  data.Attr("colorgrade", "PianoBoy/invertFlag"),
                  data.Float("lineWidth", 1f),
                  data.Float("size", 10f),
                  data.Float("alpha", 1.0f),
                  data.HexColor("color"))
        {
        }
        public override void Update()
        {
            base.Update();
            if (State)
            {
                tempK = k + Engine.DeltaTime * timeMod;
            }
        }
        public override void Render()
        {
            base.Render();
            if (State)
            {
                k = tempK;
                Vector2 playerCenter = player.Position - new Vector2(1, player.Height / 2f + 1);
                Vector2 prevValue = Vector2.Zero;
                float theta = step;
                Vector2 currentValue = Vector2.Zero;
                st = prevValue + playerCenter;
                end = currentValue + playerCenter;
                for (; theta <= (float)Math.PI * 4f; theta += step)
                {
                    yieldPointFromFunction(theta, k, ref currentValue);
                    st = prevValue + playerCenter;
                    end = currentValue + playerCenter;
                    Draw.Line(st, end, Color.White * colorAlpha * Alpha, lineWidth);
                    prevValue = currentValue;
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = Scene.Tracker.GetEntity<Player>();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        private static void yieldPointFromFunction(float theta, float k, ref Vector2 currentValue)
        {
            if (size <= 10 && sizeMult)
            {
                size = size * 10f;
                sizeMult = false;
            }
            currentValue.X = (float)(Math.Sin(k * theta) * Math.Cos(theta)) * size;
            currentValue.Y = (float)(Math.Sin(k * theta) * Math.Sin(theta)) * size;
        }

    }
}
