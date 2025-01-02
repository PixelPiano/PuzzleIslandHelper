using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class ClickbaitGenerator : Entity
    {
        public static List<Color> Colors = [Color.Red, Color.Lime, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan, Color.White, Color.YellowGreen, Color.DeepPink];
        public const int UniqueArrows = 4;
        public const int UniqueReactions = 7;
        public const int UniqueBorders = 3;
        public static string path = "objects/PuzzleIslandHelper/clickbait/";
        public VirtualRenderTarget Output;
        public Color Color = Color.White;
        public bool Exported;
        public float Alpha = 1;
        public string Name;
        private float timer;
        public class TargetEdit : GraphicsComponent
        {
            public List<Image> Arrows = [];
            public Image Circle;
            public List<Rectangle> ArrowBounds = [];
            public enum ColorMode
            {
                AllSame,
                EachDifferent,
                LikeGroups
            }
            public bool ArrowCollide(Vector2 position)
            {
                foreach (Rectangle r in ArrowBounds)
                {
                    if (r.Contains(position))
                    {
                        return true;
                    }
                }
                return false;
            }
            public TargetEdit(Vector2 target, int arrows, ColorMode mode) : base(true)
            {
                Color circleColor = Colors.Random();
                Color arrowColor = Colors.Random();
                Circle = new Image(GFX.Game[path + "circle"], true)
                {
                    Color = circleColor
                };
                Circle.CenterOrigin();
                Circle.Scale = new Vector2(Calc.Random.Range(0.7f, 1.5f), Calc.Random.Range(0.7f, 1.5f));
                Circle.Position = target;

                float circlength = Circle.HalfSize().Length();
                for (int i = 0; i < arrows; i++)
                {
                    Image arrow = new Image(GFX.Game[path + "arrows/arrow" + Calc.Random.Range(1, UniqueArrows + 1)]);
                    arrow.CenterOrigin();
                    float angle = Calc.Random.NextAngle();
                    arrow.Rotation = angle;
                    arrow.Position = target + Calc.AngleToVector(angle, circlength + arrow.HalfSize().Length() * Calc.Random.Range(0.5f, 1.5f));
                    arrow.Color = mode switch
                    {
                        ColorMode.AllSame => circleColor,
                        ColorMode.EachDifferent => Colors.Random(),
                        _ => arrowColor
                    };
                    int w = (int)(arrow.Width * arrow.Scale.X * 0.2f);
                    int h = (int)(arrow.Height * arrow.Scale.Y * 0.2f);
                    Rectangle r = new Rectangle((int)arrow.X - w / 2, (int)arrow.Y - h / 2, w, h);
                    bool skip = false;
                    foreach (Rectangle r2 in ArrowBounds)
                    {
                        if (r.Colliding(r2))
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (!skip)
                    {
                        ArrowBounds.Add(r);
                        Arrows.Add(arrow);
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                Circle.Render();
                foreach (Image arrow in Arrows)
                {
                    arrow.Render();
                }
            }
        }
        public class TextEdit : GraphicsComponent
        {
            public string Text;
            public FancyText.Text fancytext;
            public TextEdit(Vector2 position, string text, float rotation, float scale, int maxLineWidth, Color? defaultColor = null) : base(true)
            {
                Rotation = rotation;
                Scale = Vector2.One * scale;
                Position = position;
                fancytext = FancyText.Parse(text, maxLineWidth, int.MaxValue, 1, defaultColor);
            }
            public override void Render()
            {
                base.Render();
                Draw(fancytext, Position, Vector2.Zero, Scale, 1, Color.White, Color.Black);
            }
            public void Draw(FancyText.Text text, Vector2 position, Vector2 justify, Vector2 scale, float alpha, params Color[] outlines)
            {
                int start = 0, end = int.MaxValue;
                int num = Math.Min(text.Nodes.Count, end);
                int num2 = 0;
                float num3 = 0f;
                float num4 = 0f;
                PixelFontSize pixelFontSize = text.Font.Get(text.BaseSize);
                for (int i = start; i < num; i++)
                {
                    if (text.Nodes[i] is FancyText.NewLine)
                    {
                        if (num3 == 0f)
                        {
                            num3 = 1f;
                        }

                        num4 += num3;
                        num3 = 0f;
                    }
                    else if (text.Nodes[i] is FancyText.Char)
                    {
                        num2 = Math.Max(num2, (int)(text.Nodes[i] as FancyText.Char).LineWidth);
                        num3 = Math.Max(num3, (text.Nodes[i] as FancyText.Char).Scale);
                    }
                    else if (text.Nodes[i] is FancyText.NewPage)
                    {
                        break;
                    }
                }

                num4 += num3;
                position -= justify * new Vector2(num2, num4 * (float)pixelFontSize.LineHeight) * scale;
                num3 = 0f;
                for (int j = start; j < num && !(text.Nodes[j] is FancyText.NewPage); j++)
                {
                    if (text.Nodes[j] is FancyText.NewLine)
                    {
                        if (num3 == 0f)
                        {
                            num3 = 1f;
                        }

                        position.Y += (float)pixelFontSize.LineHeight * num3 * scale.Y;
                        num3 = 0f;
                    }

                    if (text.Nodes[j] is FancyText.Char)
                    {
                        FancyText.Char @char = text.Nodes[j] as FancyText.Char;
                        DrawChar(@char, text.Font, text.BaseSize, position, scale, alpha, outlines);
                        num3 = Math.Max(num3, @char.Scale);
                    }
                }
            }
            public void DrawChar(FancyText.Char c, PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, params Color[] outlines)
            {
                Color color = c.Color;
                if (57344 <= c.Character && c.Character <= Emoji.Last && !Emoji.IsMonochrome((char)c.Character))
                {
                    Color = new Color(c.Color.A, c.Color.A, c.Color.A, c.Color.A);
                }

                float num = (c.Impact ? (2f - c.Fade) : 1f) * c.Scale;
                Vector2 zero = Vector2.Zero;
                Vector2 vector = scale * num;
                PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
                PixelFontCharacter pixelFontCharacter = pixelFontSize.Get(c.Character);
                vector *= baseSize / pixelFontSize.Size;
                position.X += c.Position * scale.X;
                zero += (c.Shake ? (new Vector2(-1 + Calc.Random.Next(3), -1 + Calc.Random.Next(3)) * 2f) : Vector2.Zero);
                zero += (c.Wave ? new Vector2(0f, (float)Math.Sin((float)c.Index * 0.25f + Engine.Scene.RawTimeActive * 8f) * 4f) : Vector2.Zero);
                zero.X += pixelFontCharacter.XOffset;
                zero.Y += (float)pixelFontCharacter.YOffset + (-8f * (1f - c.Fade) + c.YOffset * c.Fade);
                pixelFontCharacter.Texture.DrawOutline(position + zero * vector, Vector2.Zero, c.Color * c.Fade * alpha, vector, c.Rotation);
                //pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, c.Color * c.Fade * alpha, vector, c.Rotation);
                c.Color = color;
            }
        }

        public ClickbaitGenerator(string name, float timer = 0) : base()
        {
            Tag |= TagsExt.SubHUD;
            Name = name;
            Output = VirtualContent.CreateRenderTarget("clickbait_output:" + name, 1920, 1080);
            Add(new BeforeRenderHook(BeforeRender));
            this.timer = timer;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Image border = new Image(GFX.Game[path + "borders/border" + Calc.Random.Range(1, UniqueBorders + 1)]);
            Add(border);
            border.Color = Colors.Random();
            Level level = scene as Level;
            var onScreen = level.EntitiesOnScreen(-8);

            for (int i = 0; i < Calc.Random.Range(1, 3); i++)
            {
                Entity target = Calc.Random.Chance(0.5f) ? level.GetPlayer() : onScreen.Random();
                TargetEdit t = new(target != null ? (target.Center - level.Camera.Position) * 6 : new Vector2(Calc.Random.Range(8, 313), Calc.Random.Range(8, 172)) * 6, Calc.Random.Range(1, 6), (TargetEdit.ColorMode)Calc.Random.Range(0, 3));
                Add(t);
            }
            Add(new Image(GFX.Game[path + "vignette"]) { Color = Color.White * 0.7f });
            Image reaction = new Image(GFX.Game[path + "reactions/reaction" + Calc.Random.Range(1, UniqueReactions + 1)]);
            Add(reaction);
            string id = "clickbait" + Calc.Random.Range(0, 5);
            TextEdit te = new(new Vector2(Calc.Random.Range(60, 240), Calc.Random.Range(20, 160)) * 2,
                Dialog.Get(id), Calc.Random.NextAngle() * 0.2f, Calc.Random.Range(1.5f, 1.8f), (int)(1920 / 2.5f));
            Add(te);
        }
        public override void Update()
        {
            base.Update();
            timer -= Engine.DeltaTime;
        }
        public void BeforeRender()
        {
            if (!Exported && !Engine.Commands.Open && timer <= 0)
            {
                Output.SetAsTarget(Color.Transparent);
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, null, Color.White, 0, Vector2.One, 6, SpriteEffects.None, 0);
                RenderEdits();
                Draw.SpriteBatch.End();

                Export();
            }
        }
        public void RenderEdits()
        {
            base.Render();
        }
        public override void Render()
        {
        }
        public void Export()
        {
            PianoUtils.SaveTargetAsPng((RenderTarget2D)Output, "Clickbait/" + Name + ".png", 0, 0, 1920, 1080);
            RemoveSelf();
            Exported = true;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }
        public void Dispose()
        {
            Output?.Dispose();
            Output = null;
        }
        [Command("create_clickbait", ":3")]
        public static void Record(string name, float timer = 0)
        {
            if (Engine.Scene is Level level)
            {
                level.Add(new ClickbaitGenerator(name, timer));
            }
        }
    }
}