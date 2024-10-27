using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Backdrops;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/CubeField")]
    public class CubeField : Backdrop
    {
        public static BasicEffect Shader;
        private static VirtualRenderTarget buffer;
        private const int CubeAreaWidth = 320;
        private const int CubeAreaHeight = 180;
        public static VirtualRenderTarget Buffer => buffer ??= VirtualContent.CreateRenderTarget("cube_renderer", 320, 180);
        public List<CubeFieldCube> Cubes = new();
        private float cubeSize;
        private float spacing = 16;
        private bool started;
        private Color color;
        private float alpha;
        private string path;
        public CubeField(BinaryPacker.Element data) : base()
        {
            int maxLayers = data.AttrInt("layers", 3);
            cubeSize = data.AttrFloat("cubeSize");
            alpha = data.AttrFloat("alpha");
            color = Calc.HexToColor(data.Attr("color", "FFFFFF")) * alpha;
            path = data.Attr("texturePath");
            int xCubes = (int)(CubeAreaWidth / (cubeSize + spacing));
            int yCubes = (int)(CubeAreaHeight / (cubeSize + spacing));

            for (int z = maxLayers; z > 0; z--)
            {
                for (float i = 0; i < xCubes; i++)
                {
                    float x = (cubeSize + spacing) * i;
                    for (float j = 0; j < yCubes; j++)
                    {
                        float y = (cubeSize + spacing) * j;
                        Vector3 pos = new Vector3(x, y + (cubeSize + spacing) / 2, -z * (cubeSize + spacing));
                        float dist = Vector2.Distance(new Vector2(pos.X, pos.Y), new Vector2(160, 90));

                        CubeFieldCube cube = new CubeFieldCube(pos, path, cubeSize)
                        {
                            ZLayer = z,
                            MaxLayers = maxLayers,
                            Color = color,
                            AdditionalZ = dist
                        };
                        Cubes.Add(cube);
                    }
                }
            }
        }

        public override void Update(Scene scene)
        {
            if (!started)
            {
                foreach (CubeFieldCube c in Cubes)
                {
                    scene.Add(c);
                }
                started = true;
            }
            base.Update(scene);
        }

        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            buffer?.Dispose();
            buffer = null;
            foreach (CubeFieldCube c in Cubes)
            {
                scene.Remove(c);
            }
        }
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            foreach (CubeFieldCube c in Cubes)
            {
                c.RenderCube();
            }
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            Draw.SpriteBatch.Draw(Buffer, Vector2.Zero, Color.White);
        }
        [OnInitialize]
        internal static void Initialize()
        {
            Shader = new(Engine.Graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                View = Matrix.CreateLookAt(new(0, 0, 160), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), Engine.Viewport.AspectRatio, 0.1f, 1000f),
            };
        }

        [OnUnload]
        internal static void Unload()
        {
            Shader?.Dispose();
            Shader = null;
        }

    }
}