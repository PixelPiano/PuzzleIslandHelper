using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [Tracked]
    public class SlopeSpeedController : Entity
    {
        public static float SpeedMult = 1;
        public bool updated;
        private static ILHook speedHook;
        public SlopeSpeedController()
        {
            Tag |= Tags.Persistent | Tags.Global;
            Add(new PostUpdateHook(() =>
            {
                if (!updated)
                {
                    SpeedMult = 1;
                }
                updated = false;

            }));
        }
        public void UpdateSpeed(float newSpeedMult)
        {
            if (!updated)
            {
                SpeedMult = newSpeedMult;
                updated = true;
            }
        }
        [OnLoad]
        public static void Load()
        {
            SpeedMult = 1;
            speedHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance), modSpeed);
        }
        [OnUnload]
        public static void Unload()
        {
            SpeedMult = 1;
            speedHook?.Dispose();
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            SpeedMult = 1;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            SpeedMult = 1;
        }
        private static float getSpeedXMultiplier()
        {
            return SpeedMult;
        }
        private static void modSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Actor>("MoveH")))
            {
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("X")))
                {
                    cursor.EmitDelegate(getSpeedXMultiplier);
                    cursor.Emit(OpCodes.Mul);
                }
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/SlopeBeta")]
    [Tracked]
    public class SlopeBeta : Entity
    {
        [TrackedAs(typeof(JumpThru))]
        public class SlopePlatform : JumpThru
        {
            
            public SlopeBeta Slope;
            public Collider Detect;
            public SlopePlatform(SlopeBeta slope, Vector2 position, int width, bool safe) : base(position, width, safe)
            {
                Slope = slope;
                Detect = new Hitbox(Width + 12, Height + 6, -6, -6);
            }
            public bool PlayerInDetect() => CheckDetect<Player>();
            public bool CheckDetect<T>() where T : Entity
            {
                bool collidable = Collidable;
                Collider collider = Collider;
                Collidable = true;
                Collider = Detect;
                bool collided = CollideCheck<T>();
                Collidable = collidable;
                Collider = collider;
                return collided;
            }
            public bool CheckDetect(Entity entity)
            {
                bool collidable = Collidable;
                Collider collider = Collider;
                Collidable = true;
                Collider = Detect;
                bool collided = CollideCheck(entity);
                Collidable = collidable;
                Collider = collider;
                return collided;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Collider prev = Collider;
                Collider = Detect;
                Draw.HollowRect(Collider, Collidable ? Color.Blue : Color.DarkBlue);
                Collider = prev;
            }
        }
        public SlopeSpeedController Controller;
        public static readonly Ease.Easer CircleIn = (float t) => (float)(1 - (double)Math.Sqrt(1 - (double)Math.Pow(t, 2)));
        public static readonly Ease.Easer CircleOut = (float t) => (float)(double)Math.Sqrt(1 - (double)Math.Pow(t - 1, 2));
        public SlopePlatform Platform;
        public FlagList FlagOnUseSlope, CollidableFlag;
        public Vector2 Low, Hi;
        public readonly (Vector2 A, Vector2 B) Points;
        public float Eased => easer(Percent);
        public float Percent;
        public float NoCollideTimer;
        public Facings Facing => (Facings)Math.Sign(Hi.X - Low.X);
        public bool FacingOffset;
        public bool OnlyIfHoldingUp;
        private Ease.Easer easer = Ease.Linear;
        private int customDepth;
        private float leftThresh, rightThresh, speedChange;
        private float xExtend;
        private float platformWidth;
        private bool flagState;
        private bool invertFlagWhenOffSlope;
        public enum Inputs
        {
            None, Up, Down, Left, Right, Any
        }
        private Inputs aDirection, bDirection, middleDirection;
        private bool hadPlayer;
        private SlopePlatform waitFor;
        public SlopeBeta(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            aDirection = data.Enum<Inputs>("fallInputA");
            bDirection = data.Enum<Inputs>("fallInputB");
            middleDirection = data.Enum<Inputs>("fallInputMiddle");
            leftThresh = data.Float("leftSpeedThresh");
            rightThresh = data.Float("rightSpeedThresh");
            speedChange = data.Float("speedChangeMult");
            FacingOffset = data.Bool("facingOffset");
            platformWidth = data.Float("platformWidth", 16);
            xExtend = data.Float("xExtend", 8);
            customDepth = Depth = data.Int("depth");
            flagState = data.Bool("flagState");
            invertFlagWhenOffSlope = data.Bool("invertFlagWhenOffSlope");
            OnlyIfHoldingUp = data.Bool("requireUpInput");
            CollidableFlag = data.FlagList("collidableFlag");
            FlagOnUseSlope = data.FlagList("flagWhenOnSlope");
            Vector2[] array = data.NodesWithPosition(offset);
            if (array[0].Y > array[1].Y)
            {
                Low = array[0];
                Hi = array[1];
            }
            else
            {
                Low = array[1];
                Hi = array[0];
            }
            if (Facing == Facings.Left)
            {
                Points = (Hi, Low);
            }
            else
            {
                Points = (Low, Hi);
            }

            float left = Math.Min(Low.X, Hi.X);
            float right = Math.Max(Low.X, Hi.X);
            float top = Math.Min(Low.Y, Hi.Y);
            float bottom = Math.Max(Low.Y, Hi.Y);
            float width = right - left;
            float height = bottom - top;

            Position = new Vector2(left, top);
            Collider = new Hitbox(width, height);
            easer = data.Attr("ease") switch
            {
                "SineIn" => Ease.SineIn,
                "SineOut" => Ease.SineOut,
                "SineInOut" => Ease.SineInOut,
                "CircleIn" => CircleIn,
                "CircleOut" => CircleOut,
                _ => Ease.Linear
            };
        }
        public void PositionUpdate(Actor actor)
        {
            Facings facing = Facing;
            float farLeft = Left - xExtend;
            float farRight = Right + xExtend;
            float mult = FacingOffset ? (int)facing : 0;
            float playerPosition = actor.CenterX + (Platform.Width / 2) * mult;
            float center = Calc.Clamp(playerPosition, farLeft, farRight);
            float platformOffset = Platform.Width / 2 * mult;
            float percent = Calc.Clamp((center - Left) / Width, 0, 1);
            float y = Calc.LerpClamp(Points.A.Y, Points.B.Y, easer(percent));
            Percent = percent;
            Platform.CenterX = center - platformOffset;
            Platform.MoveToY(y, 0);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Low.X == Hi.X)
            {
                RemoveSelf();
                return;
            }
            Platform = new SlopePlatform(this, Low, (int)platformWidth, true);
            Platform.Depth = customDepth;
            scene.Add(Platform);

            Controller = PianoUtils.SeekController<SlopeSpeedController>(scene);
            if (Controller == null)
            {
                scene.Add(Controller = new SlopeSpeedController());
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is Player player)
            {
                if (player.OnGround())
                {
                    if (player != Platform.GetPlayerRider())
                    {
                        NoCollideTimer = 0.5f;
                    }
                    /*                    else if (OnlyIfHoldingUp && Input.MoveY < 0)
                                        {
                                            //if player is riding and holding up, activate the hold up condition
                                            holdingUpCollisionCondition = true;
                                        }*/
                }
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Circle(Low, 4, Color.Magenta, 4);
            Draw.Circle(Hi, 4, Color.Cyan, 4);
        }

        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;

            CollideUpdate(player);
            PositionUpdate(player);
            FlagUpdate();
            PlayerSpeedUpdate();
        }
        public bool CanFall(float minPercent, float maxPercent, Inputs input)
        {
            if (Percent >= minPercent && Percent <= maxPercent)
            {
                switch (input)
                {
                    case Inputs.None:
                        return false;
                    case Inputs.Up:
                        return Input.MoveY == -1 && Input.MoveX == 0;
                    case Inputs.Down:
                        return Input.MoveY == 1 && Input.MoveX == 0;
                    case Inputs.Left:
                        return Input.MoveX == -1 && Input.MoveY == 0;
                    case Inputs.Right:
                        return Input.MoveX == 1 && Input.MoveY == 0;
                    case Inputs.Any:
                        return Input.MoveY != 0 || Input.MoveX != 0;
                }
            }
            return false;
        }
        public void PlayerSpeedUpdate()
        {
            if ((leftThresh > 0 || rightThresh > 0) && speedChange > 0 && !Controller.updated && Platform.HasPlayerRider())
            {
                float mult;
                float percent = Percent;
                if (percent < leftThresh)
                {
                    mult = percent / leftThresh;
                }
                else if (percent > 1 - rightThresh)
                {
                    mult = 1 - ((percent - (1 - rightThresh)) / rightThresh);
                }
                else
                {
                    mult = 1;
                }
                Controller.UpdateSpeed(1 - speedChange + speedChange * mult);
            }
        }
        public void CollideUpdate(Player player)
        {
            if (NoCollideTimer == 0)
            {
                bool riding = player.IsRiding(Platform);
                if (riding)
                {
                    if (CanFall(0, 0, aDirection) || (CanFall(1, 1, bDirection) || CanFall(0, 1, middleDirection)))
                    {
                        NoCollideTimer = 0.25f;
                    }
                }
                if (OnlyIfHoldingUp)
                {
                    if (player.Bottom > Top && !riding && !(Input.MoveY != -1 && Platform.PlayerInDetect()))
                    {
                        if (NoCollideTimer < Engine.DeltaTime * 2)
                        {
                            NoCollideTimer = Engine.DeltaTime * 2;
                        }
                    }
                }
            }

            if (NoCollideTimer > 0)
            {
                NoCollideTimer -= Engine.DeltaTime;
                if (NoCollideTimer < 0)
                {
                    NoCollideTimer = 0;
                }
            }
            Platform.Collidable = Collidable && NoCollideTimer == 0 && CollidableFlag && waitFor == null;
        }
        public void FlagUpdate()
        {
            if (!FlagOnUseSlope.Empty)
            {
                if (Platform.Collidable && Platform.HasPlayerRider())
                {
                    if (!hadPlayer)
                    {
                        FlagOnUseSlope.State = flagState;
                    }
                    hadPlayer = true;
                }
                else
                {
                    if (hadPlayer && invertFlagWhenOffSlope)
                    {
                        FlagOnUseSlope.State = !flagState;
                    }
                    hadPlayer = false;
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (easer == Ease.Linear)
            {
                Draw.Line(Low, Hi, Color.White);
            }
            else
            {
                Vector2 vector = Points.A;
                for (float i = 0; i < 1; i += 0.02f)
                {
                    Vector2 point = new Vector2(Calc.LerpClamp(Points.A.X, Points.B.X, i), Calc.LerpClamp(Points.A.Y, Points.B.Y, easer(i)));
                    Draw.Line(vector, point, Color.White);
                    vector = point;
                }
            }
        }
        public Vector2 GetPoint(float percent)
        {
            return new Vector2(Calc.LerpClamp(Points.A.X, Points.B.X, percent), Calc.LerpClamp(Points.A.Y, Points.B.Y, easer(percent)));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform?.RemoveSelf();
        }

    }
}
