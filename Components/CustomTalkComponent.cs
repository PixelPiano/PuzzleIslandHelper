using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [TrackedAs(typeof(TalkComponent))]
    public class CustomTalkComponent : TalkComponent
    {

        public bool CanHover;
        public bool VisibleFromDistance;
        private Sprite sprite;
        public string Anim => Focused ? FocusedAnim : UnfocusedAnim;
        public string FocusedAnim;
        public string UnfocusedAnim
        {
            get
            {
                if (SingleGraphic) return FocusedAnim;
                else return unfocusedAnim;
            }
            set
            {
                unfocusedAnim = value;
            }
        }
        private string unfocusedAnim;
        public bool UsesSprite;
        public bool Focused;
        public MTexture Texture
        {
            get
            {
                if (Focused)
                {
                    return FocusedTexture;
                }
                else
                {
                    return UnfocusedTexture;
                }
            }
        }

        public MTexture FocusedTexture;
        public MTexture UnfocusedTexture
        {
            get
            {
                if (SingleGraphic) return FocusedTexture;
                else return unfocusedTexture;
            }
            set
            {
                unfocusedTexture = value;
            }
        }
        private MTexture unfocusedTexture;
        public bool SingleGraphic;
        public float Alpha;
        public float AlphaUpClose = 1;
        public float AlphaAtDistance = 1;

        public float Delay;
        public bool Muted;
        public bool Loop;
        public Vector2 SpriteOffset;
        internal enum SpecialType
        {
            DotDotDot,
            DigitalLog,
            UpArrow,
            DownArrow
        }
        internal CustomTalkComponent(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk, SpecialType type)
        : base(new Rectangle((int)x, (int)y, (int)width, (int)height), drawAt, onTalk, null)
        {
            switch (type)
            {
                case SpecialType.DotDotDot:
                    sprite = new Sprite(GFX.Gui, "PuzzleIslandHelper/hover/");
                    sprite.AddLoop("idle", "digitalC", 0.5f);
                    FocusedAnim = "idle";
                    UsesSprite = true;
                    Delay = 0.5f;
                    Muted = false;
                    Loop = true;
                    VisibleFromDistance = false;
                    CanHover = false;
                    break;
                case SpecialType.DigitalLog:
                    FocusedTexture = GFX.Gui["PuzzleIslandHelper/hover/digitalB"];
                    UsesSprite = false;
                    Muted = false;
                    VisibleFromDistance = false;
                    CanHover = true;
                    break;
                case SpecialType.UpArrow:
                    FocusedTexture = GFX.Gui["PuzzleIslandHelper/hover/upArrow"];
                    VisibleFromDistance = false;
                    CanHover = false;
                    break;
                case SpecialType.DownArrow:
                    FocusedTexture = GFX.Gui["PuzzleIslandHelper/hover/downArrow"];
                    VisibleFromDistance = false;
                    CanHover = false;
                    break;
            }
            Alpha = AlphaAtDistance;
            SingleGraphic = true;
        }
        internal CustomTalkComponent(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk)
: base(new Rectangle((int)x, (int)y, (int)width, (int)height), drawAt, onTalk, null)
        {
            Alpha = AlphaAtDistance;
            SingleGraphic = true;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
        }
        public CustomTalkComponent(Entity thisEntity, Action<Player> onTalk) : base(new Rectangle(0, 0, (int)thisEntity.Width, (int)thisEntity.Height), new Vector2(thisEntity.X + thisEntity.Width / 2, thisEntity.Y), onTalk)
        {
        }
        public CustomTalkComponent(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk, bool visibleFromDistance = true, bool canHover = true, Sprite highlightSprite = null, string animID = "", float delay = 0.1f, bool loop = false)
                : base(new Rectangle((int)x, (int)y, (int)width, (int)height), drawAt, onTalk, null)
        {
            CanHover = canHover;
            VisibleFromDistance = visibleFromDistance;

            if (highlightSprite is not null)
            {
                sprite = highlightSprite;
                Loop = loop;
                FocusedAnim = animID;
                Delay = delay;
                UsesSprite = true;
            }
            else
            {
                FocusedTexture = GFX.Gui["hover/idle"];
                UsesSprite = false;
            }
        }
        public CustomTalkComponent(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk, bool visibleFromDistance = true, bool canHover = true, MTexture highlightTexture = null)
 : base(new Rectangle((int)x, (int)y, (int)width, (int)height), drawAt, onTalk, null)
        {
            CanHover = canHover;
            VisibleFromDistance = visibleFromDistance;

            if (highlightTexture is not null)
            {
                FocusedTexture = highlightTexture;
                UsesSprite = false;
            }
            else
            {
                FocusedTexture = GFX.Gui["hover/idle"];
            }

        }
        private bool wasHighlighting;
        public override void Update()
        {
            if (UI == null)
            {
                Entity.Scene.Add(UI = new CustomTalkComponentUI(this));
                (UI as CustomTalkComponentUI).started = true;
                (UI as CustomTalkComponentUI).CurrentFrame = 0;
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            bool flag = disableDelay < 0.05f && entity != null && entity.CollideRect(new Rectangle((int)(base.Entity.X + (float)Bounds.X), (int)(base.Entity.Y + (float)Bounds.Y), Bounds.Width, Bounds.Height)) && entity.OnGround() && entity.Bottom < base.Entity.Y + (float)Bounds.Bottom + 4f && entity.StateMachine.State == 0 && (!PlayerMustBeFacing || Math.Abs(entity.X - base.Entity.X) <= 16f || entity.Facing == (Facings)Math.Sign(base.Entity.X - entity.X)) && (PlayerOver == null || PlayerOver == this);
            Focused = flag;
            if (flag)
            {
                hoverTimer += Engine.DeltaTime;
                Alpha = Calc.LerpClamp(Alpha, AlphaUpClose, Engine.DeltaTime * 5);
            }
            else
            {
                if (UI.Display) hoverTimer = 0f;
                Alpha = Calc.LerpClamp(Alpha, AlphaAtDistance, Engine.DeltaTime * 5);
            }
            if (UsesSprite)
            {
                sprite.Play(Anim);
            }
            if (PlayerOver == this && !flag)
            {
                PlayerOver = null;
            }
            else if (flag)
            {
                PlayerOver = this;
            }

            if (flag && cooldown <= 0f && entity != null && (int)entity.StateMachine == 0 && Input.Talk.Pressed && Enabled && !base.Scene.Paused)
            {
                cooldown = 0.1f;
                if (OnTalk != null)
                {
                    OnTalk(entity);
                }
            }

            if (flag && (int)entity.StateMachine == 0)
            {
                cooldown -= Engine.DeltaTime;
            }

            if (!Enabled)
            {
                disableDelay += Engine.DeltaTime;
            }
            else
            {
                disableDelay = 0f;
            }

            UI.Highlighted = flag && hoverTimer > 0.1f;
        }
        public class CustomTalkComponentUI : TalkComponentUI
        {
            public new CustomTalkComponent Handler;
            private bool fromHighlight;
            private float scale = 1;
            public int CurrentFrame;
            private float buffer;
            public bool started;

            public CustomTalkComponentUI(CustomTalkComponent handler) : base(handler)
            {
                Handler = handler;
            }
            public override void Update()
            {
                base.Update();
                if ((Highlighted || (!Highlighted && Handler.VisibleFromDistance)) && started && Handler.UsesSprite)
                {
                    buffer += Engine.DeltaTime;
                }
            }
            public override void Render()
            {

                Level level = Scene as Level;
                if (level.FrozenOrPaused || !(slide > 0f) || Handler.Entity == null)
                {
                    return;
                }
                float alpha = this.alpha * Handler.Alpha;
                Vector2 vector = level.Camera.Position.Floor();
                Vector2 vector2 = Handler.Entity.Position + Handler.DrawAt - vector + Handler.SpriteOffset;
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                {
                    vector2.X = 320f - vector2.X;
                }

                vector2.X *= 6f;
                vector2.Y *= 6f;

                if (Handler.CanHover)
                {
                    vector2.Y += (float)Math.Sin(timer * 4f) * 12f + 64f * (1f - Ease.CubeOut(slide));
                }
                float num2 = Ease.CubeInOut(slide) * alpha;
                Color color = lineColor * num2;
                MTexture CurrentTexture = Handler.UsesSprite ? Handler.sprite.GetFrame(Handler.sprite.CurrentAnimationID, CurrentFrame) : Handler.Texture;
                if (Highlighted != fromHighlight)
                {
                    if (Handler.UsesSprite)
                    {
                        started = true;
                        CurrentFrame = 0;
                    }
                }
                if (Highlighted)
                {
                    fromHighlight = true;
                    CurrentTexture.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, (1f - wiggler.Value * 0.5f));
                }
                else if (Handler.VisibleFromDistance)
                {
                    CurrentTexture.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, (1f + wiggler.Value * 0.5f));
                    fromHighlight = false;
                }
                else
                {
                    if (fromHighlight)
                    {
                        scale -= 0.1f;
                        CurrentTexture.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, scale);
                        if (scale <= 0)
                        {
                            scale = 1;
                            fromHighlight = false;
                        }
                    }
                }

                if (buffer >= Handler.Delay && Handler.UsesSprite)
                {
                    if (Handler.sprite.CurrentAnimationTotalFrames - 1 > 0)
                    {
                        if (Handler.Loop)
                        {
                            CurrentFrame = (CurrentFrame + 1) % (Handler.sprite.CurrentAnimationTotalFrames - 1);
                        }
                        else
                        {
                            CurrentFrame = Calc.Clamp(CurrentFrame + 1, 0, Handler.sprite.CurrentAnimationTotalFrames - 1);
                        }
                    }

                    buffer = 0;
                }

                if (Highlighted)
                {
                    /*                    if (Input.GuiInputController(Input.PrefixMode.Latest))
                                        {
                                            Input.GuiButton(Input.GoUp,Input.PrefixMode.Latest).DrawJustified(position, new Vector2(0.5f), From.White * num2, num);
                                        }
                                        else
                                        {
                                            ActiveFont.DrawOutline(Input.FirstKey(Input.GoUp).ToString().ToUpper(), position, new Vector2(0.5f), new Vector2(num), From.White * num2, 2f, From.Black);
                                        }*/
                }
            }
        }
    }


}