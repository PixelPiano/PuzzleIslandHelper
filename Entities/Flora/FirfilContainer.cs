using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.PuzzleIslandHelper.Entities.CutsceneHeart;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/FirfilContainer")]
    [Tracked]
    public class FirfilContainer : Entity
    {
        public class UI : Entity
        {
            public FirfilContainer Container;
            public VirtualRenderTarget Target;
            public float Alpha = 1;
            public float Spacing = 8;
            public Color SelectedColor = Color.Magenta;
            public Color UnselectedColor = Color.White;
            public Color StoreColor;
            public Color TakeColor;
            public float TextY = 1920 / 8f;
            public bool TakeSelected;
            public bool HasControl;
            public bool Cancelling;
            private Tween colorTween;
            private Color flashColor = Color.Yellow;
            private float flashLerp;
            private float takeYOffset;
            private float storeYOffset;
            public bool Finished;
            private bool prevTake;
            private Tween takeOffset, storeOffset;
            public UI(FirfilContainer container) : base()
            {
                Tag |= TagsExt.SubHUD;
                Container = container;
                Target = VirtualContent.CreateRenderTarget("FirfilContainerUI", 1920, 1080);
                StoreColor = SelectedColor;
                TakeColor = UnselectedColor;
                Add(new BeforeRenderHook(() =>
                {
                    Target.SetAsTarget(Color.Black);
                    if (Scene is Level level)
                    {
                        float width = ActiveFont.Measure("Store").X + Spacing * 5;
                        Color s = StoreColor;
                        Color t = TakeColor;
                        if (TakeSelected)
                        {
                            t = Color.Lerp(t, flashColor, flashLerp);
                        }
                        else
                        {
                            s = Color.Lerp(s, flashColor, flashLerp);
                        }
                        Draw.SpriteBatch.Begin();
                        DrawPhrase("Store", new Vector2(1920 / 4f * 3 - width / 2, TextY + storeYOffset), Spacing, s);
                        DrawPhrase("Take", new Vector2(1920 / 4f - width / 2, TextY + takeYOffset), Spacing, t);
                        Draw.SpriteBatch.End();
                    }
                }));
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t =>
                {
                    Alpha = t.Eased;
                }, t =>
                {
                    HasControl = true;
                });
                takeOffset = Tween.Create(Tween.TweenMode.YoyoOneshot, Ease.SineOut, 0.3f, false);
                takeOffset.OnUpdate = t =>
                {
                    takeYOffset = 16f * (1 - t.Eased);
                };
                storeOffset = Tween.Create(Tween.TweenMode.YoyoOneshot, Ease.SineOut, 0.3f, false);
                storeOffset.OnUpdate = t =>
                {
                    storeYOffset = 16f * (1 - t.Eased);
                };
            }
            public override void Update()
            {
                base.Update();
                prevTake = TakeSelected;
                if (HasControl)
                {
                    bool canStore = Container.CanStore;
                    if (Input.MenuCancel && !Cancelling)
                    {
                        Cancelling = true;
                        HasControl = false;
                        Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t =>
                        {
                            Alpha = 1 - t.Eased;
                        }, t =>
                        {
                            RemoveSelf();
                        });
                    }
                    else if (Input.MenuLeft)
                    {
                        colorTween.Stop();
                        flashLerp = 0;
                        TakeSelected = true;
                        TakeColor = SelectedColor;
                        StoreColor = UnselectedColor;
                    }
                    else if (Input.MenuRight)
                    {
                        if (!canStore)
                        {
                            TakeSelected = true;
                            TakeColor = SelectedColor;
                            StoreColor = Color.DarkGray;
                        }
                        else
                        {
                            colorTween.Stop();
                            flashLerp = 0;
                            TakeSelected = false;
                            TakeColor = UnselectedColor;
                            StoreColor = SelectedColor;
                        }
                    }
                    else if (Input.MenuConfirm.Pressed)
                    {
                        colorTween.Start();
                        Audio.Play("event:/ui/main/button_select");
                        if (TakeSelected)
                        {
                            Container.Take();
                        }
                        else
                        {
                            Container.Store();
                        }
                    }
                    if (!canStore && TakeSelected)
                    {
                        TakeSelected = true;
                        TakeColor = SelectedColor;
                        StoreColor = Color.DarkGray;
                    }
                    if (prevTake != TakeSelected)
                    {
                        //Audio hit
                        if (TakeSelected)
                        {
                            Audio.Play("event:/ui/main/rollover_down");
                            takeOffset.RemoveSelf();
                            Add(takeOffset);
                            takeOffset.Start();
                        }
                        else
                        {
                            Audio.Play("event:/ui/main/rollover_up");
                            storeOffset.RemoveSelf();
                            Add(storeOffset);
                            storeOffset.Start();
                        }
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White * Alpha);
            }
            public void DrawPhrase(string text, Vector2 position, float spacing, Color color)
            {
                Vector2 offset = Vector2.Zero;
                foreach (char c in text)
                {
                    ActiveFont.Draw(c, position + offset, new Vector2(0.5f, 1f), Vector2.One, color);
                    offset.X += ActiveFont.Measure(c).X + spacing;
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
                Finished = true;
            }
        }
        public Image Container;
        public TalkComponent Talk;
        public Wiggler ScaleWiggler;
        private float scaleWiggle;
        private int scaleDirection = 1;
        public bool CanStore => Scene.GetPlayer() is Player player && player.Leader.HasFollower<Firfil>();
        public FirfilContainer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Add(Container = new Image(GFX.Game["objects/PuzzleIslandHelper/firfil/container"]));
            Collider = Container.Collider();
            Container.JustifyOrigin(0.5f, 1);
            Container.Position += Container.Origin;
            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Container.Width, (int)Container.Height), Vector2.UnitX * Container.Width / 2, p =>
            {
                Input.Dash.ConsumePress();
                p.DisableMovement();
                Add(new Coroutine(routine(p)));
            }));
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                scaleWiggle = f * 0.3f;
            }));
        }
        private IEnumerator routine(Player player)
        {
            UI ui = new UI(this);
            Scene.Add(ui);
            while (!ui.Finished) yield return null;
            player.EnableMovement();
        }
        public void Take()
        {
            FirfilStorage.Take();
            ScaleWiggler.Start();
            scaleDirection = -1;
        }
        public void Store()
        {
            if (Scene.GetPlayer() is Player player)
            {
                var list = player.Leader.GetFollowers<Firfil>();
                if (list != null && list.Count > 0)
                {
                    Follower follower = list.First();
                    Firfil f = follower.Entity as Firfil;
                    FirfilStorage.Store(f);
                    player.Leader.LoseFollower(follower);
                    f.RemoveSelf();
                    ScaleWiggler.Start();
                    scaleDirection = 1;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            Container.Scale = Vector2.One * (1f + scaleWiggle * scaleDirection);
        }
    }
}
