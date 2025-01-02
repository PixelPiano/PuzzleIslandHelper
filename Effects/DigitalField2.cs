using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{

    [CustomBackdrop("PuzzleIslandHelper/DigitalField2")]
    public class DigitalField2 : Backdrop
    {
        public VertexPositionColor[] Vertices;
        public string Effect;
        public int Rows = 8;
        public int PerRow = 8;
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
        private List<int> lineIndices = new();
        private LineMesh<VertexPositionColor> mesh;
        private static VirtualRenderTarget buffer;
        public DigitalField2(BinaryPacker.Element data) : base()
        {
            UseSpritebatch = false;
            Effect = data.Attr("effect");
            Vertices = new VertexPositionColor[points.Length];
            mesh = PianoUtils.CreateTriLineWallMesh(new(0), 320, 180, 16, 16, 0, GetColor, null, createVertex, out _);
            mesh.Bake();
        }
        private VertexPositionColor createVertex(Vector3 position, Color color)
        {
            return new VertexPositionColor(position, color);
        }
        private Color GetColor(Vector2 uv)
        {
            float amount = Vector2.Distance(uv, Vector2.One * 0.5f);

            Color color = Color.Lerp(Color.Blue, Color.Red, uv.Length());
            return color;
        }
        [OnUnload]
        public static void Unload()
        {
            buffer?.Dispose();
            buffer = null;
        }
        [OnInitialize]
        public static void Initialize()
        {
            buffer = VirtualContent.CreateRenderTarget("DigitalField2", 320, 180);
        }
        public bool Debug => PianoModule.Session.DEBUGBOOL1;
        public override void Render(Scene scene)
        {
            BackdropRenderer background = (scene as Level).Background;
            RenderTarget2D renderTarget2D = GameplayBuffers.Level;
            RenderTargetBinding[] renderTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();
            if (renderTargets.Length != 0)
            {
                renderTarget2D = (renderTargets[0].RenderTarget as RenderTarget2D) ?? renderTarget2D;
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            if (Debug)
            {
                background.StartSpritebatch(BlendState.Additive);
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var v = mesh.Vertices[i];
                    Draw.Rect(v.Position.X - 1, v.Position.Y - 1, 3, 3, Color.Blue * 0.8f);
                }
                background.EndSpritebatch();
            }
            else
            {
                Effect effect = ShaderHelperIntegration.GetEffect(Effect);
                Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                EffectParameterCollection parameters = effect.Parameters;
                Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
                Matrix matrix = Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
                matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
                matrix *= Matrix.CreateRotationX((float)Math.PI / 3f);
                Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                parameters["World"]?.SetValue(matrix);
                parameters["Time"]?.SetValue(scene.TimeActive);
                EffectTechnique effectTechnique = effect.Techniques[0];
                foreach (EffectPass pass in effectTechnique.Passes)
                {
                    pass.Apply();
                    mesh.Draw();
                }
            }
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            Engine.Instance.GraphicsDevice.SetRenderTarget(renderTarget2D);
            background.StartSpritebatch(BlendState.AlphaBlend);
            Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, Color.White);
            background.EndSpritebatch();
        }
    }
}

