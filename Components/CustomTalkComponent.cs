using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [TrackedAs(typeof(TalkComponent))]
    public class CustomTalkComponent : TalkComponent
    {

        private bool CanHover;
        private bool VisibleFromDistance;
        private Sprite sprite;
        private string Anim;
        public bool UsesSprite;
        public MTexture Texture;
        public float Delay;
        public bool Muted;
        public bool Loop;
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
                    Anim = "idle";
                    UsesSprite = true;
                    Delay = 0.5f;
                    Muted = false;
                    Loop = true;
                    VisibleFromDistance = false;
                    CanHover = false;
                    break;
                case SpecialType.DigitalLog:
                    Texture = GFX.Gui["PuzzleIslandHelper/hover/digitalB"];
                    UsesSprite = false;
                    Muted = false;
                    VisibleFromDistance = false;
                    CanHover = true;
                    break;
                case SpecialType.UpArrow:
                    Texture = GFX.Gui["PuzzleIslandHelper/hover/upArrow"];
                    VisibleFromDistance = false;
                    CanHover = false;
                    break;
                case SpecialType.DownArrow:
                    Texture = GFX.Gui["PuzzleIslandHelper/hover/downArrow"];
                    VisibleFromDistance = false;
                    CanHover = false;
                    break;
            }
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
                Anim = animID;
                Delay = delay;

                UsesSprite = true;
            }
            else
            {
                Texture = GFX.Gui["hover/idle"];
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
                Texture = highlightTexture;
                UsesSprite = false;
            }
            else
            {
                Texture = GFX.Gui["hover/idle"];
            }
        }
        public override void Update()
        {
            if (UI == null)
            {
                Entity.Scene.Add(UI = new CustomTalkComponentUI(this));
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            bool flag = disableDelay < 0.05f && entity != null && entity.CollideRect(new Rectangle((int)(base.Entity.X + (float)Bounds.X), (int)(base.Entity.Y + (float)Bounds.Y), Bounds.Width, Bounds.Height)) && entity.OnGround() && entity.Bottom < base.Entity.Y + (float)Bounds.Bottom + 4f && entity.StateMachine.State == 0 && (!PlayerMustBeFacing || Math.Abs(entity.X - base.Entity.X) <= 16f || entity.Facing == (Facings)Math.Sign(base.Entity.X - entity.X)) && (PlayerOver == null || PlayerOver == this);
            if (flag)
            {
                hoverTimer += Engine.DeltaTime;
            }
            else if (UI.Display)
            {
                hoverTimer = 0f;
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
            private int CurrentFrame;
            private float buffer;
            private bool started;

            public CustomTalkComponentUI(CustomTalkComponent handler) : base(handler)
            {
                Handler = handler;
            }
            public override void Update()
            {
                base.Update();
                if (fromHighlight && started && Handler.UsesSprite)
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

                Vector2 vector = level.Camera.Position.Floor();
                Vector2 vector2 = Handler.Entity.Position + Handler.DrawAt - vector;
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
                float num = ((!Highlighted) ? (1f + wiggler.Value * 0.5f) : (1f - wiggler.Value * 0.5f));
                float num2 = Ease.CubeInOut(slide) * alpha;
                Color color = lineColor * num2;


                if (Highlighted)
                {
                    if (!fromHighlight)
                    {
                        if (Handler.UsesSprite)
                        {
                            Handler.sprite.Play(Handler.Anim);
                            started = true;
                            CurrentFrame = 0;
                        }
                    }
                    fromHighlight = true;
                    MTexture CurrentTexture = Handler.UsesSprite ? Handler.sprite.GetFrame(Handler.sprite.CurrentAnimationID, CurrentFrame) : Handler.Texture;


                    CurrentTexture.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, (1f - wiggler.Value * 0.5f));
                    if (buffer >= Handler.Delay && Handler.UsesSprite)
                    {
                        if (Handler.Loop)
                        {
                            int newFrame = (CurrentFrame + 1) % (Handler.sprite.CurrentAnimationTotalFrames - 1);
                            CurrentFrame = newFrame;
                        }
                        else
                        {
                            CurrentFrame = Calc.Clamp(CurrentFrame + 1, 0, Handler.sprite.CurrentAnimationTotalFrames - 1);
                        }

                        buffer = 0;
                    }
                }
                else if (Handler.VisibleFromDistance)
                {
                    GFX.Gui[Handler.HoverUI.Texture.AtlasPath].DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, (1f + wiggler.Value * 0.5f));
                    fromHighlight = false;
                }
                else
                {
                    if (fromHighlight)
                    {
                        scale -= 0.1f;
                        MTexture CurrentTexture = Handler.UsesSprite ? Handler.sprite.GetFrame(Handler.sprite.CurrentAnimationID, Handler.sprite.CurrentAnimationTotalFrames - 1) : Handler.Texture;

                        CurrentTexture.DrawJustified(vector2, new Vector2(0.5f, 1f), color * alpha, scale);
                        if (scale <= 0)
                        {
                            scale = 1;
                            fromHighlight = false;
                        }
                    }
                }
                if (Highlighted)
                {
                    /*                    if (Input.GuiInputController(Input.PrefixMode.Latest))
                                        {
                                            Input.GuiButton(Input.GoUp,Input.PrefixMode.Latest).DrawJustified(position, new Vector2(0.5f), Color.White * num2, num);
                                        }
                                        else
                                        {
                                            ActiveFont.DrawOutline(Input.FirstKey(Input.GoUp).ToString().ToUpper(), position, new Vector2(0.5f), new Vector2(num), Color.White * num2, 2f, Color.Black);
                                        }*/
                }
            }
        }
    }


}