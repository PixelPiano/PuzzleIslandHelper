using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/3dRotator")]
    [Tracked]
    public class PolygonDrawing : Entity
    {
        public PolygonDrawing(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16, 16);
            Add(new DotX3(Collider, p => p.Scene.Add(new RotatorDisplay(p))));
        }

        public class Mouse : MouseActor
        {
            public RotatorDisplay Parent;
            public Mouse(RotatorDisplay parent) : base(Vector2.Zero, 16, 16)
            {
                Parent = parent;
            }
            public override void OnDrag(Vector2 position)
            {

            }

            public override void OnDragEnd(Vector2 position)
            {

            }

            public override void OnDragStart(Vector2 position)
            {

            }

            public override void OnLeftClicked(Vector2 position)
            {
                Parent.AddVertice(position);
            }

            public override void OnRightClicked(Vector2 position)
            {
                Parent.RemoveVertice();
            }

            public override void OnScroll(float scrollValue)
            {

            }
            public override void Render()
            {
                base.Render();
                Draw.Rect(Position, 16, 16, Color.Red);
            }
        }
        [Tracked]
        public class RotatorDisplay : Entity
        {
            public Mouse Mouse;
            public static BasicEffect Shader;
            public Level Level;
            public Player Player;
            public List<int> Indices = new();
            public List<VertexPositionColor> Vertices = new();
            public float Scale = 6;
            private static VirtualRenderTarget _buffer;
            public static VirtualRenderTarget Buffer => _buffer ??= VirtualContent.CreateRenderTarget("RotatorDisplay", 320, 180);

            public RotatorDisplay(Player player)
            {
                Mouse = new Mouse(this);
                Level = player.Scene as Level;
                Level.Add(Mouse);
                Player = player;
                Add(new BeforeRenderHook(BeforeRender));
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Mouse.RemoveSelf();
            }
            public void AddVertice(Vector2 position)
            {
                Vector2 pos = position / 6f;
                Indices.Add(Vertices.Count);
                Vertices.Add(new VertexPositionColor(new Vector3(pos, 0), Color.Lime));
            }
            public void RemoveVertice()
            {
                if (Vertices.Count > 0)
                {
                    Vertices.RemoveAt(Vertices.Count - 1);
                    Indices.RemoveAt(Indices.Count - 1);
                }
            }
            public override void Update()
            {
                Position = Level.Camera.Position;
                base.Update();
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
                SamplerState prevSamplerState = Engine.Graphics.GraphicsDevice.SamplerStates[0];
                RasterizerState prevRasterizerState = Engine.Graphics.GraphicsDevice.RasterizerState;
                Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                GFX.DrawVertices(Matrix.Identity, Vertices.ToArray(), Vertices.Count);
                //RenderField(Scene.TimeActive, 0, 0);
                Engine.Graphics.GraphicsDevice.SamplerStates[0] = prevSamplerState;
                Engine.Instance.GraphicsDevice.RasterizerState = prevRasterizerState;

                if (Vertices != null)
                {
                    Draw.SpriteBatch.StandardBegin();
                    foreach (VertexPositionColor vertice in Vertices)
                    {
                        Draw.Rect(vertice.Position.XY(), 2, 2, Color.Lime);
                    }
                    Draw.SpriteBatch.End();
                }
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Buffer, Position, Color.White);
            }
            public void RenderField(float yaw, float pitch, float roll)
            {
                if (Vertices != null && Indices.Count > 2)
                {
                    /*                    Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                                        //Matrix world = Shader.World;
                                        Shader.World = *//*rotation * *//*Matrix.CreateScale(Scale) * Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0)*//* + Vector3.UnitZ * AdditionalZ*//*);
                                        foreach (EffectPass pass in Shader.CurrentTechnique.Passes)
                                        {
                                            pass.Apply();

                                            Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices.ToArray(), 0, Vertices.Count, Indices.ToArray(), 0, Indices.Count / 3);
                                        }
                                        Shader.World = Matrix.Identity;*/

                }
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
    [Tracked]
    public abstract class MouseActor : Actor
    {
        public MouseState LastState;
        public bool LeftClicked;
        public bool RightClicked;
        public bool Scrolling;
        public bool Moving;
        public bool Dragging => LeftClicked && Moving;
        private bool wasDragging;
        public Vector2 MousePosition;
        public Vector2 DragStartPosition;
        public Image Image;
        public MouseActor(Vector2 position, float width, float height, MTexture texture = null) : base(position)
        {
            Tag |= TagsExt.SubHUD;
            if (texture != null)
            {
                Image = new Image(texture);
                Add(Image);
            }
            Collider = new Hitbox(width, height);
        }
        public override void Render()
        {
            base.Render();
            if (Image == null)
            {
                Draw.Rect(Collider, Color.Red);
            }
        }
        public abstract void OnScroll(float scrollValue);
        public abstract void OnRightClicked(Vector2 position);
        public abstract void OnLeftClicked(Vector2 position);
        public abstract void OnDragStart(Vector2 position);
        public abstract void OnDrag(Vector2 position);
        public abstract void OnDragEnd(Vector2 position);
        public override void Update()
        {
            base.Update();

            MouseState state = Mouse.GetState();
            if (Engine.Instance.IsActive)
            {
                Scrolling = state.ScrollWheelValue != LastState.ScrollWheelValue;
                LeftClicked = state.LeftButton == ButtonState.Pressed && LastState.LeftButton == ButtonState.Released;
                RightClicked = state.RightButton == ButtonState.Pressed && LastState.RightButton == ButtonState.Released;
                Vector2 nextPosition = new Vector2(state.X, state.Y) * 2;
                Moving = Position != nextPosition;
                Position = nextPosition;
                if (Scrolling)
                {
                    OnScroll(state.ScrollWheelValue);
                }
                if (LeftClicked)
                {
                    OnLeftClicked(Position);
                }
                if (RightClicked)
                {
                    OnRightClicked(Position);
                }
                if (Dragging)
                {
                    if (!wasDragging)
                    {
                        DragStartPosition = Position;
                        OnDragStart(Position);
                    }
                    OnDrag(Position);
                }
                else if (wasDragging)
                {
                    OnDragEnd(Position);
                }
                LastState = state;
                wasDragging = Dragging;
            }
        }
    }
}