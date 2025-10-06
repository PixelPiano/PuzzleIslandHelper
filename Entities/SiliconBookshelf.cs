using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.SiliconBookshelf.BookLine;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SiliconBookshelf")]
    public class SiliconBookshelf : Entity
    {
        public class BookLine : GraphicsComponent
        {
            public class Book
            {
                public bool Visible = true;
                public Color Color = Color.White;
                public Color FrontColor;
                public Color BackColor;
                public VirtualRenderTarget Texture;
                public Vector2 Position;
                public bool Prepared;
                public int FrontWidth;
                public int BackWidth;
                private Color flashColor;
                private float flashTimer;
                public bool Flashing;
                private float flashLimit;
                public void Dispose()
                {
                    Texture?.Dispose();
                }
                public Book(int frontWidth, int backWidth, int height, Vector2 position, Color frontColor, Color? flashColor = null)
                {
                    FrontColor = Color = frontColor;
                    this.flashColor = flashColor ?? Color.White;
                    FrontWidth = frontWidth;
                    BackWidth = backWidth;
                    BackColor = Color.Lerp(frontColor, Color.Black, 0.3f);
                    Position = position;
                    Texture = VirtualContent.CreateRenderTarget("booklineBook", frontWidth + backWidth, height);
                }
                public void Flash(float time)
                {
                    flashTimer = time;
                    flashLimit = time;
                    Flashing = true;
                }
                public void Update()
                {
                    if (flashTimer > 0)
                    {
                        Color = Color.Lerp(FrontColor, flashColor, Ease.SineOut(flashTimer / flashLimit));
                        flashTimer -= Engine.DeltaTime;
                        if (flashTimer <= 0)
                        {
                            flashTimer = 0;
                            Color = FrontColor;
                            Flashing = false;
                        }
                    }
                }
                public void Render()
                {
                    Draw.SpriteBatch.Draw(Texture, Position, Color);
                }
                public void Prepare()
                {
                    if (!Prepared)
                    {
                        Texture.SetAsTarget(true);
                        Draw.SpriteBatch.Begin();
                        Draw.Rect(0, 0, BackWidth, Texture.Height, BackColor);
                        Draw.Rect(BackWidth, 0, FrontWidth, Texture.Height, FrontColor);
                        Draw.SpriteBatch.End();
                        Prepared = true;
                    }
                }
            }
            public List<Book> Books = [];
            public bool RenderedOnce;
            public float Width;
            public float Height;
            public float BookWidthMin;
            public float BookWidthMax;
            public Color[] FrontColors;
            public int Seperation;
            private float interval;
            public BookLine(Vector2 position, int seperationWidth, float width, float height, float bookWidthRangeMin, float bookWidthRangeMax, Color[] frontColors) : base(true)
            {
                Seperation = seperationWidth;
                Position = position;
                Width = width;
                Height = height;
                BookWidthMin = bookWidthRangeMin;
                BookWidthMax = bookWidthRangeMax;
                FrontColors = frontColors;
            }

            public override void Added(Entity entity)
            {
                base.Added(entity);
                interval = Calc.Random.Range(0.2f, 1);
                entity.Add(new BeforeRenderHook(BeforeRender));
                float x = 0;
                while (x + 2 < Width)
                {
                    Book book = CreateBook(x);
                    Books.Add(book);
                    book.Visible = Calc.Random.Chance(0.96f);
                    x += 2 + Seperation;
                }
            }
            public Book CreateBook(float x)
            {
                int height = (int)Calc.Random.Range(3, Height);
                Color c = FrontColors.Random();
                Color b = Color.Lerp(c, Color.Black, 0.4f);
                Book book = new Book(1, 1, height, RenderPosition + new Vector2(x, Height - height), c);
                return book;
            }
            private void BeforeRender()
            {
                foreach (var b in Books)
                {
                    if (!b.Prepared)
                    {
                        b.Prepare();
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                foreach (var b in Books)
                {
                    if (b.Visible && b.Texture != null)
                    {
                        b.Render();
                    }
                }
            }
            public override void Update()
            {
                base.Update();
                foreach (Book b in Books)
                {
                    b.Update();
                }
                if (Scene.OnInterval(interval))
                {
                    PopBook();
                }
            }
            public void PopBook()
            {
                //causes a null reference error in Book.Render() for some reason.
/*                for (int i = Books.Count - 1; i > 0; i--)
                {
                    Vector2 p = Books[i].Position;
                    Books[i] = Books[i - 1];
                    Books[i].Position = p;
                }
                Books[0].Dispose();
                Books.RemoveAt(0);
                Books.Insert(0, CreateBook(0));
                Books[0].Visible = Calc.Random.Chance(0.96f);*/
            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                foreach (var b in Books)
                {
                    b.Dispose();
                }
            }
        }
        public Image Base;
        public List<Image> Sides = [];
        public List<Image> Backs = [];
        public List<Image> Books = [];
        public List<Image> Tops = [];
        public List<BookLine> BookLines = [];
        public Image Front;
        public int Padding = 16;
        public float GlowEase;
        public float GlowDirection;
        public class ShinyImage : Image
        {
            private float lerp = 0;
            private Color ColorA, ColorB;
            private Coroutine coroutine;
            public ShinyImage(MTexture texture, float delay, float waitTime, float lerpTime, Color colorA, Color colorB) : base(texture, true)
            {
                coroutine = new Coroutine(routine(delay, waitTime, lerpTime));
                ColorA = colorA;
                ColorB = colorB;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                entity.Add(coroutine);
            }
            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                entity.Remove(coroutine);
            }
            private IEnumerator routine(float delay, float waitTime, float lerpTime)
            {
                if (delay > 0)
                {
                    yield return delay;
                }

                while (true)
                {
                    yield return PianoUtils.Lerp(Ease.SineInOut, lerpTime, f => lerp = f);
                    yield return waitTime;
                    yield return PianoUtils.ReverseLerp(Ease.SineInOut, lerpTime, f => lerp = f);
                    yield return waitTime;
                }
            }
            public override void Update()
            {
                base.Update();
                Color = Color.Lerp(ColorA, ColorB, lerp);
            }
        }
        public SiliconBookshelf(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 2;
            Add(Base = new Image(GFX.Game["objects/PuzzleIslandHelper/siliconBookshelf/bookshelf"]));
            Base.Visible = false;
            Collider = Base.Collider();
            var a = GFX.Game.GetAtlasSubtextures("objects/PuzzleIslandHelper/siliconBookshelf/side");
            Color colorA = data.HexColor("colorA", Color.DarkGray);
            Color colorB = data.HexColor("colorB", Color.LightGray);
            float delay = Calc.Random.Range(0, 1f);
            foreach (var i in a)
            {
                ShinyImage s = new ShinyImage(i, delay, 1, 2f, colorA, colorB);
                Add(s);
                Sides.Add(s);
                delay += 0.7f;
                if (delay > 6f)
                {
                    delay %= 6;
                }
            }
            delay = Calc.Random.Range(0, 1f);
            var b = GFX.Game.GetAtlasSubtextures("objects/PuzzleIslandHelper/siliconBookshelf/backs");
            foreach (var i in b)
            {
                ShinyImage s = new ShinyImage(i, delay, 0.16f, 2f, colorA, colorB);
                Add(s);
                Backs.Add(s);
                delay += 0.8f;
                if (delay > 6f)
                {
                    delay %= 6;
                }
            }
            delay = Calc.Random.Range(0, 1f);
            var d = GFX.Game.GetAtlasSubtextures("objects/PuzzleIslandHelper/siliconBookshelf/top");
            foreach (var i in d)
            {
                ShinyImage s = new ShinyImage(i, delay, 0.5f, 0.1f, Color.Lerp(colorA, Color.White, 0.35f), colorB);
                Add(s);
                Tops.Add(s);
                delay += 0.1f;
            }
            Front = new ShinyImage(GFX.Game["objects/PuzzleIslandHelper/siliconBookshelf/front00"], 2, 2, 1.2f, colorA, colorB);
            Add(Front);
            for (int i = 0; i < 5; i++)
            {
                BookLine line = new BookLine(Vector2.One * 8 + Vector2.UnitY * 9 * i, 0, 31, 6, 1, 1, [Color.Gray, Color.DarkGray, Color.DimGray]);
                Add(line);
                BookLines.Add(line);
            }
        }
        public override void Update()
        {
            base.Update();
            foreach (var line in BookLines)
            {
                foreach (Book book in line.Books)
                {
                    if (!book.Flashing && Calc.Random.Chance(0.05f))
                    {
                        book.Flash(0.7f);
                    }
                }
            }
        }

        public override void Render()
        {
            if (this.OnScreen())
            {
                base.Render();
            }
        }
    }
}
