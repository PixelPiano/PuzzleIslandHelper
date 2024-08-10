using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class BitrailTex : Entity
    {
        public enum Direction
        {
            Up,
            Right,
            Down,
            Left,
            None
        }
        public Vector2 startNode;
        public Vector2 endNode;
        public Vector2 nextNode;
        public bool startNodeExit;
        public bool endNodeExit;
        public float length;
        public float pipeWidth;
        public MTexture railTexture;
        public Direction direction;
        public List<Image> Images = new();
        public List<Image> Corners = new();
        public float Rate = 0.1f;
        public Vector2 CurrentPosition;
        public void addCornerVisuals(Direction direction, Direction nextDirection)
        {
            if (length < pipeWidth)
            {
                return;
            }

            KeyValuePair<string, string> cornerType = getCornerType(direction, nextDirection);
            if (cornerType.Key == "unknown" || cornerType.Value == "unknown")
            {
                return;
            }

            MTexture tex = GetTextureQuad(railTexture, CornerTextureQuads[cornerType.Key]);
            Image image = new Image(tex);
            Add(image);
            Images.Add(image);
            Corners.Add(image);
        }
        public void addVisuals()
        {
            Direction pipeExitDirection = GetPipeExitDirection(endNode, startNode);
            direction = pipeExitDirection;
            Direction pipeExitDirection2 = GetPipeExitDirection(nextNode, endNode);
            bool horizontal = pipeExitDirection == Direction.Right || pipeExitDirection == Direction.Left;
            bool vertical = pipeExitDirection == Direction.Down || pipeExitDirection == Direction.Up;
            float num = !endNodeExit ? length - pipeWidth : length;
            float num2 = !endNodeExit && (pipeExitDirection == Direction.Up || pipeExitDirection == Direction.Left) ? pipeWidth : 0f;
            float offset = 0;
            List<Image> images = new();
            if (horizontal)
            {
                if (pipeExitDirection == Direction.Right) offset = length - 8;

                for (int i = 0; i < num; i += 8)
                {
                    MTexture textureQuad;
                    textureQuad = GetTextureQuad(railTexture, StraightTextureQuads["horizontal"]);
                    Image image3 = new Image(textureQuad);
                    image3.X = i + num2 - offset;
                    Add(image3);

                    images.Add(image3);
                }

            }
            else if (vertical)
            {
                if (pipeExitDirection == Direction.Down) offset = length - 8;
                for (int k = 0; k < num; k += 8)
                {
                    MTexture textureQuad2;
                    textureQuad2 = GetTextureQuad(railTexture, StraightTextureQuads["vertical"]);

                    Image image6 = new Image(textureQuad2);
                    image6.Y = k + num2 - offset;
                    Add(image6);
                    images.Add(image6);

                }
            }
            if (horizontal || vertical)
            {
                if (pipeExitDirection is Direction.Left or Direction.Up)
                {
                    images.Reverse();
                }
                foreach (Image i in images)
                {
                    Images.Add(i);
                }
            }

            if (!endNodeExit)
            {
                addCornerVisuals(pipeExitDirection, pipeExitDirection2);
            }
        }
        public static Dictionary<string, Vector2> StraightTextureQuads = new Dictionary<string, Vector2>
        {
            {
                "vertical",
                 new Vector2(0f,1f)
            },
            {
                "horizontal",
                new Vector2(0f,0f)
            }
        };

        public static Dictionary<string, Vector2> CornerTextureQuads = new Dictionary<string, Vector2>
        {
            {
                "upLeft", new Vector2(2f,1f)
            },
            {
                "upRight",new Vector2(2f,0f)

            },
            {
                "downRight",new Vector2(1f,1f)

            },
            {
                "downLeft",new Vector2(1f,0f)
            }
        };

        public static MTexture GetTextureQuad(MTexture texture, Vector2 position)
        {
            return texture.GetSubtexture((int)position.X * 8, (int)position.Y * 8, 8, 8);
        }


        public BitrailTex(Vector2 position, float width, float height, float length, float pipeWidth, Vector2 startNode, Vector2 endNode, Vector2 nextNode, bool startNodeExit, bool endNodeExit)
            : base(position)
        {
            Collider = new Hitbox(width, height, 0, 0);
            this.startNode = startNode;
            this.endNode = endNode;
            this.nextNode = nextNode;
            this.startNodeExit = startNodeExit;
            this.endNodeExit = endNodeExit;
            this.length = length;
            this.pipeWidth = pipeWidth;
            railTexture = GFX.Game["objects/PuzzleIslandHelper/bitRail/rails"];
            Depth = 2;
            addVisuals();
        }
        public static Direction GetPipeExitDirection(Vector2 exit, Vector2 previous)
        {
            if (exit.X < previous.X) return Direction.Left;
            if (exit.X > previous.X) return Direction.Right;
            if (exit.Y < previous.Y) return Direction.Up;
            if (exit.Y > previous.Y) return Direction.Down;
            return Direction.None;
        }
        public static BitrailTex FromNodes(Vector2 startNode, Vector2 endNode, Vector2 nextNode, bool startNodeExit, bool endNodeExit, float pipeWidth)
        {
            float num = (endNode - startNode).Length();
            Vector2 position = endNode;
            float num3 = 8f;
            float num4 = 8f;

            return new BitrailTex(position, num3, num4, num, pipeWidth, startNode, endNode, nextNode, startNodeExit, endNodeExit);
        }
        public static KeyValuePair<string, string> getCornerType(Direction direction, Direction nextDirection)
        {
            if (direction == Direction.Right && nextDirection == Direction.Up)
            {
                return new("upLeft", "Backwards");
            }
            if (direction == Direction.Down && nextDirection == Direction.Left)
            {
                return new("upLeft", "Forward");
            }
            if (direction == Direction.Down && nextDirection == Direction.Right)
            {
                return new("upRight", "Forward");
            }
            if (direction == Direction.Left && nextDirection == Direction.Up)
            {
                return new("upRight", "Backwards");
            }
            if (direction == Direction.Left && nextDirection == Direction.Down)
            {
                return new("downRight", "Backwards");
            }
            if (direction == Direction.Up && nextDirection == Direction.Right)
            {
                return new("downRight", "Forward");
            }

            if (direction == Direction.Up && nextDirection == Direction.Left)
            {
                return new("downLeft", "Forward");
            }
            if (direction == Direction.Right && nextDirection == Direction.Down)
            {
                return new("downLeft", "Backwards");
            }
            return new("unknown", "unknown");
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
