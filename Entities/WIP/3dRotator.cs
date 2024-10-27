using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    //[CustomEntity("PuzzleIslandHelper/3dRotator")]
    [Tracked]
    public class Rotator3D : Entity
    {
        public Rotator3D(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16, 16);
            Add(new DotX3(Collider, p => p.Scene.Add(new RotatorDisplay(this, p))));
        }
        [Tracked]
        public class RotatorDisplay : Entity
        {
            public static BasicEffect Shader;
            public Level Level;
            public Player Player;
            public Rotator3D Parent;
            public Vector2[] Points = { new(0, 0), new(1, 0), new(0, 1) };
            public int[] indices = { 0, 1, 2 };
            public float Scale = 6;
            public VertexPositionColor[] Vertices;
            private static VirtualRenderTarget _buffer;
            public static VirtualRenderTarget Buffer => _buffer ??= VirtualContent.CreateRenderTarget("RotatorDisplay", 320, 180);

            public RotatorDisplay(Rotator3D parent, Player player)
            {
                Level = player.Scene as Level;
                Player = player;
                Parent = parent;
                Vertices = new VertexPositionColor[Points.Length];
                Add(new BeforeRenderHook(BeforeRender));
                CollectAvailableVertices();

            }
            public void CollectAvailableVertices()
            {
                Vector2 position = Position;
                for (int i = 0; i < Points.Length; i++)
                {
                    Vertices[i] = new(new Vector3(position + Points[i] * Scale, 0), Color.Lime);
                }
            }
            public override void Update()
            {
                Position = Level.Camera.Position;
                base.Update();
                UpdateVertices();
            }
            public void UpdateVertices()
            {
                Vector2 position = Position;
                for (int i = 0; i < Points.Length; i++)
                {

                }
            }
            public void BeforeRender()
            {
                Shader ??= new(Engine.Graphics.GraphicsDevice)
                {
                    TextureEnabled = false,
                    VertexColorEnabled = true,
                    View = Matrix.CreateLookAt(new(0, 0, 160), Vector3.Zero, Vector3.Up),
                    Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), Engine.Viewport.AspectRatio, 0.1f, 1000f),
                };
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                RenderField(Scene.TimeActive, 0, 0);
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Buffer, Position, Color.White);
            }
            public void RenderField(float yaw, float pitch, float roll)
            {
                Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                //Matrix world = Shader.World;
                Shader.World = rotation * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0)/* + Vector3.UnitZ * AdditionalZ*/);
                foreach (EffectPass pass in Shader.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, 3, indices, 0, 1);
                }
                Shader.World = Matrix.Identity;
            }
            private IEnumerator FadeTo(float from, float to, float time)
            {
                yield return null;
            }

            [OnUnload]
            internal static void Unload()
            {
                Shader?.Dispose();
                Shader = null;
            }
        }
    }
}