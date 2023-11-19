using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using Celeste.Mod.Entities;
using System.Collections;
using VivHelper;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MovingJelly")]
    [TrackedAs(typeof(Glider))]
    internal class MovingJelly : Glider
    {
        #region Variables


        private Sprite arrowSprite;
        private Entity arrow;
        private string sFlag;
        private float Rate;
        private bool StartActive;
        private DynamicData gliderData;
        private ParticleSystem particlesBG;
        private Direction directionA;
        private Direction directionB;
        private bool AddedComponant;
        private bool DisableAudio;
        private bool ToggleDirection;
        private bool flag;
        private bool NoGravity;
        private bool isHeld = false;
        private bool wasThrown = false;
        private bool inRoutine = false;
        private bool isOpen = false;
        private Color color;
        private bool start = true;
        private bool hitWall = false;
        private bool hitFloor = false;
        private bool movePlayer;
        private bool release;
        private float from;
        private bool IsVertical;
        private bool IsHorizontal;
        private bool drawLine = false;
        private float lineOpacity = 0.0f;
        private bool playerAdjust;
        private float adjustSpeed = 8f;
        public enum Direction
        {
            None, Up, Down, Left, Right
        }

        #endregion

        #region Constructors

        public MovingJelly(EntityData data, Vector2 offset)
          : this(data.Position + offset, data.Bool("bubble"), data.Bool("tutorial"))
        {
            directionA = data.Enum<Direction>("directionB", Direction.None);
            directionB = data.Enum<Direction>("directionA", Direction.None);
            ToggleDirection = data.Bool("toggleDirection");
            NoGravity = data.Bool("disableGravity");
            StartActive = data.Bool("startActive");
            movePlayer = data.Bool("dragPlayerAlong");
            playerAdjust = data.Bool("playerCanInfluence");
            sFlag = data.Attr("flag");
            Rate = data.Float("rate");
            color = data.HexColor("color");
            DisableAudio = data.Bool("disableAudio");

            ArrowDust.Color = color;
            ArrowDust.Color2 = Color.Lerp(color, Color.AliceBlue, 0.1f);
            ArrowLoad.Color = Color.Lerp(ArrowDust.Color.Invert(), Color.White, 0.5f);
            ArrowLoad.Color2 = Color.Lerp(ArrowDust.Color2.Invert(), Color.White, 0.5f);


            IsVertical = ((directionA == Direction.Up || directionA == Direction.Down) && flag)
                            || ((directionB == Direction.Up || directionB == Direction.Down) && !flag);

            IsHorizontal = ((directionA == Direction.Left || directionA == Direction.Right) && flag)
                            || ((directionB == Direction.Left || directionB == Direction.Right) && !flag);

            gliderData = DynamicData.For(this);
        }

        public MovingJelly(Vector2 position, bool bubble, bool tutorial)
      : base(position, bubble, tutorial)
        {
        }


        #endregion

        #region Helper Functions
        private void AdjustPosition(Direction a, Scene scene)
        {

            if (isHeld && !movePlayer)
            {
                return;
            }

            string id = "";
            string prevId = arrowSprite.CurrentAnimationID;

            int slightY = scene.OnInterval(8 / 60) ? 2 : 0;
            //float gravTime = (directionA == Direction.None && flag) || (directionB == Direction.None && !flag) ? 0 : Engine.DeltaTime;
            if (CurrentDirection() != Direction.None && NoGravity) gliderData.Set("noGravityTimer", Engine.DeltaTime);

            if (hitWall || hitFloor)
            {
                return;
            }
            switch (a)
            {
                case Direction.Up:
                    Speed.Y = -Rate;
                    id = "up";
                    break;

                case Direction.Down:
                    Speed.Y = Rate;
                    id = "down";
                    break;

                case Direction.Left:
                    Speed.X = -Rate;
                    Speed.Y = -slightY;
                    id = "left";
                    break;

                case Direction.Right:
                    Speed.X = Rate;
                    Speed.Y = -slightY;
                    id = "right";
                    break;

                case Direction.None:
                    id = "none";
                    break;
            }

            if (prevId != id && !start)
            {
                Coroutine coroutine = new Coroutine(colorChange(), true);
                arrow.Add(coroutine);
            }
            arrowSprite.Play(id);
        }
        private Direction CurrentDirection()
        {
            if (!ToggleDirection) return directionA;
            else return flag ? directionA : directionB;
        }

        #endregion

        #region Coroutines
        private IEnumerator colorChange()
        {
            for (float i = 1f; i > 0; i -= Engine.DeltaTime)
            {
                arrowSprite.Color = Color.Lerp(color, Color.White, i / 1f);
                yield return null;
            }
        }
        private IEnumerator ReleaseWaiter()
        {
            if (!release)
            {
                from = Position.Y;
                release = true;
            }
            inRoutine = true;
            float count = 0.0f;

            lineOpacity = 0f;
            while (Position.Y <= from)
            {
                drawLine = true;
                lineOpacity = Calc.Approach(lineOpacity, 1f, count);
                count += Engine.DeltaTime;
                if (isHeld)
                {
                    lineOpacity = 0f;
                    break;
                }
                yield return null;
            } //draw line

            Position.Y = from;
            wasThrown = false;
            isOpen = false;
            AppearParticles(false);

            for (float i = 0.0f; i < 1.0f; i += Engine.DeltaTime)
            {
                lineOpacity = Calc.Approach(lineOpacity, 0f, i);
                if (isHeld)
                {
                    lineOpacity = 0f;
                    break;
                }
                yield return null;
            } //draw line

            drawLine = false;
            inRoutine = false;
            yield return null;
        }
        private IEnumerator dragPlayer()
        {
            if (CurrentDirection() != Direction.None)
            {
                if (gliderData.Get<Sprite>("Texutre") is not null)
                {
                    gliderData.Get<Sprite>("Texture").Rotation = arrowSprite.Rotation;
                }
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            float increment = 0f;
            Vector2 temp = Position;
            Vector2 target = new Vector2(BottomCenter.X, BottomLeft.Y + Height);
            while (isHeld)
            {
                if (IsVertical)
                {
                    if (playerAdjust)
                    {
                        //player.Speed.X = (int)player.Facing * adjustSpeed;
                        player.Speed.X = Input.MoveX.Value * adjustSpeed;
                        Position.X = player.Position.X;
                    }
                    else
                    {
                        player.MoveToX(target.X);
                        Position.X = temp.X;
                    }
                    player.Speed.Y = Speed.Y;
                }
                else if (IsHorizontal)
                {
                    player.MoveToY(target.Y);
                    Position.Y = temp.Y;
                    player.Speed.X = Speed.X;
                }
                increment += Engine.DeltaTime * 2;
                yield return null;
            }
        }
        public IEnumerator WaitThenSet(bool didHitWall, bool value)
        {
            for (int i = 0; i < 10; i++)
            {
                if (NoGravity) gliderData.Set("noGravityTimer", Engine.DeltaTime);
                yield return null;
            }

            hitWall = didHitWall ? value : hitWall;
            hitFloor = didHitWall ? hitFloor : value;
            yield return null;
        }
        #endregion

        #region Override Methods
        public override void Added(Scene scene)
        {
            base.Added(scene);
            string[] anims = { "left", "right", "up", "down", "none", "load" };

            scene.Add(arrow = new Entity(Position - new Vector2(4f, Height * 3f)));
            arrow.Add(arrowSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/movingJelly/"));


            foreach (string id in anims)
            {
                arrowSprite.AddLoop(id, id, 0.1f);
            }
            arrowSprite.Color = color;
            if (!StartActive)
            {
                arrowSprite.Play("none");
            }
            scene.Add(particlesBG = new ParticleSystem(arrow.Depth + 1, 40));
        }
        public override void Update()
        {
            base.Update();
            if (movePlayer && IsVertical)
            {
                Speed.X = 0;
            }
            IsVertical = ((directionA == Direction.Up || directionA == Direction.Down) && flag)
                 || ((directionB == Direction.Up || directionB == Direction.Down) && !flag);

            IsHorizontal = ((directionA == Direction.Left || directionA == Direction.Right) && flag)
                            || ((directionB == Direction.Left || directionB == Direction.Right) && !flag);
            flag = SceneAs<Level>().Session.GetFlag(sFlag);
            isOpen = (string)gliderData.Get("Texture.CurrentAnimationID") != "fall" && (string)gliderData.Get("Texture.CurrentAnimationID") != "fallLoop" ? false : true;

            if (!inRoutine && wasThrown && !isHeld)
            {
                release = false;
                Coroutine coroutine = new Coroutine(ReleaseWaiter(), true);
                Add(coroutine);
            }

            if ((!ToggleDirection && !flag) || (ToggleDirection && ((flag && directionA == Direction.None) || (!flag && directionB == Direction.None))))
            {
                arrowSprite.Play("none");
            }

            if (!wasThrown)
            {
                Direction temp = flag ? directionA : ToggleDirection ? directionB : Direction.None;
                temp = ToggleDirection ? flag ? directionA : directionB : directionA;
                AdjustPosition(temp, Scene as Level);

                if ((isHeld && isOpen) || !isHeld)
                {
                    AppearParticles(true);
                }
                if (movePlayer)
                {
                    Coroutine coroutine = new Coroutine(dragPlayer(), true);
                    Add(coroutine);
                }
            }

            arrow.Position.Y = Position.Y - (isOpen ? Height * 5f + 20 : Height * 3f);
            arrow.Position.X = Position.X - 4;
            start = false;
        }
        public override void Render()
        {
            base.Render();
            if (drawLine)
            {
                if (from != 0)
                {
                    Draw.Line(Center.X - Width, from - 2, Center.X + Width * 1.3f, from - 2, color * lineOpacity);
                    Draw.Point(new Vector2(Center.X - Width - 2, from - 2), Color.Lerp(color, Color.Black, 0.5f) * lineOpacity);
                    Draw.Point(new Vector2(Center.X + Width * 1.3f + 1, from - 2), Color.Lerp(color, Color.Black, 0.5f) * lineOpacity);
                }

            }
        }
        #endregion
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(particlesBG);
        }
        #region Hooks and Loading
        private static void MovingGliderPickup(On.Celeste.Glider.orig_OnPickup orig, Glider self)
        {
            if (self is MovingJelly jelly)
            {
                jelly.isHeld = true;
                jelly.hitWall = false;
                jelly.hitFloor = false;
                if (!jelly.movePlayer)
                {
                    jelly.arrowSprite.Play("load");
                }
            }
            orig(self);
        }
        private static void MovingGliderRelease(On.Celeste.Glider.orig_OnRelease orig, Glider self, Vector2 force)
        {
            if (self is MovingJelly jelly)
            {
                Player player = jelly.Scene.Tracker.GetEntity<Player>();
                jelly.Position.X += jelly.IsVertical ? player.Facing == Facings.Right ? -1.67f : 1.67f : 0;
                jelly.Position.Y += jelly.IsHorizontal ? jelly.movePlayer ? 1 : 0 : 0;
                jelly.isHeld = false;
                jelly.wasThrown = true;

            }
            orig(self, force);
        }

        private static void MovingGliderOnCollideH(On.Celeste.Glider.orig_OnCollideH orig, Glider self, CollisionData data)
        {
            if (self is MovingJelly jelly)
            {
                jelly.hitWall = true;
                Coroutine coroutine = new Coroutine(jelly.WaitThenSet(true, false));
                jelly.Add(coroutine);
            }
            orig(self, data);
        }

        private static void MovingGliderOnCollideV(On.Celeste.Glider.orig_OnCollideV orig, Glider self, CollisionData data)
        {
            bool disableAudio = false;
            if (self is MovingJelly jelly)
            {
                jelly.hitFloor = true;
                disableAudio = jelly.DisableAudio;
                Coroutine coroutine = new Coroutine(jelly.WaitThenSet(false, false));
                jelly.Add(coroutine);
            }
            if (disableAudio)
            {
                if (data.Hit is DashSwitch)
                {
                    (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(self.Speed.X));
                }
                self.Speed.X *= -1f;
            }
            else
            {
                orig(self, data);
            }

        }
        internal static void Load()
        {
            On.Celeste.Glider.OnPickup += MovingGliderPickup;
            On.Celeste.Glider.OnRelease += MovingGliderRelease;
            On.Celeste.Glider.OnCollideH += MovingGliderOnCollideH;
            On.Celeste.Glider.OnCollideV += MovingGliderOnCollideV;
        }

        internal static void Unload()
        {
            On.Celeste.Glider.OnPickup -= MovingGliderPickup;
            On.Celeste.Glider.OnRelease -= MovingGliderRelease;
            On.Celeste.Glider.OnCollideV -= MovingGliderOnCollideV;
            On.Celeste.Glider.OnCollideH -= MovingGliderOnCollideH;
        }
        #endregion

        #region Particles
        private static ParticleType ArrowDust = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("00ff00"),
            Color2 = Calc.HexToColor("94ff00"),
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -(float)Math.PI / 2f,
            DirectionRange = (float)Math.PI / 3f,
            LifeMin = 0.5f,
            LifeMax = 0.7f,
            SpeedMin = 10f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.2f,
            FadeMode = ParticleType.FadeModes.InAndOut,
            Friction = 1f
        };

        private static ParticleType ArrowLoad = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("67ff6e"),
            Color2 = Calc.HexToColor("35b37b"),
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -(float)Math.PI / 2f,
            DirectionRange = (float)Math.PI / 3f,
            LifeMin = 0.1f,
            LifeMax = 2f,
            SpeedMin = 25f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.40f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 2f
        };
        private void AppearParticles(bool idle)
        {
            float dustY = CurrentDirection() == Direction.Up ? arrow.Center.Y + 4 + Height : arrow.Center.Y + 4;
            if (idle)
            {
                particlesBG.Emit(ArrowDust, 1, new Vector2(arrow.Center.X + Width / 2 + 2, dustY), Vector2.One * 2f, MathHelper.Pi / 2);
            }
            else
            {
                gliderData.Get<Sprite>("Texture").Scale.Y = Calc.Approach(gliderData.Get<Sprite>("Texture").Scale.Y, Vector2.One.Y, Engine.DeltaTime * 2f);
                gliderData.Get<Sprite>("Texture").Scale.X = Calc.Approach(gliderData.Get<Sprite>("Texture").Scale.X, (float)Math.Sign(gliderData.Get<Sprite>("Texture").Scale.X) * Vector2.One.X, Engine.DeltaTime * 2f);
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(ArrowLoad, 2, new Vector2(arrow.Center.X + Width / 2, arrow.Center.Y + 4), Vector2.One * 2f, i * (MathHelper.Pi / 180f));
                }
            }
        }
        #endregion

    }

}