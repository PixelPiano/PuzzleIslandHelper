using Celeste.Mod.CommunalHelper;
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
        private readonly ParticleType PipeShard = new ParticleType()
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/pipeShard"],
            Size = 1,
            SpeedMin = 40,
            SpeedMax = 100,
            LifeMin = 0.8f,
            LifeMax = 1,
            SpinMin = 30,
            SpinMax = 90,
            RotationMode = ParticleType.RotationModes.Random,
            Color = Color.Orange,
            Color2 = Color.DarkOrange,
            ColorMode = ParticleType.ColorModes.Choose,
            ScaleOut = true,
            DirectionRange = 15f.ToRad()

        };
        public bool Broken;
        public bool ForceEnable;
        public bool InBreakRoutine;

        public SoundSource sfx;
        public bool UseSaveData;
        public VisibleTypes HideMethod;
        private bool UseDissolveTexture;
        private float StartDelay;
        private const float MinScale = 0.6f;
        public static MTexture StreamSpritesheet;
        public static MTexture[] DissolveTextures = new MTexture[4];
        private int dtIndex;
        private static Vector2 originThingy = new Vector2(0, 3);
        private int tvOffset;
        private Sprite Splash;
        private Image Hole;
        public Rectangle ClipRect;
        public bool WasOn;
        public EntityID ID;
        private bool Vertical
        {
            get
            {
                return Direction == Directions.Up || Direction == Directions.Down;
            }
        }
        private float Angle => (float)Math.PI / -2f * (float)Direction;

        private float WaitTime;
        public bool IsTimed;
        public bool IsOn;
        public bool inverted;
        private bool CanCollide;
        public bool RenderTextures = true;

        public bool Enabled
        {
            get
            {
                if (InBreakRoutine)
                {
                    return true;
                }
                if (UseSaveData && (!PianoModule.SaveData.HasBrokenPipes || PianoModule.SaveData.HasFixedPipes))
                {
                    if (PianoModule.Session.CutsceneSpouts.Contains(this))
                    {
                        return FlagState;
                    }
                    return false;
                }
                if (PianoModule.SaveData.HasBrokenPipes)
                {
                    return FlagState;
                }
                return false;
            }
        }
        public bool FlagState
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
        public bool WaitForRoutine;
        private bool Started;
        public string flag;
        private Vector2 Scale = Vector2.One;
        private Vector2 hideOffset = Vector2.Zero;
        private float MoveTime;
        private bool Moving;
        public bool OnlyMoveOnFlag;

        private enum Directions
        {
            Right,
            Up,
            Left,
            Down
        }
        private Directions Direction;

        public enum VisibleTypes
        {
            Dissolve,
            Grow,
            Instant
        }
        private Vector2 offset = Vector2.Zero;
        public Collider SplashBox;
        public Collider HurtBox;
        public Collider orig_Collider;
        public VirtualRenderTarget Target;
        public PipeSpout(EntityData data, Vector2 offset, EntityID iD)
        : base(data.Position + offset)
        {
            sfx = new SoundSource(Vector2.Zero, "event:/PianoBoy/env/local/pipes/water-stream-1");
            Add(sfx);
            Tag = Tags.TransitionUpdate;
            UseSaveData = data.Bool("useSaveData");
            MoveTime = data.Float("moveDuration");
            HideMethod = data.Enum<VisibleTypes>("hideMethod");
            Direction = data.Enum<Directions>("direction");
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            IsTimed = data.Bool("isTimed");
            WaitTime = data.Float("waitTime");
            StartDelay = data.Float("startDelay");
            OnlyMoveOnFlag = data.Bool("onlyMoveOnFlag");
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
            ClipRect = SplashBox.Bounds;
            Add(new BeforeRenderHook(BeforeRender));
            ID = iD;
        }
        public void EmitShards()
        {
            Vector2 direction = Direction switch
            {
                Directions.Left => Vector2.UnitX,
                Directions.Right => -Vector2.UnitX,
                Directions.Up => -Vector2.UnitY,
                Directions.Down => Vector2.UnitY,
                _ => Vector2.Zero
            };
            ParticleSystem system = SceneAs<Level>().ParticlesFG;
            system.Emit(PipeShard, 4, Hole.RenderPosition, Vector2.Zero, direction.Angle());

        }
        private IEnumerator Routine()
        {
            AdjustHideOffset(1); //set collider to fully in

            yield return WaitForEnabled(); //wait until state is true
            RenderTextures = false;
            yield return StartDelay;
            while (true)
            {
                yield return null;
                if (!Enabled)
                {
                    continue;
                }
                #region Grow
                Moving = true;
                RenderTextures = true;
                CanCollide = true;
                //Start
                AdjustHideOffset(1);
                switch (HideMethod)
                {
                    case VisibleTypes.Grow:
                        IsOn = true;
                        break;
                    case VisibleTypes.Dissolve:
                        if (IsTimed)
                        {
                            Splash.Play("undissolve");
                            UseDissolveTexture = true;
                        }
                        break;
                }
                //Update
                switch (HideMethod)
                {
                    case VisibleTypes.Grow:
                        for (float i = 0; i < 1; i += Engine.DeltaTime / MoveTime)
                        {
                            AdjustHideOffset(1 - i);
                            yield return null;
                            CanCollide = true;
                            Scale.Y = Calc.LerpClamp(MinScale, 1, i);
                        }
                        break;
                    case VisibleTypes.Dissolve:

                        for (float i = 0; i < 1; i += Engine.DeltaTime)
                        {
                            if (i > 0.5f)
                            {
                                IsOn = true;
                                AdjustHideOffset(0);
                            }
                            PickDissolveTexture(1 - i, 0.5f);
                            yield return null;
                            CanCollide = true;
                        }
                        break;
                }
                //End
                switch (HideMethod)
                {
                    case VisibleTypes.Grow:
                        AdjustHideOffset(0);
                        Scale.Y = 1;
                        break;
                    case VisibleTypes.Dissolve:
                        UseDissolveTexture = false;
                        break;
                }
                if (!Started)
                {
                    Started = true;
                    IsOn = true;
                }
                yield return null;
                Moving = false;
                #endregion
                if (!Enabled)
                {
                    continue;
                }

                yield return WaitTime;

                if (!Enabled)
                {
                    continue;
                }
                #region Shrink
                Moving = true;
                AdjustHideOffset(0);
                switch (HideMethod)
                {
                    case VisibleTypes.Grow:
                        break;
                    case VisibleTypes.Dissolve:
                        IsOn = false;
                        if (IsTimed)
                        {
                            UseDissolveTexture = true;
                            Splash.Play("dissolve");
                        }
                        break;
                }
                switch (HideMethod)
                {
                    case VisibleTypes.Grow:
                        for (float i = 0; i < 1; i += Engine.DeltaTime / MoveTime)
                        {
                            Scale.Y = Calc.LerpClamp(1, MinScale, i);
                            AdjustHideOffset(i);
                            yield return null;
                        }
                        break;
                    case VisibleTypes.Dissolve:
                        for (float i = 0; i < 1; i += Engine.DeltaTime)
                        {
                            AdjustHideOffset(1);
                            PickDissolveTexture(i, 0.6f);
                            yield return null;
                        }
                        break;
                }
                if (HideMethod == VisibleTypes.Grow)
                {
                    AdjustHideOffset(1);
                    Scale.Y = MinScale;
                    IsOn = false;
                }
                Moving = false;
                #endregion
                if (!Enabled)
                {
                    continue;
                }

                yield return WaitTime;

                if (!Enabled)
                {
                    continue;
                }
            }
        }
        public IEnumerator BreakRoutine()
        {
            //ForceEnable = true;
            if (Scene is not Level level)
            {
                yield break;
            }
            PianoModule.Session.CutsceneSpouts.Add(this);
            Visible = false;
            while (!SceneAs<Level>().InsideCamera(Hole.RenderPosition))
            {
                yield return null;
            }
            //play creak sound
            yield return 0.8f;
            Audio.Play("event:/PianoBoy/env/local/pipes/pipeburst");
            //burst open
            #region Grow
            AdjustHideOffset(1);
            Visible = true;
            RenderTextures = true;
            InBreakRoutine = true;
            Broken = true;
            EmitShards();
            level.DirectionalShake(-Vector2.UnitY, 0.1f);
            sfx.Play(sfx.EventName);
            float duration = 0.5f;
            for (float i = 0; i < duration; i += Engine.DeltaTime)
            {
                float lerp = i / duration;
                AdjustHideOffset(1 - Ease.SineIn(lerp));
                yield return null;
                Scale.Y = Calc.LerpClamp(MinScale, 1, Ease.SineIn(lerp));
            }
            AdjustHideOffset(0);
            Scale.Y = 1;
            Started = true;
            IsOn = true;
            PianoModule.SaveData.HasBrokenPipes = true;
            yield return null;
            CanCollide = true;
            Moving = false;
            #endregion
            yield return null;
        }
        public void GrowBreak()
        {
            Add(new Coroutine(BreakRoutine()));
        }
        private IEnumerator WaitForEnabled()
        {
            while (!Enabled)
            {
                yield return null;
            }
        }
        private IEnumerator OnlyOnFlagRoutine()
        {
            float lerp = Enabled ? 0 : 1;
            while (true)
            {
                RenderTextures = lerp < 1;
                lerp = Calc.Approach(lerp, Enabled ? 0 : 1, Engine.DeltaTime / MoveTime);
                AdjustHideOffset(lerp);
                yield return null;
                CanCollide = true;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Started = Enabled;
            if (StartDelay == 0)
            {
                IsOn = true;
            }
            if (IsTimed)
            {
                if (OnlyMoveOnFlag)
                {
                    Add(new Coroutine(OnlyOnFlagRoutine()));
                }
                else
                {
                    Add(new Coroutine(Routine()));
                }

            }
            else
            {
                CanCollide = true;
            }
        }
        public void DrawTextures(Vector2 pos, Color color)
        {
            Vector2 retreatOffset = (HideMethod == VisibleTypes.Grow || InBreakRoutine ? hideOffset : Vector2.Zero);
            Vector2 DrawPosition = pos + offset + retreatOffset;

            int Length = Vertical ? (int)orig_Collider.Height : (int)orig_Collider.Width;
            float rotation = Angle;
            float width = StreamSpritesheet.Width;
            Vector2 origin = originThingy * Scale;
            int i = 0;
            Texture2D texture = StreamSpritesheet.Texture.Texture_Safe;
            if (UseDissolveTexture && HideMethod == VisibleTypes.Dissolve)
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
            if (FlagState)
            {
                WasOn = true;
                if (!PianoModule.SaveData.SpoutWreckage.Contains(ID))
                {
                    PianoModule.SaveData.SpoutWreckage.Add(ID);
                }
            }
            /*            if (!Enabled)
                        {
                            if (sfx.InstancePlaying)
                            {
                                sfx.Stop(true);
                            }
                        }
                        else
                        {
                            if (!sfx.InstancePlaying)
                            {
                                sfx.Play(sfx.EventName);
                            }
                        }*/
            Collidable = (!IsTimed || IsOn) && CanCollide && Enabled;
            if (Scene.OnInterval(1 / 12f))
            {
                tvOffset = ++tvOffset % 3;
            }
            RoutineUpdate();


        }
        private void RoutineUpdate()
        {
            if (HideMethod == VisibleTypes.Grow || ForceEnable)
            {
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


        public void AdjustHideOffset(float lerp)
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
            if (Scene is not Level level)
            {
                return;
            }
            if (!level.Camera.GetBounds().Intersects(SplashBox.Bounds))
            {
                return;
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin();
            DrawTextures(Position - SplashBox.Position, Color.White);
            Draw.SpriteBatch.End();

        }
        public override void Render()
        {
            base.Render();

            if (Scene is not Level level)
            {
                return;
            }
            if (level.Camera.GetBounds().Intersects(SplashBox.Bounds))
            {
                if (RenderTextures && (OnlyMoveOnFlag || Enabled))
                {

                    Draw.SpriteBatch.Draw(Target, SplashBox.Position, Color.White);
                }
            }
            Hole.Rotation = Angle;
            if (((PianoModule.SaveData.HasBrokenPipes || Broken) && (WasOn || PianoModule.SaveData.SpoutWreckage.Contains(ID))) || PianoModule.SaveData.GetPipeState() > 3)
            {
                Hole.Render();
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(HurtBox, Color.Green);
        }
    }
}