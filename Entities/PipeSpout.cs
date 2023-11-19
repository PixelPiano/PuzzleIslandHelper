using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/PipeSpout")]
    [Tracked]
    public class PipeSpout : Entity
    {
        public enum HideMethods
        {
            Retreat,
            Dissolve
        }
        public HideMethods HideMethod;

        public enum States
        {
            Idle,
            Dorment,
            Shrinking,
            Growing,
            Waiting
        }
        public StateMachine StateMachine { get; set; }
        public States State;
        public const int StIdle = 0;
        public const int StDorment = 1;
        public const int StShrinking = 2;
        public const int StGrowing = 3;
        public const int StWaiting = 4;

        private float lerp;


        private bool StartState;
        private float StartDelay;
        private float StartTimer;
        private const float MinScale = 0.6f;
        public static MTexture StreamSpritesheet;
        public static MTexture[] DissolveTextures = new MTexture[4];
        private int dtIndex;
        private static Vector2 originThingy = new Vector2(0, 3);
        private int tvOffset;

        private Sprite Splash;
        private Image Hole;
        private bool Vertical
        {
            get
            {
                return Direction == Directions.Up || Direction == Directions.Down;
            }
        }
        private float Angle => (float)Math.PI / -2f * (float)Direction;

        private float WaitTime;
        private float WaitTimer;
        public bool IsTimed;
        public bool IsOn;

        private bool inverted;
        private bool InRoutine;
        public bool Enabled
        {
            get
            {
                Level level = Scene as Level;
                if (level is null)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                if (inverted)
                {
                    return !level.Session.GetFlag(flag);
                }
                else
                {
                    return level.Session.GetFlag(flag);
                }
            }
        }
        private bool Started;
        private string flag;
        private float Timer;
        private Vector2 Scale = Vector2.One;
        private Vector2 hideOffset = Vector2.Zero;
        private float MoveTime;
        private bool Moving;
        private enum Directions
        {
            Right,
            Up,
            Left,
            Down
        }
        private Directions Direction;
        private Vector2 offset = Vector2.Zero;
        public Collider SplashBox;
        public Collider HurtBox;
        public Collider orig_Collider;
        public VirtualRenderTarget Target;
        public PipeSpout(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag = Tags.TransitionUpdate;
            MoveTime = data.Float("moveDuration");
            HideMethod = data.Enum<HideMethods>("hideMethod");
            Direction = data.Enum<Directions>("direction");
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            IsTimed = data.Bool("isTimed");
            WaitTime = data.Float("waitTime");
            StartDelay = data.Float("startDelay");

            if (StartDelay > 0)
            {
                State = States.Dorment;
            }
            else
            {
                State = States.Idle;
            }
            Timer = 0;
            #region Sprites
            Splash = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/waterPipes/")
            {
                Visible = false
            };
            Splash.AddLoop("idle", "splashtexture", 0.09f);
            Splash.Add("dissolve", "splashDissolve", 0.09f);
            Splash.Add("undissolve", "splashDissolveRev", 0.09f, "idle");
            Add(Splash);
            Splash.Play("idle");
            Splash.CenterOrigin();
            Splash.Position += new Vector2(Splash.Width / 2, Splash.Height / 2);

            Hole = new Image(GFX.Game["objects/PuzzleIslandHelper/waterPipes/opening2"])
            {
                Visible = false,
            };
            Hole.CenterOrigin();
            Hole.Position += new Vector2(Hole.Width / 2, Hole.Height / 2);
            Add(Hole);
            Collider = new Hitbox(data.Width, data.Height);
            orig_Collider = new Hitbox(data.Width, data.Height);
            HurtBox = new Hitbox(data.Width, data.Height);
            Add(new PlayerCollider(OnPlayer));
            SplashBox = Direction switch
            {
                Directions.Right => SplashBox = new Hitbox(data.Width + 8, Splash.Height, 0, -(Splash.Height - data.Height) / 2),
                Directions.Left => SplashBox = new Hitbox(data.Width + 8, Splash.Height, -8, -(Splash.Height - data.Height) / 2),
                Directions.Up => new Hitbox(Splash.Height, data.Height + 8, -(Splash.Height - data.Width) / 2, -8),
                Directions.Down => new Hitbox(Splash.Height, data.Height + 8, -(Splash.Height - data.Width) / 2, 0),
                _ => null
            };
            SplashBox.Position += Position;
            this.offset = Direction switch
            {
                Directions.Left => new(Width, Height / 2),
                Directions.Right => new(0, Height / 2),
                Directions.Down => new(Width / 2, 0),
                Directions.Up => new(Width / 2, Height),
                _ => Vector2.Zero
            };
            Hole.RenderPosition = Direction switch
            {
                Directions.Right => Collider.CenterLeft,
                Directions.Up => Collider.BottomCenter,
                Directions.Left => Collider.CenterRight,
                Directions.Down => Collider.TopCenter - Vector2.UnitX,
                _ => Vector2.Zero
            };
            Hole.RenderPosition += Position;
            #endregion
            Collider = HurtBox;
            Target = VirtualContent.CreateRenderTarget("Target", (int)SplashBox.Width, (int)SplashBox.Height);
            Add(StateMachine = new StateMachine(5));
            StateMachine.SetCallbacks(0, IdleUpdate, null, IdleBegin);
            StateMachine.SetCallbacks(1, DormentUpdate);
            StateMachine.SetCallbacks(2, ShrinkingUpdate, null, ShrinkingBegin, ShrinkingEnd);
            StateMachine.SetCallbacks(3, GrowingUpdate, null, GrowingBegin, GrowingEnd);
            StateMachine.SetCallbacks(4, WaitingUpdate, null, WaitingBegin);
            StateMachine.State = (int)State;
            Add(new BeforeRenderHook(BeforeRender));
        }

        public int IdleUpdate() //Fully Out
        {
            if (!Enabled)
            {
                return StDorment;
            }
            if (IsTimed)
            {
                return StWaiting;
            }

            return StIdle;
        }
        public void IdleBegin()
        {
            AdjustHideOffset(0);
            IsOn = true;
        }
        public int GrowingUpdate() //Extending from fully in
        {
            if (!Enabled)
            {
                return StDorment;
            }
            switch (HideMethod)
            {
                case HideMethods.Retreat:
                    if (lerp > 0)
                    {
                        //lerp = Calc.Approach(lerp, 0, Engine.DeltaTime / MoveTime);
                        AdjustHideOffset(1 - lerp);
                        lerp -= Engine.DeltaTime / MoveTime;
                    }
                    break;
                case HideMethods.Dissolve:

                    if (lerp > 0)
                    {
                        if (lerp > 0.5f)
                        {
                            IsOn = true;
                            AdjustHideOffset(0);
                        }
                        PickDissolveTexture(1 - lerp, 0.5f);
                        lerp -=Engine.DeltaTime;
                    }
                    break;
            }
            if (lerp <= 0)
            {
                return StGrowing;
            }
            return StIdle;
        }
        private void GrowingEnd()
        {
            switch (HideMethod)
            {
                case HideMethods.Retreat:
                    AdjustHideOffset(0);
                    Scale.Y = 1;
                    break;
                case HideMethods.Dissolve:
                    break;
            }
            if (!Started)
            {
                Started = true;
                IsOn = true;
            }
        }

        public void GrowingBegin()
        {
            lerp = 1;
            AdjustHideOffset(1); //Set collider to fully in
            switch (HideMethod)
            {
                case HideMethods.Retreat:
                    IsOn = true;
                    break;
                case HideMethods.Dissolve:
                    if (IsTimed)
                    {
                        Splash.Play("undissolve");
                    }
                    break;
            }
        }
        public int ShrinkingUpdate() //Compressing from fully out
        {
            if (!Enabled)
            {
                return StDorment;
            }
            if (lerp < 1)
            {
                switch (HideMethod)
                {
                    case HideMethods.Retreat:
                        AdjustHideOffset(lerp);
                        //lerp = Calc.Approach(lerp, 1, Engine.DeltaTime / MoveTime);
                        Scale.Y = Calc.LerpClamp(1, MinScale, lerp);
                        lerp += Engine.DeltaTime/MoveTime;
                        break;
                    case HideMethods.Dissolve:
                        AdjustHideOffset(1);
                        //lerp = Calc.Approach(lerp, 1, Engine.DeltaTime);
                        PickDissolveTexture(lerp, 0.6f);
                        lerp+=Engine.DeltaTime;
                        break;
                }
            }
            if (lerp >= 1)
            {
                if (HideMethod == HideMethods.Retreat)
                {
                    AdjustHideOffset(1);
                    lerp = 1;
                    Scale.Y = MinScale;
                    IsOn = false;
                }
                return StWaiting;
            }
            return StShrinking;
        }
        public void ShrinkingBegin()
        {
            lerp = 0;
            AdjustHideOffset(0);
            switch (HideMethod)
            {
                case HideMethods.Retreat:

                    break;
                case HideMethods.Dissolve:
                    IsOn = false;
                    if (IsTimed)
                    {
                        Splash.Play("dissolve");
                    }
                    break;
            }


        }

        public void ShrinkingEnd()
        {
            switch (HideMethod)
            {
                case HideMethods.Retreat:
                    AdjustHideOffset(1);
                    lerp = 1;
                    Scale.Y = MinScale;
                    IsOn = false;
                    break;
                case HideMethods.Dissolve:

                    break;
            }
        }
        public int DormentUpdate()
        {
            if (!Enabled)
            {
                return StDorment;
            }
            if (StartDelay > 0)
            {
                return StWaiting;
            }
            return 0;
        } //Waiting for flag
        public int WaitingUpdate() //Waiting for start
        {
            if (!Enabled)
            {
                return StDorment;
            }
            if (StartTimer < StartDelay && StartDelay > 0) //If should wait StartDelay seconds before growing
            {
                StartTimer += Engine.DeltaTime;
                return StWaiting;
            }
            if (Started)
            {
                if (WaitTimer < WaitTime) //If on the cycle after entity has Started
                {
                    WaitTimer += Engine.DeltaTime;
                    return StWaiting;
                }
            }
            return StGrowing; //Start growing
        }

        public void WaitingBegin()
        {
            WaitTimer = 0;
            if (!Started)
            {
                switch (HideMethod)
                {
                    case HideMethods.Retreat:
                        Scale.Y = MinScale;
                        IsOn = false;
                        break;
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            StartState = Enabled;
            Started = Enabled;
            if (StartDelay == 0)
            {
                IsOn = true;
            }
            else if (StartState)
            {
                StateMachine.State = StWaiting;
            }

        }
        public void DrawTextures(Vector2 pos, Color color)
        {
            Vector2 retreatOffset = (HideMethod == HideMethods.Retreat ? hideOffset : Vector2.Zero);
            Vector2 DrawPosition = pos + offset + retreatOffset;

            int Length = Vertical ? (int)orig_Collider.Height : (int)orig_Collider.Width;
            float rotation = Angle;
            float width = StreamSpritesheet.Width;
            Vector2 origin = originThingy * Scale;
            int i = 0;
            Texture2D texture = StreamSpritesheet.Texture.Texture_Safe;
            if (InRoutine && HideMethod == HideMethods.Dissolve)
            {
                texture = DissolveTextures[dtIndex].Texture.Texture_Safe;
            }
            Splash.RenderPosition = Direction switch
            {
                Directions.Up => new Vector2(orig_Collider.Width / 2, 8),
                Directions.Left => new Vector2(8, orig_Collider.Height / 2),
                Directions.Down => new Vector2(orig_Collider.Width / 2, orig_Collider.Height - 8),
                Directions.Right => new Vector2(orig_Collider.Width - 8, orig_Collider.Height / 2),
                _ => Vector2.Zero
            };
            Splash.Scale = Scale;
            Splash.RenderPosition += pos + retreatOffset;
            Splash.Rotation = rotation;
            for (; i < Length - width; i += (int)width)
            {
                Draw.SpriteBatch.Draw
                        (
                            color: color, rotation: rotation, origin: origin, scale: Scale, effects: 0, layerDepth: 0f,
                            texture: texture, position: DrawPosition + new Vector2(i, 0).Rotate(rotation),
                            sourceRectangle: new Rectangle(0, 6 * tvOffset, (int)width, 6)
                        );
            }
            Draw.SpriteBatch.Draw
                    (
                        color: color, rotation: rotation, origin: origin, scale: Scale, effects: 0, layerDepth: 0f,
                        texture: texture,
                        position: DrawPosition + new Vector2(i, 0).Rotate(rotation),
                        sourceRectangle: new Rectangle(0, 6 * tvOffset, Length - i, 6)
                    );
            Splash.Render();
        }

        private void OnPlayer(Player player)
        {
            player.Die(Vector2.Zero);
        }

        public override void Update()
        {
            base.Update();
            Collidable = (!IsTimed || IsOn) && Enabled;
            if (!Enabled)
            {
                return;
            }
            if (StateMachine.State == StIdle)
            {
                if (Timer < WaitTime)
                {
                    Timer += Engine.DeltaTime;
                }
                else
                {
                    Timer = 0;
                    StateMachine.State = StGrowing;
                }
            }
            if (Scene.OnInterval(1 / 12f))
            {
                tvOffset = ++tvOffset % 3;
            }
            if (Moving)
            {
                RoutineUpdate();
            }

        }
        private void RoutineUpdate()
        {
            switch (HideMethod)
            {
                case HideMethods.Retreat:
                    HurtBox.Width = (orig_Collider.Width - Math.Abs(hideOffset.X));
                    HurtBox.Height = (orig_Collider.Height - Math.Abs(hideOffset.Y));
                    if (Direction == Directions.Up)
                    {
                        HurtBox.Position.Y = Math.Abs(hideOffset.Y);
                    }
                    if (Direction == Directions.Left)
                    {
                        HurtBox.Position.X = Math.Abs(hideOffset.X);
                    }
                    break;
                case HideMethods.Dissolve:

                    break;
            }
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Target?.Dispose();
            Target = null;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
        private void PickDissolveTexture(float lerp, float time)
        {
            if (lerp < (time / 3) * 3)
            {
                if (lerp < (time / 3) * 2)
                {
                    if (lerp < time / 3)
                    {
                        dtIndex = 0;
                    }
                    else
                    {
                        dtIndex = 1;
                    }
                }
                else
                {
                    dtIndex = 2;
                }
            }
            else
            {
                dtIndex = 3;
            }
        }


        private void AdjustHideOffset(float lerp)
        {
            switch (Direction)
            {
                case Directions.Right:
                    hideOffset.X = Calc.LerpClamp(0, -orig_Collider.Width, lerp);
                    break;
                case Directions.Up:
                    hideOffset.Y = Calc.LerpClamp(0, orig_Collider.Height, lerp);
                    break;
                case Directions.Left:
                    hideOffset.X = Calc.LerpClamp(0, orig_Collider.Width, lerp);
                    break;
                case Directions.Down:
                    hideOffset.Y = Calc.LerpClamp(0, -orig_Collider.Height, lerp);
                    break;
            }
        }

        private void BeforeRender()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin();
            DrawTextures(Position - SplashBox.Position, Color.White);
            Draw.SpriteBatch.End();

        }
        public override void Render()
        {
            base.Render();
            if (Enabled)
            {
                Draw.SpriteBatch.Draw(Target, SplashBox.Position, Color.White);
            }
            Hole.Rotation = Angle;
            Hole.Render();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(SplashBox, Color.Red);
            Draw.HollowRect(HurtBox, Color.Green);
        }
    }
}