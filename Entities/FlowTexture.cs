using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Components.BitrailNode;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class FlowTexture : Entity
    {
        public static List<FlowTexture> Create(Scene scene, Vector2[] nodes, string path, string backTexture, string frontTexture, string cornerTexture, Color indentColor, Color fillColor)
        {
            List<FlowTexture> textures = [];
            Vector2 startNode = Vector2.Zero;
            bool flag = false;
            int num = 0;
            Vector2[] array = nodes;
            foreach (Vector2 endNode in array)
            {
                if (flag)
                {
                    bool startNodeExit = num == 1;
                    bool endNodeExit = num == nodes.Length - 1;
                    Vector2 nextNode = nodes.ElementAtOrDefault(num + 1);
                    FlowTexture tex = FromNodes(startNode, endNode, nextNode, path, backTexture, frontTexture, cornerTexture, startNodeExit, endNodeExit, 8, indentColor, fillColor);
                    textures.Add(tex);
                    scene.Add(tex);
                }
                flag = true;
                startNode = endNode;
                num++;
            }
            return textures;
        }



        public enum Direction
        {
            Up = -2,
            Down = -1,
            None = 0,
            Left = 1,
            Right = 2
        }
        public Vector2 startNode;
        public Vector2 endNode;
        public Vector2 nextNode;
        public bool startNodeExit;
        public bool endNodeExit;
        public float length;
        public float size;
        public MTexture texture;
        public Direction direction;
        public List<Fill> Fills = new();
        public List<Corner> Corners = new();
        public Color SpriteColor = Color.White;
        public Color IndentColor = Color.White;
        public float Rate = 0.1f;
        public Vector2 CurrentPosition;
        [Tracked]
        public class Fill : Image
        {
            public bool Animating;
            public float Amount;
            public float Rate = 1;
            public float MaxWidth;
            public float MaxHeight;
            public bool IgnoreWidth;
            public bool IgnoreHeight;
            public bool IgnoreX;
            public bool IgnoreY;
            public float ClipWidth;
            public float ClipHeight;
            public float XOffset;
            public float YOffset;
            private bool flipped;
            public Color BackColor = Color.White;
            public Color FrontColor = Color.White;
            public MTexture BackTexture;
            public MTexture FrontTexture;
            public enum Method
            {
                None,
                LeftToRight,
                RightToLeft,
                TopToBottom,
                BottomToTop,
                UpLeft,
                UpRight,
                BottomLeft,
                BottomRight
            }

            public Method RevealMethod;
            public bool Finished
            {
                get { return Amount == 1; }
                set { Amount = value ? 1 : 0; }
            }
            public Fill(MTexture front, MTexture back, MTexture fill, float rate, Method revealMethod) : base(fill, true)
            {
                FrontTexture = front;
                BackTexture = back;
                Rate = rate;
                if (Texture != null)
                {
                    MaxWidth = Texture.Width;
                    MaxHeight = Texture.Height;
                }
                RevealMethod = revealMethod;

                IgnoreHeight = RevealMethod is Method.LeftToRight or Method.RightToLeft;
                IgnoreWidth = RevealMethod is Method.TopToBottom or Method.BottomToTop;
                IgnoreX = RevealMethod is Method.LeftToRight or Method.UpLeft or Method.UpRight || IgnoreWidth;
                IgnoreY = RevealMethod is Method.TopToBottom or Method.BottomLeft or Method.BottomRight || IgnoreHeight;
            }
            public void Animate()
            {
                if (RevealMethod is Method.None) return;
                Animating = true;
            }
            public void Stop(bool snapValue)
            {
                if (snapValue)
                {
                    Amount = 1;
                }
                Animating = false;
            }
            public override void Update()
            {
                base.Update();
                if (Animating)
                {
                    Amount = Calc.Min(1, Amount + Engine.DeltaTime / Rate);
                    if (Finished)
                    {
                        Stop(true);
                    }
                }
                ClipWidth = MaxWidth * (IgnoreWidth ? 1 : Amount);
                ClipHeight = MaxHeight * (IgnoreHeight ? 1 : Amount);
                XOffset = MaxWidth - MaxWidth * (IgnoreX ? 1 : Amount);
                YOffset = MaxHeight - MaxHeight * (IgnoreY ? 1 : Amount);
            }
            public bool IsDiagonal()
            {
                return !IgnoreHeight && !IgnoreWidth;
            }
            public override void Render()
            {
                int xOff = Animating ? 1 : 0;
                int yOff = Animating ? 1 : 0;
                Rectangle clip = new Rectangle((int)XOffset, (int)YOffset, (int)ClipWidth + xOff, (int)ClipHeight + yOff);
                Vector2 position = RenderPosition + new Vector2((int)XOffset, (int)YOffset);
                BackTexture?.Draw(position, Origin, BackColor, Scale, Rotation);
                FrontTexture?.Draw(position, Origin, FrontColor, Scale, Rotation);
                Texture?.Draw(position, Origin, Color, Scale, Rotation, clip);
            }
        }
        public class Corner : Sprite
        {
            public Color BackColor = Color.White;
            public Color FrontColor = Color.White;
            public Vector2 SpriteScale = Vector2.One;
            public Vector2 FrontScale = Vector2.One;
            public Vector2 BackScale = Vector2.One;
            public Vector2 BackOrigin = Vector2.Zero;
            public MTexture FrontTexture;
            public MTexture BackTexture;
            public bool Activated;
            public Vector2 Offset;
            public Corner(Vector2 position, MTexture frontTexture, MTexture backTexture, string spritePath, string cornerType, float rate, bool flipped, bool scaled) : base(GFX.Game, spritePath)
            {
                Offset = position;
                BackTexture = backTexture;
                FrontTexture = frontTexture;
                AddLoop("idle", "cornerFill" + cornerType, 0.1f, 3);
                Add("fill", "cornerFill" + cornerType, 0.1f * rate, "idle", 0, 1, 2, 3);
                //CenterOrigin();
                if (flipped) SpriteScale.Y = -1;
                if (scaled) FrontScale.X = SpriteScale.X = -1;
            }
            public override void Render()
            {
                Vector2 p = RenderPosition;
                BackTexture?.Draw(p, BackOrigin, BackColor, BackScale * Scale, 0, Effects);
                FrontTexture?.Draw(p + Offset, Origin, FrontColor, FrontScale * Scale, 0, Effects);
                if (Activated)
                {
                    Texture?.Draw(p, Origin, Color, SpriteScale * Scale, Rotation, Effects);
                }
            }
        }
        public string Path;
        public string FrontSuffix;
        public string CornerSuffix;
        public static Dictionary<string, Vector2> StraightTextureQuads = new Dictionary<string, Vector2>
        {
            {"vertical",new Vector2(0f,1f)},
            {"horizontal", new Vector2(0f,0f)}
        };
        public static Dictionary<string, Vector2> CornerTextureQuads = new Dictionary<string, Vector2>
        {
            {"upLeft", new Vector2(2f,1f)},
            {"upRight",new Vector2(2f,0f)},
            {"downRight",new Vector2(1f,1f)},
            {"downLeft",new Vector2(1f,0f)}
        };
        public FlowTexture(Vector2 position, float width, float height, float length, float size, string path, string textureName, string frontSuffix, string cornerSuffix, Vector2 startNode, Vector2 endNode, Vector2 nextNode, bool startNodeExit, bool endNodeExit, Color emptyColor, Color fillColor)
    : base(position)
        {
            IndentColor = emptyColor;
            SpriteColor = fillColor;
            Collider = new Hitbox(width, height, 0, 0);
            this.startNode = startNode;
            this.endNode = endNode;
            this.nextNode = nextNode;
            this.startNodeExit = startNodeExit;
            this.endNodeExit = endNodeExit;
            this.length = length;
            this.size = size;
            FrontSuffix = frontSuffix;
            CornerSuffix = cornerSuffix;
            Path = path;
            texture = GFX.Game[path + textureName];
            Depth = 2;
            addVisuals();
            Tag |= Tags.TransitionUpdate;
        }

        public void addCornerVisuals(Direction direction, Direction nextDirection)
        {
            if (length < size) return;

            if (TryGetCornerType(direction, nextDirection, out (string, string) cornerType))
            {
                bool flipped = cornerType.Item1.Contains("up");
                bool scaled = cornerType.Item1.Contains("Left");
                string flip = flipped ? "Flip" : "";
                string path = Path;// "objects/PuzzleIslandHelper/drillMachine/"; //make global
                string frontSuffix = CornerSuffix;// "cornerIndent"; //make global
                MTexture frontTexture = GFX.Game[path + frontSuffix + flip];
                MTexture backTexture = GetTextureQuad(texture, CornerTextureQuads[cornerType.Item1]);
                Vector2 offset = Vector2.Zero;
                if(scaled) offset.X += 8;
                Corner corner = new Corner(offset, frontTexture, backTexture, path, cornerType.Item2, Rate, flipped, scaled);
                Add(corner);
                Corners.Add(corner);
            }
        }
        public void addVisuals()
        {
            Direction direction = GetExitDirection(endNode, startNode);
            Direction nextDirection = GetExitDirection(nextNode, endNode);
            this.direction = direction;
            bool horizontal = (int)direction > 0;
            bool vertical = (int)direction < 0;
            if (horizontal || vertical)
            {
                float totalLength = endNodeExit ? length : length - size;
                float imageOffset = 0;//!endNodeExit && (direction == Direction.Up || direction == Direction.Left) ? size : 0f;
                float offset = 0;
                List<Fill> fills = [];
                Fill.Method method = DirectionToMethod[direction];

                string path = Path;
                string frontSuffix = FrontSuffix;
                Vector2 mult = horizontal ? Vector2.UnitX : Vector2.UnitY;
                string dictionaryKey = horizontal ? "horizontal" : "vertical";
                if (horizontal)
                {
                    frontSuffix += "Flip";
                    if (direction == Direction.Right) offset = length - 8;
                }
                else if (direction == Direction.Down) offset = length - 8;

                for (int i = 0; i < totalLength; i += 8)
                {
                    MTexture backtex = GetTextureQuad(texture, StraightTextureQuads[dictionaryKey]);
                    MTexture fronttex = GFX.Game[path + frontSuffix];
                    MTexture filltex = GFX.Game[path + frontSuffix + "Filled"];
                    Fill fill = new Fill(fronttex, backtex, filltex, Rate, method);
                    fill.Position = mult * (i + imageOffset - offset);
                    Add(fill);
                    fill.Color = SpriteColor;
                    fill.FrontColor = IndentColor;
                    fills.Add(fill);
                }
                if (direction is Direction.Left or Direction.Up)
                {
                    fills.Reverse();
                }
                foreach (Fill fill in fills)
                {
                    Fills.Add(fill);
                }
            }
            if (!endNodeExit)
            {
                addCornerVisuals(direction, nextDirection);
            }
        }


        public static MTexture GetTextureQuad(MTexture texture, Vector2 position) => texture.GetSubtexture((int)position.X * 8, (int)position.Y * 8, 8, 8);
        public static Direction GetExitDirection(Vector2 exit, Vector2 previous)
        {
            if (exit.X < previous.X) return Direction.Left;
            if (exit.X > previous.X) return Direction.Right;
            if (exit.Y < previous.Y) return Direction.Up;
            if (exit.Y > previous.Y) return Direction.Down;
            return Direction.None;
        }
        public static FlowTexture FromNodes(Vector2 startNode, Vector2 endNode, Vector2 nextNode, string path, string textureName, string frontSuffix, string cornerSuffix, bool startNodeExit, bool endNodeExit, float size, Color indentColor, Color spriteColor)
        {
            Direction exit = GetExitDirection(endNode, startNode);
            float length = (endNode - startNode).Length();
            Vector2 position = endNode;
            float width = 8f;
            float height = 8f;
            return new FlowTexture(position, width, height, length, size, path, textureName, frontSuffix, cornerSuffix, startNode, endNode, nextNode, startNodeExit, endNodeExit, indentColor, spriteColor);
        }
        private static Direction getOpposite(Direction from)
        {
            return from switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => Direction.None
            };
        }
        public static bool TryGetCornerType(Direction direction, Direction nextDirection, out (string, string) pair)
        {
            pair = ("", "");
            if (direction is Direction.None || nextDirection is Direction.None) return false;
            if (direction == nextDirection) return false;

            int a = (int)direction;
            int b = (int)nextDirection;
            if (a == 0 || b == 0 || Math.Sign(a) == Math.Sign(b))
            {
                return false;
            }
            direction = getOpposite(direction);
            string orientation = b < 0 ? "Backwards" : "Forwards";
            string combo = a < b ? direction.ToString().ToLower() + nextDirection.ToString() : nextDirection.ToString().ToLower() + direction.ToString();
            Engine.Commands.Log(string.Format("CornerType: combo: {0}, orientation: {1}.", combo, orientation));
            pair = (combo, orientation);
            return true;
            /*            if (direction == Direction.Right && nextDirection == Direction.Up) return ("upLeft", "Backwards");
                        if (direction == Direction.Left && nextDirection == Direction.Up) return ("upRight", "Backwards");
                        if (direction == Direction.Left && nextDirection == Direction.Down) return ("downRight", "Backwards");
                        if (direction == Direction.Right && nextDirection == Direction.Down) return ("downLeft", "Backwards");
                        if (direction == Direction.Down && nextDirection == Direction.Right) return ("upRight", "Forward");
                        if (direction == Direction.Down && nextDirection == Direction.Left) return ("upLeft", "Forward");
                        if (direction == Direction.Up && nextDirection == Direction.Right) return ("downRight", "Forward");
                        if (direction == Direction.Up && nextDirection == Direction.Left) return ("downLeft", "Forward");*/
        }
        public override void Update()
        {
            base.Update();
            foreach (Fill fill in Fills)
            {
                fill.Color = SpriteColor;
                fill.FrontColor = IndentColor;
            }
            foreach (Corner s in Corners)
            {
                s.Color = SpriteColor;
                s.FrontColor = IndentColor;
            }
        }
        public static Dictionary<Direction, Fill.Method> DirectionToMethod = new()
        {
            {Direction.Up, Fill.Method.BottomToTop},
            {Direction.Right, Fill.Method.LeftToRight},
            {Direction.Down, Fill.Method.TopToBottom},
            {Direction.Left, Fill.Method.RightToLeft},
            {Direction.None, Fill.Method.None}
        };

    }
}
