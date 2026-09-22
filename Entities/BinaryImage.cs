using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/BinaryImage")]
    public class BinaryImage : Entity
    {
        private class subHudEntity : Entity
        {
            public BinaryImage Parent;
            public Vector2 Scale;
            public subHudEntity(BinaryImage image) : base()
            {
                Parent = image;
                Tag |= TagsExt.SubHUD | Tags.TransitionUpdate;
                Scale = new Vector2(320, 180) / new Vector2(1920, 1080);
            }
            public override void Update()
            {
                base.Update();
            }
            public override void Render()
            {
                base.Render();
                if (Parent.IsVisible)
                {
                    Vector2 scale = Scale * Parent.Scale;
                    foreach (Digit d in Parent.Digits)
                    {
                        d.Texture.DrawCentered(d.RenderPosition, d.Color, scale);
                    }
                }
            }

        }
        public struct Digit
        {
            public MTexture Texture;
            public Color Color;
            public Vector2 Offset;
            public Vector2 RenderPosition;
        }
        public Digit[] Digits;
        public int Rows;
        public int Columns;
        public bool IsVisible;
        public Vector2 Scale = Vector2.One;
        public MTexture One, Zero;
        //private subHudEntity renderer;
        private Color onColor, offColor;
        private Color[] array;
        private Color[] ogArray;
        private string texturePath;
        public BinaryImage(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            onColor = data.HexColor("onColor", Color.Lime);
            offColor = data.HexColor("offColor", Color.Red);
            texturePath = data.Attr("texturePath");
        }
        public BinaryImage(Vector2 position, Texture2D texture, Color onColor, Color offColor) : base(position)
        {
            Tag |= Tags.TransitionUpdate;
            Depth = 1000;
            Rows = texture.Height;
            Columns = texture.Width;
            array = new Color[texture.Width * texture.Height];
            texture.GetData(array);
            ogArray = new Color[array.Length];
            array.CopyTo(ogArray, 0);
            this.onColor = onColor;
            this.offColor = offColor;
        }
        public BinaryImage(Vector2 position, Color[] array, int rows, int columns, Color onColor, Color offColor) : base(position)
        {
            Tag |= Tags.TransitionUpdate;
            Depth = 1000;
            this.array = array;
            ogArray = new Color[array.Length];
            array.CopyTo(ogArray, 0);
            this.onColor = onColor;
            this.offColor = offColor;
            Rows = rows;
            Columns = columns;
        }
        public void SetData(Color[] array, int columns, Color on, Color off)
        {
            Digits = new Digit[array.Length];
            int x = 0;
            int y = 0;
            for (int i = 0; i < array.Length; i++)
            {
                Digit digit = new();
                if (array[i] == Color.Black)
                {
                    digit.Texture = Zero;
                    digit.Color = off;
                }
                else
                {
                    digit.Texture = One;
                    digit.Color = on;
                }
                digit.Offset = new Vector2(x, y);
                Digits[i] = digit;
                x++;
                if (x >= columns)
                {
                    x = 0;
                    y++;
                }
            }
            UpdateDigits();
        }
        public void SetData(Texture2D texture, Color on, Color off)
        {
            Rows = texture.Height;
            Columns = texture.Width;
            Color[] array = new Color[Rows * Columns];
            texture.GetData(array);
            SetData(array, texture.Width, on, off);

        }
        public bool GetIsVisible()
        {
            return CullHelper.IsRectangleVisible(X, Y, Columns * Scale.X, Rows * Scale.Y, 0);
        }
        public override void Update()
        {
            base.Update();
            IsVisible = GetIsVisible();
            UpdateDigits();
        }
        public void UpdateDigits()
        {
            for (int i = 0; i < Digits.Length; i++)
            {
                Digit d = Digits[i];
                //Digits[i].RenderPosition = SceneAs<Level>().WorldToScreen(Position + d.Offset + d.Texture.HalfSize());
                Digits[i].RenderPosition = Position + d.Offset;
            }
        }
        public override void Render()
        {
            base.Render();
            if (IsVisible)
            {
                foreach (Digit d in Digits)
                {
                    d.Texture.Draw(d.RenderPosition, Vector2.Zero, d.Color, Scale);
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (array == null || array.Length == 0)
            {
                if (string.IsNullOrEmpty(texturePath))
                {
                    RemoveSelf();
                    return;
                }
                else
                {
                    MTexture tex = GFX.Game[texturePath];
                    if (tex == null || tex.Texture.Texture_Safe == null)
                    {
                        RemoveSelf();
                        return;
                    }
                    Rows = tex.Texture.Texture_Safe.Height;
                    Columns = tex.Texture.Texture_Safe.Width;
                    array = new Color[Rows * Columns];
                    tex.Texture.Texture_Safe.GetData(array);
                    ogArray = new Color[array.Length];
                    array.CopyTo(ogArray, 0);
                }
            }
            //scene.Add(renderer = new subHudEntity(this));
            One = GFX.Game["objects/PuzzleIslandHelper/binaryImage/one"];
            Zero = GFX.Game["objects/PuzzleIslandHelper/binaryImage/zero"];
            SetData(array, Columns, onColor, offColor);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            //renderer.RemoveSelf();
        }
    }
}