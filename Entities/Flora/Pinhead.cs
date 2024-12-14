using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Pinhead")]
    [Tracked]
    public class Pinhead : Entity
    {
        public bool OnScreen;
        public static readonly Vector2[] BasePoints = new Vector2[]
        {
            new(0, 0.5f),new(0.5f, 0),new(0.5f, 1)
        };
        public static readonly Vector2[] SpikePoints = new Vector2[]
        {
            new(0, 0),new(0.5f, 0.5f),new(0, 1)
        };
        public static readonly int[] Indices = new int[] { 0, 1, 2 };

        public VertexPositionColor[] BaseVertices;
        public VertexPositionColor[] SpikeVertices;
        public enum SpikeDirections
        {
            Up,
            Down,
            Left,
            Right
        }
        public Rectangle Rect;
        public Hitbox SpikeHitbox;
        public SpikeDirections Direction;
        public Vector2 BasePosition
        {
            get
            {
                return Direction switch
                {
                    SpikeDirections.Up => BottomLeft,
                    SpikeDirections.Left => TopRight,
                    _ => Position
                }; ;
            }
        }
        public Vector2 SpikePosition
        {
            get
            {
                return Direction switch
                {
                    SpikeDirections.Down => new(0, 8),
                    SpikeDirections.Right => new(8, 0),
                    _ => Position
                };
            }
        }
        public Pinhead(EntityData data, Vector2 offset) : this(data.Position + offset, data.Enum<SpikeDirections>("direction"), data.Width)
        {
 
        }
        public Pinhead(Vector2 position, SpikeDirections direction, int length) : base(position)
        {
            Depth = 1;
            BaseVertices = PianoUtils.Initialize((VertexPositionColor)default, BasePoints.Length);
            SpikeVertices = PianoUtils.Initialize((VertexPositionColor)default, SpikePoints.Length);
            Direction = direction;
            int i = (int)direction;
            int width = 8, height = 8;
            if (i < 2) height = length - 8;
            else width = length - 8;


            Collider = new Hitbox(width, height);
            Rect = new Rectangle((int)X, (int)Y, width, height);
            switch (direction)
            {
                case SpikeDirections.Down:
                    Collider.Position.Y += 8;
                    break;
                case SpikeDirections.Right:
                    Collider.Position.X += 8;
                    break;
            }
            AddTag(Tags.TransitionUpdate);
            UpdateVertices();
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            OnScreen = level.Camera.GetBounds().Contains(Rect);
            UpdateVertices();
        }

        public void UpdateVertices()
        {
            Vector2 basePos = BasePosition;
            Vector2 spikePos = SpikePosition;
            for (int i = 0; i < BasePoints.Length; i++)
            {
                BaseVertices[i].Position = new(basePos + BasePoints[i] * 8, 0);
            }
            for (int i = 0; i < SpikePoints.Length; i++)
            {
                SpikeVertices[i].Position = new(spikePos + SpikePoints[i] * Collider.Size, 0);
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || !OnScreen) return;
            Draw.SpriteBatch.End();
            GFX.DrawIndexedVertices(level.Camera.Matrix, BaseVertices, BaseVertices.Length, Indices, 1);
            GFX.DrawIndexedVertices(level.Camera.Matrix, SpikeVertices, SpikeVertices.Length, Indices, 1);
            GameplayRenderer.Begin();
        }
    }
}
