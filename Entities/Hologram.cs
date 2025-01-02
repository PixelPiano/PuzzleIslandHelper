using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

// PuzzleIslandHelper.LabFallingBlock
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Hologram")]
    [Tracked]
    public class Hollogram : Entity
    {
        public VertexPositionColor[] Vertices;
        public Vector2 VertexScale = Vector2.Zero;
        private Color[] colors;
        public readonly Vector2 ScaleTarget = new Vector2(20, 28);
        public Vector2[] Points = [new(-0.5f, -1), new(0, -1), new(0, 0), new(0.5f, -1)];
        public int[] indices = [0, 1, 2, 1, 3, 2];
        public bool Loading;
        public Effect Shader;
        private float lineAmount;
        public Sprite sprite;
        public bool RenderVertices;
        public float VertexAlpha;
        private Coroutine spriteRoutine;
        [TrackedAs(typeof(Solid))]
        public class ButtonSolid : Solid
        {
            public Vector2 OrigPosition;
            public Vector2 DepressedPosition;
            public enum FacingDirections
            {
                Left,
                Right,
                Up,
                Down
            }
            public FacingDirections Facing;
            public Action TurnOn;
            public Action TurnOff;
            private bool On, wasOn;
            public ButtonSolid(Vector2 position, MTexture texture, FacingDirections dir, Action turnOn, Action turnOff) : base(position, texture.Width, texture.Height, true)
            {
                TurnOn = turnOn;
                TurnOff = turnOff;
                OrigPosition = Position;
                DepressedPosition = dir switch
                {
                    FacingDirections.Left => TopLeft,
                    FacingDirections.Right => TopRight,
                    FacingDirections.Up => BottomLeft,
                    FacingDirections.Down => TopLeft,
                    _ => Position
                };
            }
            public override void Update()
            {
                base.Update();
                if (HasPlayerRider())
                {
                    MoveTowardsX(DepressedPosition.X, 8 * Engine.DeltaTime);
                    MoveTowardsY(DepressedPosition.Y, 8 * Engine.DeltaTime);
                }
                else
                {
                    MoveTowardsX(OrigPosition.X, 8 * Engine.DeltaTime);
                    MoveTowardsY(OrigPosition.Y, 8 * Engine.DeltaTime);
                }
                wasOn = On;
                On = Position == DepressedPosition;
                if (!wasOn && On)
                {
                    TurnOn.Invoke();
                }
                else if (wasOn && !On)
                {
                    TurnOff.Invoke();
                }
            }

        }
        public ButtonSolid Button;
        public Hollogram(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesOffset(offset)[0], data.HexColor("color"), data.Attr("decalPath"), data.Width, data.Height)
        {

        }
        public Hollogram(Vector2 position, Vector2 node, Color color, string decalPath, float width, float height) : base(position)
        {
            Color main = color;
            colors = [color, Color.Lerp(color, Color.White, 0.4f)];
            Tag |= Tags.TransitionUpdate;
            Depth = 1;
            sprite = new Sprite(GFX.Game, "decals/");
            sprite.AddLoop("idle", decalPath, 0.1f);
            sprite.RenderPosition = node;
            Add(sprite);
            sprite.Visible = false;
            Collider = new Hitbox(width, height);
            Vertices = new VertexPositionColor[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i] * Collider.Size, 0), Color.White);
            }
            Add(spriteRoutine = new Coroutine(false));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/button"];
            Button = new ButtonSolid(BottomLeft - Vector2.UnitY * tex.Height, tex, ButtonSolid.FacingDirections.Up, Press, Release);
            scene.Add(Button);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Button.RemoveSelf();
        }
        public override void Update()
        {
            base.Update();
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = new Vector3(TopCenter + Points[i] * VertexScale, 0);
                Vertices[i].Color = colors[i / 2] * VertexAlpha;
            }
        }
        public IEnumerator VertexFlickerRoutine(bool endState)
        {
            bool high = true;
            for (int i = 1; i < 16; i++)
            {
                float wait = i / 15f * 0.1f;
                if (high)
                {
                    VertexAlpha = Calc.Random.Range(0.6f, 0.9f);
                }
                else
                {
                    VertexAlpha = Calc.Random.Range(0.2f, 0.5f);
                }
                yield return wait;
                high = !high;
            }
            VertexAlpha = endState ? 1 : 0;
            yield return null;
        }
        public IEnumerator spriteFlickerRoutine(bool endState)
        {
            int loops = Calc.Random.Range(4, 8);
            for (int i = 0; i < loops; i++)
            {
                sprite.Visible = i % 2 == 0;
                sprite.Color = Color.White * Calc.Random.Range(0.1f, 0.6f);
                float mult = (float)i / loops;
                yield return Calc.Random.Range(mult * 0.5f, mult * 0.8f);
            }
            sprite.Visible = endState;
            sprite.Color = Color.White * (endState ? 1 : 0);
        }
        public void RevealSprite()
        {
            spriteRoutine.Replace(spriteFlickerRoutine(true));
        }
        public void HideSprite()
        {
            spriteRoutine.Replace(spriteFlickerRoutine(false));
        }
        public void Press()
        {
            Loading = true;
            Add(new Coroutine(VertexFlickerRoutine(true)));
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.Linear, t =>
            {
                VertexScale.X = Calc.LerpClamp(0, ScaleTarget.X, Ease.SineIn(t.Eased));
                VertexScale.Y = Calc.LerpClamp(0, ScaleTarget.Y, Ease.CubeIn(t.Eased * 2));
            }, t => RevealSprite());
        }
        public void Release()
        {
            Loading = false;
            HideSprite();
            Add(new Coroutine(VertexFlickerRoutine(false)));
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.Linear, t =>
            {
                VertexScale.X = Calc.LerpClamp(ScaleTarget.X, 0, Ease.SineIn(t.Eased));
                VertexScale.Y = Calc.LerpClamp(ScaleTarget.Y, 0, Ease.CubeIn(t.Eased * 2));
            });
        }
        public void ApplyParameters()
        {
            if (Shader != null)
            {
                Shader.ApplyCameraParams(Scene as Level);
                Shader.Parameters["LineOsc"]?.SetValue(lineAmount);
            }
        }
        public override void Render()
        {
            base.Render();

            if (VertexScale != Vector2.Zero)
            {
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, 4, indices, 2);
                GameplayRenderer.Begin();
            }
        }


    }

}