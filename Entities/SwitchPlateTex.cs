using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    //note to self stop messing with this evil evil evil evil thing evil
    [Tracked]
    public class SwitchPlateTex : Entity
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
        public MTexture pipeTexture;
        public Direction direction;
        public List<Image> Images = new();
        public List<Image> Indents = new();
        public List<FillAnim> Bolts = new();
        public List<Sprite> Corners = new();
        public Color SpriteColor = Color.White;
        public Color IndentColor = Color.White;
        public float Rate = 0.1f;
        public Vector2 CurrentPosition;
        private int boltCount => Bolts.Count;

        [Tracked]
        public class FillAnim : Image
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
            public FillAnim(MTexture tex, float rate, Method revealMethod) : base(tex, true)
            {
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
                Texture?.Draw(RenderPosition + new Vector2((int)XOffset, (int)YOffset), Origin, Color, Scale, Rotation, new Rectangle((int)XOffset, (int)YOffset, (int)ClipWidth + xOff, (int)ClipHeight + yOff));
            }
        }
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

            MTexture tex = GetTextureQuad(pipeTexture, CornerTextureQuads[cornerType.Key]);
            bool flipped = cornerType.Key is "upLeft" or "upRight";
            bool scaled = cornerType.Key is "upLeft" or "downLeft";
            string flip = flipped ? "Flip" : "";
            Image image = new Image(tex);
            Image image2 = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/cornerIndent" + flip]);
            Sprite sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/drillMachine/");
            sprite.AddLoop("idle", "cornerFill" + cornerType.Value, 0.1f, 1);
            sprite.Add("fill", "cornerFill" + cornerType.Value, 0.1f * Rate, "idle", 0, 1);
            image2.CenterOrigin();
            sprite.CenterOrigin();
            sprite.Color = SpriteColor;
            image2.Color = IndentColor;
            image2.Position = sprite.Position = Vector2.One * 4;
            if (flipped)
            {
                sprite.Scale.Y = -1;
            }
            if (scaled)
            {
                image2.Scale.X = sprite.Scale.X = -1;
            }
            Add(image, image2, sprite);
            Images.Add(image);
            Indents.Add(image2);
            Corners.Add(sprite);
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
            List<FillAnim> fillAnims = new();
            List<Image> indents = new();
            FillAnim.Method method = DirectionToMethod(pipeExitDirection);
            if (horizontal)
            {
                if (pipeExitDirection == Direction.Right) offset = length - 8;

                for (int i = 0; i < num; i += 8)
                {
                    MTexture textureQuad;
                    textureQuad = GetTextureQuad(pipeTexture, StraightTextureQuads["horizontal"]);
                    Image image3 = new Image(textureQuad);
                    Image image4 = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/indentFlip"]);
                    image4.X = image3.X = i + num2 - offset;
                    Add(image3, image4);

                    images.Add(image3);
                    indents.Add(image4);


                    FillAnim sprite = new FillAnim(GFX.Game["objects/PuzzleIslandHelper/drillMachine/indentFlipFilled"], Rate, method);
                    sprite.X = image3.X;
                    Add(sprite);
                    sprite.Color = SpriteColor;
                    image4.Color = IndentColor;
                    fillAnims.Add(sprite);
                }

            }
            else if (vertical)
            {
                if (pipeExitDirection == Direction.Down) offset = length - 8;
                for (int k = 0; k < num; k += 8)
                {
                    MTexture textureQuad2;
                    textureQuad2 = GetTextureQuad(pipeTexture, StraightTextureQuads["vertical"]);

                    Image image6 = new Image(textureQuad2);
                    Image image4 = new Image(GFX.Game["objects/PuzzleIslandHelper/drillMachine/indent"]);
                    image4.Y = image6.Y = k + num2 - offset;
                    Add(image6, image4);
                    images.Add(image6);
                    indents.Add(image4);

                    FillAnim sprite = new FillAnim(GFX.Game["objects/PuzzleIslandHelper/drillMachine/indentFilled"], Rate, method);
                    sprite.Y = image6.Y;
                    Add(sprite);
                    sprite.Color = SpriteColor;
                    image4.Color = IndentColor;
                    fillAnims.Add(sprite);

                }
            }
            if (horizontal || vertical)
            {
                if (pipeExitDirection is Direction.Left or Direction.Up)
                {
                    images.Reverse();
                    fillAnims.Reverse();
                    indents.Reverse();
                }
                foreach (Image i in images)
                {
                    Images.Add(i);
                }
                foreach (Image i in indents)
                {
                    Indents.Add(i);
                }
                foreach (FillAnim fill in fillAnims)
                {
                    Bolts.Add(fill);
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
        public SwitchPlateTex(Vector2 position, float width, float height, float length, float pipeWidth, Vector2 startNode, Vector2 endNode, Vector2 nextNode, bool startNodeExit, bool endNodeExit, Color indentColor, Color spriteColor)
            : base(position)
        {
            IndentColor = indentColor;
            SpriteColor = spriteColor;
            Collider = new Hitbox(width, height, 0, 0);
            this.startNode = startNode;
            this.endNode = endNode;
            this.nextNode = nextNode;
            this.startNodeExit = startNodeExit;
            this.endNodeExit = endNodeExit;
            this.length = length;
            this.pipeWidth = pipeWidth;
            pipeTexture = GFX.Game["objects/PuzzleIslandHelper/drillMachine/plates"];
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
        public static SwitchPlateTex FromNodes(Vector2 startNode, Vector2 endNode, Vector2 nextNode, bool startNodeExit, bool endNodeExit, float pipeWidth, Color indentColor, Color spriteColor)
        {

            Direction pipeExitDirection = GetPipeExitDirection(endNode, startNode);
            float num = (endNode - startNode).Length();
            Vector2 position = endNode;
            float num3 = 8f;
            float num4 = 8f;

            return new SwitchPlateTex(position, num3, num4, num, pipeWidth, startNode, endNode, nextNode, startNodeExit, endNodeExit, indentColor, spriteColor);
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
            foreach (FillAnim fill in Bolts)
            {
                fill.Color = SpriteColor;
            }
            foreach (Sprite s in Corners)
            {
                s.Color = SpriteColor;
            }
            foreach (Image i in Indents)
            {
                i.Color = IndentColor;
            }
        }
        public FillAnim.Method DirectionToMethod(Direction direction)
        {
            return direction switch
            {
                Direction.Up => FillAnim.Method.BottomToTop,
                Direction.Right => FillAnim.Method.LeftToRight,
                Direction.Down => FillAnim.Method.TopToBottom,
                Direction.Left => FillAnim.Method.RightToLeft,
                _ => FillAnim.Method.None
            };
        }

    }
}
