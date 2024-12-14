
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/VertexShaderTrigger")]
    public class VertexShaderTrigger : Trigger
    {
        public string[] Effects;

        public bool Activated;

        public bool Clear;

        public VertexPositionColor[] Vertices;
        private static int[] indices = { 0, 1, 3, 3, 1, 4, 1, 2, 4, 5, 3, 6, 3, 4, 6, 6, 4, 7, 0, 3, 5, 4, 2, 7 };
        private Color[] colors =
        {
            Color.White,
            Color.Red,
            Color.Blue,
            Color.Yellow,
            Color.Cyan,
            Color.Magenta,
            Color.Lime,
            Color.Orange
        };
        private static Vector2[] points = {new(0,0),   new(0.5f, 0),  new(1,0),
                                           new(0.25f, 0.5f),  new(0.75f,0.5f),
                                           new(0, 1),  new(0.5f, 1), new(1, 1)};
        public VertexShaderTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Depth = -100000;
            Effects = data.Attr("effects").Split(',');
            Activated = data.Bool("alwaysOn", defaultValue: true);
            Clear = data.Bool("clear");
            Vertices = new VertexPositionColor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vertices[i] = new(new(points[i] * new Vector2(320, 180f), 0), colors[i]);
            }
        }
        public static void DrawIndexedVertices<T>(Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Effect effect, BlendState blendState = null) where T : struct, IVertexType
        {
            BlendState blendState2 = ((blendState != null) ? blendState : BlendState.AlphaBlend);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            effect.ApplyMatrixParameters(matrix);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }
        public override void OnEnter(Player player)
        {
            Activated = true;
        }

    }
}
