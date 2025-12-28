using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CustomEvent("PuzzleIslandHelper/AuthorIntro")]
    [Tracked]
    public class AuthorIntroCutscene : CutsceneEntity
    {
        [CustomEntity("PuzzleIslandHelper/AuthorIntroBook")]
        [Tracked]
        public class AuthorBook : Actor
        {
            public Vector2 Speed;
            public float Alpha = 1;
            public Image Image;
            public Vector2 Position1;
            public Vector2 Position2;
            public FlagData Flag;
            public bool HasGravity;
            public AuthorBook(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Flag = data.Flag("positionFlag");
                Position1 = Position;
                Position2 = data.NodesOffset(offset)[0];
                Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/redBook"]));
                Image.CenterOrigin();
                Image.Position += Image.HalfSize();
                Collider = Image.Collider();
                Tag |= Tags.TransitionUpdate;
                Depth = 1;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Position = Flag.GetState(scene) ? Position2 : Position1;
            }
            public override void Render()
            {
                if (Alpha > 0)
                {
                    Image.Color = Color.White * Alpha;
                    Image.Render();
                }
            }
            public override void Update()
            {
                base.Update();
                if (Speed.X != 0)
                {
                    MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                }
                if (Speed.Y != 0)
                {
                    MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                }
                if (HasGravity)
                {
                    Speed.Y = Calc.Approach(Speed.Y, 160f, (float)(900.0 * (double)Engine.DeltaTime));
                }
                Speed.X = Calc.Approach(Speed.X, 0, (float)(400.0 * (double)Engine.DeltaTime));
            }
            public void OnCollideH(CollisionData data)
            {
                if (data.Hit is Solid)
                {
                    while (CollideCheck<Solid>())
                    {
                        MoveH(-Math.Sign(Speed.X));
                    }
                    Speed.X = 0;

                }
            }
            public void OnCollideV(CollisionData data)
            {
                if (data.Hit is Solid)
                {
                    Speed.X *= 0.7f;
                    Speed.Y = 0;
                    while (CollideCheck<Solid>())
                    {
                        MoveV(-1);
                    }
                }
            }
        }
        private class AuthorBookInspect : Entity
        {
            public bool Finished;
            public MTexture Texture;
            public float Alpha = 0;
            private VirtualRenderTarget target;
            private Tween tween;
            private float savedBloom;
            public AuthorBookInspect() : base()
            {
                Depth = int.MinValue;
                target = VirtualContent.CreateRenderTarget("HudBookA", 320, 180);
                Add(new BeforeRenderHook(() =>
                {
                    target.SetAsTarget(Color.Black);
                    Draw.SpriteBatch.Begin();
                    Texture.Draw(Vector2.Zero);
                    Draw.SpriteBatch.End();
                }));
                tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => Alpha = t.Eased, t => tween.RemoveSelf());
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Finished = true;
                target?.Dispose();
                (scene as Level).Bloom.Strength = savedBloom;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                savedBloom = (scene as Level).Bloom.Strength;
            }
            public override void Update()
            {
                base.Update();
                SceneAs<Level>().Bloom.Strength = Calc.LerpClamp(savedBloom, 0, 1 - Alpha);
                if (tween.Scene == null)
                {
                    if (Input.MenuCancel.Pressed)
                    {
                        tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t => Alpha = 1 - t.Eased, t =>
                        {
                            Finished = true;
                            RemoveSelf();
                        });
                    }
                }
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Texture = GFX.Game["objects/PuzzleIslandHelper/hud/authorBookA"];
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(target, SceneAs<Level>().Camera.Position, Color.White * Alpha);
            }
        }
        public Author author;
        public Player Player;
        public AuthorBook Book;
        private AuthorBookInspect BookInspect;
        private bool faceAuthor;
        private float bookYOffset;
        private float playerTo;
        public bool BookCarried = true;
        public FlagData MentionedBook = new FlagData("AuthorIntroMentionedBook");
        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                Player = player;
                if (level.Tracker.GetEntity<Author>() is Author author)
                {
                    this.author = author;
                    bookYOffset = author.Height / 2;
                    if (level.Tracker.GetEntity<AuthorBook>() is AuthorBook book)
                    {
                        Book = book;
                        if (Marker.TryFind("player", out var v))
                        {
                            playerTo = v.X;
                            Add(new Coroutine(cutscene()));

                        }
                    }
                }
            }
        }
        public override void OnEnd(Level level)
        {
            BookInspect?.RemoveSelf();
            faceAuthor = false;
            Author.IntroOneWatched.State = true;
            author.Alpha = 1;
            Book.Position = Book.Position2;
            if (Marker.TryFind("authorDefault", out var p))
            {
                author.X = p.X;
            }
            author.Facing = Facings.Right;
            author.HasGravity = true;
            BookCarried = false;
            Book.HasGravity = true;
            Player.EnableMovement();
        }
        public override void Update()
        {
            base.Update();
            if (faceAuthor)
            {
                Player?.Face(author);
            }
            if (Book != null && BookCarried && !Author.IntroOneWatched)
            {
                if (author.Facing == Facings.Right)
                {
                    Book.Position = new Vector2(author.CenterX + (author.Width / 2) - Book.Width, author.Y + bookYOffset);
                }
                else
                {
                    Book.Position = new Vector2(author.CenterX - (author.Width / 2), author.Y + bookYOffset);
                }
                Book.Image.Scale.X = -(int)author.Facing;
            }
        }
        public IEnumerator cutscene()
        {
            Player.DisableMovement();
            author.Alpha = 1;

            if (!Author.IntroOneWatched)
            {
                Book.Depth = author.Depth - 1;
                yield return Player.DummyWalkTo(playerTo);
                yield return Textbox.Say("AuthorIntro", authorEnter, authorFaceRight, authorToLeft, authorToRight, throwBook, allJump);
                Author.IntroOneWatched.State = true;
            }
            else
            {
                yield return Player.DummyWalkTo(author.Right + 10);
                Player.Facing = Facings.Left;
            }
            bool abort = false;
            while (!abort)
            {
                List<string> options = ["AuthorIntroChoiceA", "AuthorIntroChoiceB"];
                if (MentionedBook) options.Add("AuthorIntroChoiceC");
                options.Add("AuthorIntroChoiceLeave");
                yield return ChoicePrompt.Prompt(options.ToArray());
                string dialogue = options[ChoicePrompt.Choice] + "Dialogue";
                switch (ChoicePrompt.Choice)
                {
                    case 0:
                        yield return Textbox.Say(dialogue);
                        break;
                    case 1:
                        yield return Textbox.Say(dialogue);
                        MentionedBook.State = true;
                        break;
                    case 2:
                        if (!MentionedBook)
                        {
                            abort = true;
                        }
                        else
                        {
                            yield return Textbox.Say(dialogue, inspectBook);
                        }
                        break;
                    case 3:
                        abort = true;
                        break;
                }

            }
            EndCutscene(Level);
        }
        private IEnumerator authorEnter()
        {
            if (Marker.TryFind("authorEntrance", out var vector))
            {
                author.Position.X = vector.X;
            }
            yield return 1f;
            faceAuthor = true;
            yield return null;
        }
        private IEnumerator inspectBook()
        {
            BookInspect = new();
            Level.Add(BookInspect);
            while (!BookInspect.Finished)
            {
                yield return null;
            }
            BookInspect.RemoveSelf();
            yield return null;
        }
        private IEnumerator authorFaceRight()
        {
            author.Facing = Facings.Right;
            yield return null;
        }
        private IEnumerator throwBook()
        {
            //author faces Maddy and Calidus, throws book on ground
            author.FacePlayer(Player);
            author.HasGravity = true;
            yield return 0.7f;
            Book.Depth = -1;
            Book.HasGravity = true;
            BookCarried = false;
            Book.Speed.X = 70f;
            Book.Speed.Y = -120f;
            while (!Book.CollideCheck<Solid>(Book.Position + Vector2.UnitY))
            {
                yield return null;
            }
            yield return 0.7f;
        }
        private IEnumerator authorToLeft()
        {
            if (Marker.TryFind("authorDefault", out var p))
            {
                yield return author.WalkToX(p.X);
            }
            yield return null;
        }
        private IEnumerator authorToRight()
        {
            if (Marker.TryFind("authorPace", out var p))
            {
                yield return author.WalkToX(p.X);
            }
            yield return null;
        }
        private IEnumerator allJump()
        {
            Player.Jump();
            author.Jump();
            yield return null;
        }
    }
}
