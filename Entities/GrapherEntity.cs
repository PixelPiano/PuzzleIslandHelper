using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
// PuzzleIslandHelper.GrapherEntity
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/GrapherEntity")]
    public class GrapherEntity : Entity
    {
        public static float timeMod = 0.2f;
        public static float piMod = 32;
        public static float step = (float)Math.PI / piMod;
        public static bool State = false; //overall visibility override
        public static string ColorGradeName = "PianoBoy/inverted";
        public static float k = 0;
        public static float lineWidth = 1f;
        public static float size = 10f; 
        public static bool sizeMult = true;
        public static string luaColor = "ffffff";
        public static float colorAlpha = 1.0f;
        public static Player player;
        private static float tempK = k;
        private static Vector2 st;
        private static Vector2 end;
        public static Color color = Calc.HexToColor(luaColor);

        public GrapherEntity(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            timeMod = data.Float("timeMod",0.2f);
            ColorGradeName = data.Attr("Colorgrade", "PianoBoy/inverted");
            lineWidth = data.Float("lineWidth", 1f);
            size = data.Float("size", 10f);
            luaColor = data.Attr("color", "ffffff");
            colorAlpha = data.Float("opacity", 1.0f);
            color = Calc.HexToColor(luaColor);
        }
        public override void Update()
        {
            if (State)
            {
                tempK = k + Engine.DeltaTime * timeMod;
            }
            //k += Engine.DeltaTime * timeMod;
            base.Update();
        }
        public override void Render()
        {
            if (State)
            {
                k = tempK;
                Vector2 playerCenter = player.Position - new Vector2(1, (player.Height / 2f)+1);
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
                        Draw.Line(st, end, Color.White * colorAlpha, lineWidth);
                        prevValue = currentValue;
                    }
            }
            base.Render();
        }
        public override void Awake(Scene scene)
        {
            player = Scene.Tracker.GetEntity<Player>();
            base.Awake(scene);
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
