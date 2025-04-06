using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class WarpNode3DBeta : Entity
    {
        public static ObjModel Model;
        public static VertexPositionTexture[] OrigVerts;
        public VirtualRenderTarget Target;
        public VertexPositionColor[] FakeVertices;
        public VertexPositionColor[] FakeProjected;
        private float roll;
        private float pitch;
        private float yaw;
        public static BasicEffect Shader;

        public static void Initialize()
        {
            Model = ObjModel.Create(Path.Combine(Engine.AssemblyDirectory, "Mods\\PuzzleIslandHelper\\Models", "WarpNode.obj"));
            var v = Model.verts;
            OrigVerts = new VertexPositionTexture[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                Model.verts[i].Position *= 16;
                OrigVerts[i] = new VertexPositionTexture(v[i].Position, v[i].TextureCoordinate);
            }
        }
        public WarpNode3DBeta(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16, 16);
            Depth = -1000000;
            Target = VirtualContent.CreateRenderTarget("Test", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
        }
        public void BeforeRender()
        {
            RealDrawBeforeRender();
        }
        public void RealDrawBeforeRender()
        {
            Target.SetAsTarget(true);
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/testModelTex"];
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            Shader.Texture = tex.Texture.Texture_Safe;
            Model.Draw(Shader);
        }
        public void FakeDrawBeforeRender()
        {
            Target.SetAsTarget(true);
            Matrix t = Matrix.CreateTranslation(new Vector3(160, 90, 0));
            Matrix s = Matrix.CreateScale(new Vector3(1, 1, 0));
            Draw.SpriteBatch.StandardBegin(Matrix.Identity, null, null);
            GFX.DrawVertices(t * s, FakeProjected, FakeProjected.Length);
            Draw.SpriteBatch.End();
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            FakeVertices = new VertexPositionColor[OrigVerts.Length];
            FakeProjected = new VertexPositionColor[OrigVerts.Length];
            for (int i = 0; i < OrigVerts.Length; i++)
            {
                //Console.WriteLine("Point " + i + ": " + ModelVertices[i].ToString());
                FakeVertices[i] = new VertexPositionColor(OrigVerts[i].Position /*+ new Vector3(Position, 0)*/, Color.Green.RandomShade(0.5f));
                FakeProjected[i] = new VertexPositionColor(FakeVertices[i].Position * 16, FakeVertices[i].Color);
            }
        }
        public override void Update()
        {
            base.Update();
            roll += Engine.DeltaTime;
            pitch += Engine.DeltaTime * 2;
            Shader.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(new Vector3(160, -30, 0));
            UpdateVertices();
            Shader.World *= Matrix.CreateScale(new Vector3(1, 1, 0));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
        public void UpdateVertices()
        {
            /*            Matrix r = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                        for (int i = 0; i < FakeProjected.Length; i++)
                        {
                            VertexPositionColor vert = FakeVertices[i];
                            Vector3 pos = vert.Position;
                            Vector3 rotated = Vector3.Transform(pos, r);
                            FakeProjected[i].Position = rotated * 16;
                            FakeProjected[i].Color = GetZColor(rotated.Z, Color.Green);
                            Vector3 rotated2 = Vector3.Transform(OrigVerts[i].Position, r);
                            Model.verts[i].Position = rotated2 * 16;
                        }*/

        }
        public Color GetZColor(float z, Color color)
        {
            float avg = z / 4f;
            Color to = avg < 0 ? Color.Lerp(color, Color.Black, 0.5f) : Color.White;
            float lerp = Math.Abs(avg);
            Color newColor = Color.Lerp(color, to, lerp);
            return newColor;
        }

    }
}
