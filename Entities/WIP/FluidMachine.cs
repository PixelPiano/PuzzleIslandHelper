using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// PuzzleIslandHelper.FluidMachine
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/FluidMachine")]
    [Tracked]
    public class FluidMachine : Entity
    {

        private static Color ColorState;
        private bool Climbing;
        private static float HairAlpha;
        private const float ShrinkMult = 0.4f;
        private const float Slowed = 0.6f;
        private const int Stuck = 0;
        private const float Slower = 0.2f;
        private const float Faster = 1.5f;
        private List<TalkComponent> talks = new();
        private List<int> ActiveEffects = new();
        private float Duration = 15;
        private Player player;
        private bool Bouncing;
        private static ILHook speedHook;
        private static ILHook wallJumpHook;
        private ParticleSystem system;
        private List<Player> Players = new();
        private List<Alarm> alarms = new();
        private Vector2 Direction;
        private ParticleType Slime = new ParticleType
        {
            Color = Color.Green,
            Color2 = Color.DarkGreen,
            SpeedMin = 100,
            SpeedMax = 100,
            Direction = (float)(Math.PI / -2f) * 3,
            LifeMax = 2,
            LifeMin = 0.5f,

        };

        public FluidMachine(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Action<Player>[] actions = { Interact1, Interact2, Interact3, Interact4 };
            for (int i = 0; i < 4; i++)
            {
                Vector2 position = new Vector2(i * 24, 0);
                Rectangle rect = new Rectangle((int)position.X, 0, 16, 16);
                talks.Add(new TalkComponent(rect, new Vector2(position.X + rect.Width / 2, 0), actions[i]));
            }
            Add(new DashListener(OnDash));
            Collider = new Hitbox(8, 8);
            Add(talks.ToArray());

        }
        private void OnDash(Vector2 direction)
        {
            Direction = direction * 4;
            if (ActiveEffects.Contains(3))
            {
                Direction *= 2;
            }
        }
        private void OnCollideH(CollisionData data)
        {
            Direction.X = -Direction.X;
        }
        private void OnCollideV(CollisionData data)
        {
            Direction.Y = -Direction.Y;
        }
        private IEnumerator Effect(int variant, Player player)
        {
            ActiveEffects.Add(variant);
            switch (variant)
            {
                case 1:
                    #region Sticky
                    for (int j = 0; j < 4; j++)
                    {
                        for (float i = 0; i < 1.5f; i += Engine.DeltaTime)
                        {
                            PianoModule.Session.SpeedMult.X = Slowed;
                            PianoModule.Session.SpeedMult.Y = Climbing ? Slowed : 1;
                            yield return null;
                        }
                        for (float i = 0; i < 0.4f; i += Engine.DeltaTime)
                        {
                            PianoModule.Session.SpeedMult.X = Slower;
                            PianoModule.Session.SpeedMult.Y = Climbing ? Slower : 1f;
                            yield return null;
                        }
                    }
                    PianoModule.Session.SpeedMult = Vector2.One;
                    yield return null;
                    #endregion
                    break;
                case 2:
                    #region Bouncy
                    while (!player.DashAttacking)
                    {
                        yield return null;
                    }
                    yield return null;
                    foreach (Player playerr in Players)
                    {
                        playerr.StateMachine.State = Player.StFrozen;
                    }
                    player.ForceCameraUpdate = true;
                    Bouncing = true;
                    PianoModule.Session.SpeedMult = Vector2.Zero;
                    for (float i = 0; i < Duration; i += Engine.DeltaTime)
                    {
                        foreach (Player playerr in Players)
                        {
                            playerr.MoveH(Direction.X * Engine.DeltaTime * 30, OnCollideH);
                            playerr.MoveV(Direction.Y * Engine.DeltaTime * 30, OnCollideV);
                        }
                        yield return null;
                    }
                    PianoModule.Session.SpeedMult = Vector2.One;
                    yield return null;
                    Bouncing = false;
                    player.StateMachine.State = Player.StNormal;
                    foreach (Player playerr in Players)
                    {

                        playerr.StateMachine.State = Player.StNormal;

                    }
                    player.ForceCameraUpdate = false;
                    #endregion
                    break;
                case 3:
                    #region Shrink
                    for (float i = 0; i < 1; i += Engine.DeltaTime * 1.5f)
                    {
                        PianoModule.Session.CurrentScale = Calc.LerpClamp(1, ShrinkMult, i); //shrink the Player
                        HairAlpha = 1 - i * 1.5f;
                        yield return null;
                    }
                    for (float j = 0; j < Duration; j += Engine.DeltaTime)
                    {

                        if (!ActiveEffects.Contains(1) && !Bouncing)
                        {
                            PianoModule.Session.SpeedMult = Vector2.One * Faster;
                        }
                        PianoModule.Session.CurrentScale = ShrinkMult;
                        HairAlpha = 0;
                        yield return null;
                    }
                    if (!ActiveEffects.Contains(4))
                    {
                        PianoModule.Session.SpeedMult = Vector2.One;
                    }
                    for (float i = 0; i < 1; i += Engine.DeltaTime * 1.5f)
                    {
                        PianoModule.Session.CurrentScale = Calc.LerpClamp(ShrinkMult, 1, i); //revert the Player's Size
                        HairAlpha = i * 1.5f;
                        yield return null;
                    }
                    #endregion
                    break;
                case 4:
                    #region Copy
                    Player SecondPlayer = new Player(Players[Players.Count - 1].Position + Vector2.UnitX * 8, PlayerSpriteMode.Madeline);
                    SceneAs<Level>().Add(SecondPlayer);
                    Players.Add(SecondPlayer);
                    yield return Duration;

                    for (int i = 0; i < 8; i++)
                    {
                        SecondPlayer.Visible = !SecondPlayer.Visible;
                        yield return Engine.DeltaTime * 3;
                    }
                    SecondPlayer.Visible = false;
                    Players.Remove(SecondPlayer);
                    SceneAs<Level>().Remove(SecondPlayer);

                    #endregion
                    break;
            }
            ActiveEffects.Remove(variant);
            yield return null;
        }
        public override void Update()
        {
            base.Update();
            if (player is null)
            {
                return;
            }
            foreach (Player player in Players)
            {
                if (player is not null)
                {
                    player.Sprite.Rate = ActiveEffects.Contains(1) ? PianoModule.Session.SpeedMult.X == Slowed ? 0.4f : PianoModule.Session.SpeedMult.X == Slower ? 0.2f : 0 : 1;
                    player.OverrideHairColor = ActiveEffects.Contains(1) && player.Dashes != 0 ? Color.Green : null;
                    player.Hair.Alpha = HairAlpha;
                }
            }

            system.Position = player.TopLeft;
            Climbing = player.StateMachine.State == Player.StClimb;
            ColorState = ActiveEffects.Contains(1) ? Color.Green : Color.White;
            PianoModule.Session.JumpMult = ActiveEffects.Contains(1) ? 0.4f : 1;
            if (ActiveEffects.Contains(1))
            {
                EmitSlime();
            }
        }

        private void EmitSlime()
        {
            foreach (Player player in Players)
            {
                int rand = Calc.Random.Range(0, 11);
                if (rand == 10)
                {
                    float randX = Calc.Random.Range(0, player.Width);
                    float randY = Calc.Random.Range(player.Height / 2, player.Height);
                    system.Emit(Slime, new Vector2(randX, randY) + system.Position);
                }
            }
        }


        public static void Load()
        {
            speedHook = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance), modSpeed);
            wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic), modWallJump);
            On.Celeste.PlayerSprite.Render += RenderHook;
            IL.Celeste.Player.Jump += modJump;
        }
        public static void Unload()
        {
            speedHook?.Dispose();
            wallJumpHook?.Dispose();
            speedHook = null;
            wallJumpHook = null;
            On.Celeste.PlayerSprite.Render -= RenderHook;
            IL.Celeste.Player.Jump -= modJump;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            HairAlpha = 1;
            PianoModule.Session.CurrentScale = 1;
            PianoModule.Session.SpeedMult = Vector2.One;
            PianoModule.Session.JumpMult = 1;
            ColorState = Color.White;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            HairAlpha = 1;
            PianoModule.Session.CurrentScale = 1;
            PianoModule.Session.SpeedMult = Vector2.One;
            PianoModule.Session.JumpMult = 1;
            ColorState = Color.White;
        }
        private static bool ShouldContinue()
        {
            if (Engine.Scene is null)
            {
                return false;
            }
            if (Engine.Scene.Tracker.GetEntity<FluidMachine>() is null)
            {
                return false;
            }
            return true;
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
        private static float determineJumpHeightFactor()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.JumpMult;
        }
        private static float getSpeedYMultiplier()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.SpeedMult.Y;
        }
        private static float getSpeedXMultiplier()
        {
            if (!ShouldContinue())
            {
                return 1;
            }
            return PianoModule.Session.SpeedMult.X;
        }
        private bool ChooseEffect(int variant, Player player)
        {
            if (variant != 4)
            {
                if (ActiveEffects.Contains(variant))
                {
                    return false;
                }
            }
            else if (Players.Count > 4)
            {
                return false;
            }
            Add(new Coroutine(Effect(variant, player)));
            return true;
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
            /*            if (Cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<bool>("IsRendering")))
                        {
                            if (Cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("X")))
                            {
                                Logger.Log("PuzzleIslandHelper/FluidMachine", $"Modding dash speed at index {Cursor.Index} in CIL code for {Cursor.Method.Name}");

                                Cursor.EmitDelegate(getScaleMultiplier);
                                Cursor.Emit(OpCodes.Mul);

                            }
                        }
                        if (Cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<bool>("IsRendering")))
                        {
                            if (Cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdfld<Vector2>("X")))
                            {
                                Logger.Log("PuzzleIslandHelper/FluidMachine", $"Modding dash speed at index {Cursor.Index} in CIL code for {Cursor.Method.Name}");

                                Cursor.EmitDelegate(getScaleMultiplier);
                                Cursor.Emit(OpCodes.Mul);

                            }
                        }*/
        }
        private static void RenderHook(On.Celeste.PlayerSprite.orig_Render orig, PlayerSprite self)
        {
            if (!ShouldContinue())
            {
                orig(self);
                return;
            }
            if (ColorState != Color.White)
            {
                self.Color = ColorState;
            }

            Vector2 scale = self.Scale;
            if (PianoModule.Session.CurrentScale != 0)
            {
                self.Scale *= PianoModule.Session.CurrentScale;
            }
            orig(self);
            self.Scale = scale;
        }
        private void Interact1(Player player)
        {
            //Sticky
            ChooseEffect(1, player);
        }
        private void Interact2(Player player)
        {
            //Bouncy
            ChooseEffect(2, player);
        }
        private void Interact3(Player player)
        {
            //Smaller
            ChooseEffect(3, player);
        }
        private void Interact4(Player player)
        {
            //Copy
            ChooseEffect(4, player);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            Players.Add(player);
            scene.Add(system = new ParticleSystem(1, 40));
            HairAlpha = 1;
            PianoModule.Session.CurrentScale = 1;
            PianoModule.Session.SpeedMult = Vector2.One;
            PianoModule.Session.JumpMult = 1;
            ColorState = Color.White;
        }
    }
}