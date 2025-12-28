using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;

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
            public bool TakeSelected;
            public bool HasControl;
            public bool Cancelling;
            public bool Finished;
            private class input : GraphicsComponent
            {
                public Action Action;
                public string Text;
                public Vector2 Offset;
                public bool Disabled;
                private Color Color2;
                public Color Color1 = Color.White;
                public bool Selected;
                private float timer;
                public input(string text, Color secondaryColor, Action onSelect) : base(true)
                {
                    Text = text;
                    Action = onSelect;
                    Color2 = secondaryColor;
                    Color = Color1;
                }
                public void Select()
                {
                    if (!Selected)
                    {
                        Audio.Play("event:/ui/main/rollover_up");
                        Selected = true;
                        timer = 0.7f;
                        Color = Color2;
                    }
                }
                public void Deselect()
                {
                    if (Selected)
                    {
                        Selected = false;
                        Color = Color1;
                        timer = 0;
                    }
                }
                public void Confirm()
                {
                    Action?.Invoke();
                }
                public override void Update()
                {
                    base.Update();
                    if (timer > 0)
                    {
                        Offset.Y = 16 * Ease.CubeOut(timer / 0.7f);
                        timer -= Engine.DeltaTime;
                        if (timer <= 0)
                        {
                            Offset.Y = 0;
                            timer = 0;
                        }
                    }
                }
                public override void Render()
                {
                    base.Render();
                    ActiveFont.DrawOutline(Text, RenderPosition + Offset, new Vector2(0.5f, 0), Vector2.One, Disabled ? Color.DarkGray : Color, 1, Color.Black);
                }
            }
            private input store, take;
            public UI(FirfilContainer container) : base()
            {
                Tag |= TagsExt.SubHUD;
                Container = container;
                Target = VirtualContent.CreateRenderTarget("FirfilContainerUI", 1920, 1080);
                Add(new BeforeRenderHook(() =>
                {
                    Target.SetAsTarget(true);
                    if (Scene is Level level)
                    {
                        SubHudRenderer.BeginRender();
                        store.Render();
                        take.Render();
                        SubHudRenderer.EndRender();
                    }
                }));
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                store = new input("Store", Color.Magenta, Container.Store);
                take = new input("Take", Color.Magenta, Container.Take);
                Vector2 leftPos = new Vector2(1920 / 4f * 3, 1080 / 8f);
                Vector2 rightPos = new Vector2(1920 - 1920f / 4f, 1080 / 4f);
                if (Container != null)
                {
                    store.RenderPosition = (scene as Level).WorldToScreen(Container.Position + new Vector2(-8, -20));
                    take.RenderPosition = (scene as Level).WorldToScreen(Container.TopRight + new Vector2(8, -20));
                }
                else
                {
                    RemoveSelf();
                    return;
                }
                take.Visible = store.Visible = false;
                Add(store, take);

                Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.SineInOut, t =>
                {
                    Alpha = t.Eased;
                }, t =>
                {
                    HasControl = true;
                });
            }
            public override void Update()
            {
                base.Update();
                if (HasControl)
                {
                    bool canStore = Container.CanStore;
                    bool canTake = Container.CanTake;
                    store.Disabled = !canStore;
                    take.Disabled = !canTake;
                    if(store.Disabled != take.Disabled)
                    {
                        input disabled = store.Disabled ? store : take;
                        input enabled = store.Disabled ? take : store;
                        if (!enabled.Selected)
                        {
                            enabled.Select();
                        }
                        if (disabled.Selected)
                        {
                            disabled.Deselect();
                        }
                    }
                    if (Input.MenuCancel.Pressed && !Cancelling)
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
                    else if (Input.MenuConfirm.Pressed)
                    {
                        if (!store.Disabled && store.Selected)
                        {
                            Audio.Play("event:/ui/main/button_select");
                            store.Confirm();
                        }
                        else if (!take.Disabled && take.Selected)
                        {
                            Audio.Play("event:/ui/main/button_select");
                            take.Confirm();
                        }
                    }
                    else
                    {
                        if (!store.Disabled && !take.Disabled)
                        {
                            if (!store.Selected && Input.MenuLeft.Pressed)
                            {
                                store.Select();
                                take.Deselect();
                            }
                            else if (!take.Selected && Input.MenuRight.Pressed)
                            {
                                take.Select();
                                store.Deselect();
                            }
                        }
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Target, Vector2.UnitY * (540 - 540 * Alpha), Color.White * Alpha);
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
        public bool Collectable;
        private int scaleDirection = 1;
        public EntityID ID;
        public bool CanStore => Scene.GetPlayer() is Player player && player.Leader.HasFollower<Firfil>();
        public bool CanTake => FirfilStorage.Stored.Count > 0;
        public FlagList FlagOnCollected;
        public FlagList ExistFlag;
        public bool InstantCollect;
        public bool Persistent;
        public GetItemComponent GetItem;
        public FirfilContainer(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 1;
            ExistFlag = data.FlagList("flag");
            Collectable = data.Bool("collectable");
            Persistent = data.Bool("persistent");
            InstantCollect = data.Bool("instantCollect");
            FlagOnCollected = data.FlagList("flagOnCollected");
            ID = id;
            Container = new Image(GFX.Game["objects/PuzzleIslandHelper/firfil/container"]);
            Collider = Container.Collider();
            Add(Container);
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
            GetItem = new GetItemComponent((p) =>
            {
                if (Persistent)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(ID);
                }
            }, "FirfilContainer", true, "You got the Strange Container! Many have been scattered around the world.", "Exudes a flowery aroma. Some creatures may enjoy it...")
            {
                EntityOffset = new Vector2(-1, 1),
                RevertPlayerState = true,
            };
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Collectable)
            {
                Add(GetItem);
            }
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
        public override void Render()
        {
            if (!ExistFlag) return;
            base.Render();
        }
        public override void Update()
        {
            if (!ExistFlag)
            {
                GetItem.Active = false;
                Talk.Enabled = false;
                return;
            }
            GetItem.Active = Collectable;
            base.Update();
            Container.Scale = Vector2.One * (1f + scaleWiggle * scaleDirection);
            Player player = Scene.Tracker.GetEntity<Player>();
            Talk.Enabled = !Collectable && (FirfilStorage.Stored.Count > 0 || (player != null && player.Leader.HasFollower<Firfil>()));
        }
    }
}
