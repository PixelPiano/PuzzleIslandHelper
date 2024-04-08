using Celeste.Mod.CommunalHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Reflection;
// PuzzleIslandHelper.FluidBottle
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [Tracked]
    public class PotionFluid : Actor
    {
        public enum PotionEffects
        {
            Sticky,
            Hot,
            Bouncy,
            Refill,
            Invert
        }

        public PotionEffects Effect;
        private Player player;
        private Level l;
        private bool Permanent;
        private bool Climbing
        {
            get
            {
                if (player is not null)
                {
                    return player.StateMachine.State == Player.StClimb;
                }
                else
                {
                    return false;
                }
            }
        }
        private List<Vector2> Placements = new();
        private List<FluidBottle.Side> Sides = new();
        private MTexture SideTexture;
        private MTexture CornerTexture;
        private Color Color;
        private bool collide;
        private Color orig_Color;
        private float Duration = 30;
        private float Alpha;
        private bool Activated;
        private bool Disappearing;
        private static ILHook speedHook;
        private static ILHook wallJumpHook;
        private float BounceCooldown;
        private float SavedY;
        private bool Hanging;
        public List<Fluid> Fluids = new();
        public List<Fluid> CeilingFluids = new();
        private VirtualButton grabButton;
        private VirtualButton downButton;
        private VirtualMap<char> Map;
        private Rectangle PlayerTop;
        public struct Fluid
        {
            private Player player;
            public Vector2 Position { get; private set; }
            public FluidBottle.Side Side { get; private set; }
            public Collider Collider { get; private set; }
            public Rectangle Bounds { get; private set; }
            private Level level;
            public Vector2 TilePosition
            {
                get
                {
                    Vector2 offset = Side switch
                    {
                        FluidBottle.Side.L => -Vector2.UnitX,
                        FluidBottle.Side.R => Vector2.UnitX,
                        FluidBottle.Side.D => -Vector2.UnitY,
                        FluidBottle.Side.U => Vector2.UnitY,
                        FluidBottle.Side.DL => new Vector2(1, -1),
                        FluidBottle.Side.DR => -Vector2.One,
                        FluidBottle.Side.UR => new Vector2(-1, 1),
                        FluidBottle.Side.UL => Vector2.One,
                        _ => Vector2.Zero
                    };
                    return Position + offset;
                }
            }

            public char SideChar
            {
                get
                {
                    char c = Side switch
                    {
                        FluidBottle.Side.L => 'L',
                        FluidBottle.Side.R => 'R',
                        FluidBottle.Side.D => 'D',
                        FluidBottle.Side.U => 'U',
                        FluidBottle.Side.DL => '[',
                        FluidBottle.Side.DR => ']',
                        FluidBottle.Side.UR => '}',
                        FluidBottle.Side.UL => '{',
                        _ => '\0'
                    };
                    return c;
                }
            }
            public bool IsLR
            {
                get
                {
                    string name = Side.ToString();
                    return name.Contains("L") || name.Contains("R");
                }
            }
            public bool IsUD
            {

                get
                {
                    string name = Side.ToString();
                    return name.Contains("U") || name.Contains("D");
                }
            }
            public bool IsCorner
            {
                get
                {
                    return IsLR && IsUD;
                }
            }
            public bool IsCeiling
            {
                get
                {
                    return Side == FluidBottle.Side.D;
                }
            }
            public Fluid(Vector2 Position, FluidBottle.Side Side, Collider Collider, Player player, Level level)
            {
                this.player = player;
                this.Position = Position;
                this.Side = Side;
                this.Collider = Collider;
                Bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Collider.Width, (int)Collider.Height);
                this.level = level;
            }

        }
        private void SetMapData(Level level, List<Fluid> fluids)
        {
            int x = level.Bounds.Width / 8;
            int y = level.Bounds.Height / 8;
            char[,] FluidTiles = new char[x, y];
            Grid grid = new Grid(x, y, 8, 8);
            grid.Position = new Vector2(level.Bounds.Left, level.Bounds.Top);
            bool[,] bools = new bool[x, y];
            foreach (Fluid fluid in fluids)
            {
            }
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {

                }
            }

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    bools[i, j] = FluidTiles[i, j] != '\0';
                }
            }
            for (int i = 0; i < FluidTiles.GetLength(0); i++)
            {
                for (int j = 0; j < FluidTiles.GetLength(1); j++)
                {

                }
            }
        }
        public PotionFluid(List<Vector2> TilePlacements, List<FluidBottle.Side> TileSides, PotionEffects Effect, bool permanent)
        : base(Vector2.Zero)
        {
            Placements = TilePlacements;
            Sides = TileSides;
            this.Effect = Effect;
            SideTexture = GFX.Game["objects/PuzzleIslandHelper/potion/side"];
            CornerTexture = GFX.Game["objects/PuzzleIslandHelper/potion/corner"];
            Alpha = 1;
            Depth = -10001;
            Permanent = permanent;
            Color = Effect switch
            {
                PotionEffects.Hot => Color.Red,
                PotionEffects.Refill => Color.Pink,
                PotionEffects.Bouncy => Color.Blue,
                PotionEffects.Sticky => Color.Green,
                PotionEffects.Invert => Color.Gray,
                _ => Color.White
            };
            orig_Color = Color;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            l = scene as Level;
            player = l.Tracker.GetEntity<Player>();
            PlayerTop = new Rectangle((int)player.X - (int)player.Width / 2, (int)player.Y - 5 - (int)player.Height, (int)player.Width, 5);
            grabButton = (VirtualButton)typeof(Input).GetField("Grab").GetValue(null);
            //downButton = (VirtualButton)typeof(Input).GetField("Down").GetValue(null);

            List<Vector2> temp = new();
            List<FluidBottle.Side> tempSide = new();
            for (int i = 0; i < temp.Count; i++)
            {
                if (PianoModule.Session.PotionTiles.Contains(Placements[i]))
                {
                    temp.Add(Placements[i]);
                    tempSide.Add(Sides[i]);
                }
            }
            foreach (FluidBottle.Side side in tempSide)
            {
                Sides.Remove(side);
            }
            foreach (Vector2 vector in temp)
            {
                Placements.Remove(vector);
            }

            PianoModule.Session.PotionTiles.AddRange(Placements);
            List<Collider> Colliders = new();

            for (int i = 0; i < Sides.Count; i++)
            {
                int x = 0, y = 0, width = 0, height = 0;
                switch (Sides[i])
                {
                    case FluidBottle.Side.L:
                        x = 1;
                        break;
                    case FluidBottle.Side.R:
                        x = -1;
                        break;
                    case FluidBottle.Side.D:
                        y = 1;
                        break;
                    case FluidBottle.Side.U:
                        y = -1;
                        break;
                    case FluidBottle.Side.DL:
                        width = height = 1;
                        y = -1;
                        x = -1;
                        break;
                    case FluidBottle.Side.DR:
                        width = height = 1;
                        y = -1;
                        x = -1;
                        break;
                    case FluidBottle.Side.UR:
                        width = height = 1;
                        x = -1;
                        y = -1;
                        break;
                    case FluidBottle.Side.UL:
                        width = height = 1;
                        x = -1;
                        y = -1;
                        break;
                }
                Colliders.Add(new Hitbox(8 + width * 2, 8 + height * 2, Placements[i].X + x, Placements[i].Y + y));
            }

            for (int i = 0; i < Placements.Count; i++)
            {
                Fluids.Add(new Fluid(Placements[i], Sides[i], Colliders[i], player, l));
            }
            foreach (Fluid fluid in Fluids)
            {
                if (fluid.IsCeiling)
                {
                    CeilingFluids.Add(fluid);
                }
            }
            if (Colliders.Count != 0)
            {
                Collider = new ColliderList(Colliders.ToArray());
            }
            //SetMapData(level, Fluids);
        }
        public override void Render()
        {
            base.Render();
            for (int i = 0; i < Fluids.Count; i++)
            {
                DrawTexture(Fluids[i].Position + Vector2.One * 4, Color, Fluids[i].Side);
            }
        }


        private void FluidEffect(PotionEffects effect, Player player)
        { //Player is affected if they are above or to the side of the fluid, lasts for the duration 

            bool Colliding = CollideCheck<Player>();

            bool CollideH = false;
            bool CollideV = false;
            if (Colliding)
            {
                CollideV = player.OnGround();
                CollideH = Climbing;
            }

            //Constant effects

            switch (effect)
            {
                case PotionEffects.Hot:
                    if (Colliding)
                    {
                        player.Die(Vector2.Zero);
                    }
                    break;
                case PotionEffects.Bouncy:
                    if (Colliding)
                    {
                        if (BounceCooldown <= 0)
                        {
                            BounceCooldown = 0.4f;
                            if (player.StateMachine.State == Player.StDash)
                            {
                                player.SuperBounce(player.Center.Y);
                            }
                            else
                            {
                                if (CollideV)
                                {
                                    player.Bounce(player.Center.Y);
                                }
                            }
                        }
                        Activated = false;
                    }
                    break;
                case PotionEffects.Sticky:
                    if (Colliding && grabButton.Check && Input.MoveY != 1) //If colliding and climbing and not holding down
                    {
                        if (player.StateMachine.State == Player.StDash)
                        {
                            PianoModule.Session.PotionSpeedMult.X = 1;
                            if (Hanging)
                            {
                                Hanging = false;
                                player.RefillDash();
                                break;
                            }
                        }
                        foreach (Fluid fluid in CeilingFluids)
                        {
                            bool Intersects = PlayerTop.Intersects(fluid.Bounds);
                            if (Intersects)
                            {
                                if (Climbing)
                                {
                                    if ((int)player.Facing == -1)
                                    {
                                        if (Input.MoveX != 1)
                                        {
                                            break;
                                        }
                                        player.StateMachine.State = Player.StNormal;
                                    }
                                    else
                                    {
                                        if (Input.MoveX != -1)
                                        {
                                            break;
                                        }
                                        player.StateMachine.State = Player.StNormal;
                                    }
                                }
                                SavedY = fluid.Position.Y + 8;
                                Hanging = true;

                                break;
                            }
                            else
                            {
                                Hanging = false;
                            }
                        }
                    }
                    else
                    {
                        Hanging = false;
                    }
                    break;
            }
            //Conditional effects
            if (!Activated && (CollideV || CollideH))
            {
                Activated = true;
                switch (effect)
                {
                    case PotionEffects.Sticky:
                        if (!Hanging)
                        {
                            PianoModule.Session.PotionSpeedMult = Vector2.One * 0.4f;
                        }
                        break;

                    case PotionEffects.Refill:
                        player.RefillDash();
                        break;
                    case PotionEffects.Invert:
                        Color = Color.Invert();
                        break;
                }
            }
            else
            {

                if (!CollideV && !CollideH)
                {
                    Activated = false;
                    EffectCleanup(Effect);
                }
            }

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(PlayerTop, Color.Magenta);
        }
        private void EffectCleanup(PotionEffects effect)
        {
            switch (effect)
            {
                case PotionEffects.Sticky:
                    PianoModule.Session.PotionSpeedMult = Vector2.One;
                    break;
                case PotionEffects.Bouncy:

                    break;
                case PotionEffects.Refill:
                    break;
                case PotionEffects.Invert:
                    Color = orig_Color;
                    break;
            }
        }
        public override void Update()
        {
            base.Update();
            if (player is null)
            {
                return;
            }
            PlayerTop.X = (int)player.X - (int)player.Width / 2;
            PlayerTop.Y = (int)player.Y - 5 - (int)player.Height;
            if (Hanging)
            {
                player.TopCenter = new Vector2(player.TopCenter.X, SavedY);
            }

            if (PianoModule.Session.PotionSpeedMult == Vector2.One * 0.4f)
            {
                player.Stamina = 110f;
            }
            if (Permanent || !Disappearing)
            {
                FluidEffect(Effect, player);
            }
            if (BounceCooldown > 0)
            {
                BounceCooldown -= Engine.DeltaTime;
            }

            #region Visuals
            if (!Permanent)
            {
                if (Duration > 0)
                {
                    Duration -= Engine.DeltaTime;
                }
                if (Duration <= 0 && Alpha > 0)
                {
                    Disappearing = true;
                    Alpha -= Engine.DeltaTime;
                    Color = orig_Color * Alpha;
                }
                if (Alpha <= 0)
                {
                    RemoveSelf();
                }
            }

            #endregion
        }

        private void DrawTexture(Vector2 Position, Color color, FluidBottle.Side side)
        {

            switch (side)
            {
                case FluidBottle.Side.L:
                    SideTexture?.Draw(Position + new Vector2(2, -4), Vector2.Zero, color, 1, 0);
                    break;
                case FluidBottle.Side.R:
                    SideTexture?.Draw(Position - new Vector2(2, -4), Vector2.Zero, color, 1, 180f.ToRad());
                    break;
                case FluidBottle.Side.D:
                    SideTexture?.Draw(Position + new Vector2(4, 2), Vector2.Zero, color, 1, 90f.ToRad());
                    break;
                case FluidBottle.Side.U:
                    SideTexture?.Draw(Position - new Vector2(4, 2), Vector2.Zero, color, 1, -90f.ToRad());
                    break;
                case FluidBottle.Side.DL:
                    CornerTexture?.Draw(Position + new Vector2(4, 4), Vector2.Zero, color, 1, 180f.ToRad());
                    break;
                case FluidBottle.Side.DR:
                    CornerTexture?.Draw(Position + new Vector2(-4, 4), Vector2.Zero, color, 1, -90f.ToRad());
                    break;
                case FluidBottle.Side.UR:
                    CornerTexture?.Draw(Position - new Vector2(4, 4), Vector2.Zero, color, 1, 0);
                    break;
                case FluidBottle.Side.UL:
                    CornerTexture?.Draw(Position + new Vector2(4, -4), Vector2.Zero, color, 1, 90f.ToRad());
                    break;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Vector2 vector in Placements)
            {
                if (PianoModule.Session.PotionTiles.Contains(vector))
                {
                    PianoModule.Session.PotionTiles.Remove(vector);
                }
            }
        }
        #region Annoying stinky blegh
        public static void Load()
        {
            speedHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance), modSpeed);
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
            IL.Celeste.Player.Jump += modJump;
        }
        public static void Unload()
        {
            speedHook?.Dispose();
            wallJumpHook?.Dispose();
            speedHook = null;
            wallJumpHook = null;
            IL.Celeste.Player.Jump -= modJump;
        }
        private static void modJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                Logger.Log("ExtendedVariantMode/JumpHeight", $"Modding constant at {cursor.Index} in CIL code for Jump to make jump height editable");

                cursor.EmitDelegate(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }

        private static void modWallJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // we want to multiply -105f (height given by a superdash) with the jump height factor
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-105f)))
            {
                cursor.EmitDelegate(determineJumpHeightFactor);
                cursor.Emit(OpCodes.Mul);
            }
        }
        private static bool ShouldContinue()
        {
            if (Engine.Scene.Tracker.GetEntity<PotionFluid>() == null)
            {
                return false;
            }
            return true;
        }
        private static float determineJumpHeightFactor()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.PotionJumpMult;
        }
        private static float getSpeedYMultiplier()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.PotionSpeedMult.Y;
        }
        private static float getSpeedXMultiplier()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.PotionSpeedMult.X;
        }

        private static void modSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Actor>("MoveH")))
            {
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("X")))
                {
                    Logger.Log("PuzzleIslandHelper/FluidMachine", $"Modding dash speed at index {cursor.Index} in CIL code for {cursor.Method.Name}");

                    cursor.EmitDelegate(getSpeedXMultiplier);
                    cursor.Emit(OpCodes.Mul);

                }
            }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallOrCallvirt<Actor>("MoveV")))
            {
                if (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("Y")))
                {
                    Logger.Log("PuzzleIslandHelper/FluidMachine", $"Modding dash speed at index {cursor.Index} in CIL code for {cursor.Method.Name}");

                    cursor.EmitDelegate(getSpeedYMultiplier);
                    cursor.Emit(OpCodes.Mul);

                }
            }
        }
        #endregion
    }
}