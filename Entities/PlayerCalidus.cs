using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using static Celeste.Mod.PuzzleIslandHelper.MenuElements.OuiFileFader;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PlayerCalidus")]
    [Tracked]
    public class PlayerCalidus : Calidus
    {
        public bool HasHead = true;
        public bool HasEye = true;
        public bool HasLeftArm = true;
        public bool HasRightArm = true;

        public Vector2 Speed;
        public PlayerCalidus(Vector2 position) : base(position)
        {
            FloatHeight = 3;
            LookTargetEnabled = true;
            IsPlayer = true;
            LookSpeed = 0.5f;
            Tag = Tags.Persistent;
        }
        public PlayerCalidus(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (EntityInScene) RemoveSelf();
            EntityInScene = true;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            EntityInScene = false;
        }
        public override void Update()
        {
            OrbSprite.Visible = HasHead;
            EyeSprite.Visible = HasEye;
            Arms[0].Visible = HasLeftArm;
            Arms[1].Visible = HasRightArm;

            float num3 = 90f;
            float num2 = 1f;
            float moveX = Input.MoveX.Value;
            float moveY = Input.MoveY.Value;
            if (Math.Abs(Speed.X) > num3 && Math.Sign(Speed.X) == moveX)
            {
                Speed.X = Calc.Approach(Speed.X, num3 * (float)moveX, 400f * num2 * Engine.DeltaTime);
            }
            else
            {
                Speed.X = Calc.Approach(Speed.X, num3 * (float)moveX, 500f * num2 * Engine.DeltaTime);
            }
            if (Math.Abs(Speed.Y) > num3 && Math.Sign(Speed.Y) == moveY)
            {
                Speed.Y = Calc.Approach(Speed.Y, num3 * (float)moveY, 200f * num2 * Engine.DeltaTime);
            }
            else
            {
                Speed.Y = Calc.Approach(Speed.Y, num3 * (float)moveY, 500f * num2 * Engine.DeltaTime);
            }
            MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
            MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            if (Scene.GetPlayer() is Player player)
            {
                player.MoveToX(Position.X + player.Width / 2, null);
                player.MoveToY(Position.Y + player.Height, null);
            }
            Vector2 move = new Vector2(moveX, moveY);
            if (move == Vector2.Zero)
            {
                LookDir = Looking.Center;
                LookTargetEnabled = false;
            }
            else
            {
                LookDir = Looking.Target;
                LookTargetEnabled = true;
                LookTarget = Center + Calc.SafeNormalize(move) * OrbSprite.Width / 4f;
            }

            base.Update();
        }
        private void OnCollideH(CollisionData data)
        {
            if (data.Hit != null && data.Hit.OnCollide != null)
            {
                data.Hit.OnCollide(data.Direction);
            }
            Speed.X = 0f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (Speed.Y < 0f)
            {
                int num3 = 4;

                if (Speed.X <= 0.01f)
                {
                    for (int j = 1; j <= num3; j++)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(-j, -1f)))
                        {
                            Position += new Vector2(-j, -1f);
                            return;
                        }
                    }
                }

                if (Speed.X >= -0.01f)
                {
                    for (int k = 1; k <= num3; k++)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(k, -1f)))
                        {
                            Position += new Vector2(k, -1f);
                            return;
                        }
                    }
                }
            }
            if (data.Hit != null && data.Hit.OnCollide != null)
            {
                data.Hit.OnCollide(data.Direction);
            }
            Speed.Y = 0f;
        }
        public static bool EntityInScene;
        public static void Unload()
        {
            EntityInScene = false;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
            On.Celeste.Player.Render -= Player_Render;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.StartDash -= Player_StartDash;
            On.Celeste.Player.WallJump -= Player_WallJump;
            On.Celeste.PlayerCollider.Check -= PlayerCollider_Check;
            On.Celeste.Dust.BurstFG -= Dust_BurstFG;
            On.Celeste.Dust.Burst_Vector2_float_int_ParticleType -= Dust_Burst_Vector2_float_int_ParticleType;
            On.Celeste.Dust.Burst_Vector2_float_int -= Dust_Burst_Vector2_float_int;
        }

        public static void Load()
        {
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
            On.Celeste.Player.Render += Player_Render;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.StartDash += Player_StartDash;
            On.Celeste.Player.WallJump += Player_WallJump;
            On.Celeste.PlayerCollider.Check += PlayerCollider_Check;
            On.Celeste.Dust.BurstFG += Dust_BurstFG;
            On.Celeste.Dust.Burst_Vector2_float_int_ParticleType += Dust_Burst_Vector2_float_int_ParticleType;
            On.Celeste.Dust.Burst_Vector2_float_int += Dust_Burst_Vector2_float_int;
        }


        private static void Dust_Burst_Vector2_float_int(On.Celeste.Dust.orig_Burst_Vector2_float_int orig, Vector2 position, float direction, int count)
        {
            if (EntityInScene) return;
            orig(position, direction, count);
        }

        private static void Dust_Burst_Vector2_float_int_ParticleType(On.Celeste.Dust.orig_Burst_Vector2_float_int_ParticleType orig, Vector2 position, float direction, int count, ParticleType particleType)
        {
            if (EntityInScene) return;
            orig(position, direction, count, particleType);
        }

        private static void Dust_BurstFG(On.Celeste.Dust.orig_BurstFG orig, Vector2 position, float direction, int count, float range, ParticleType particleType)
        {
            if (EntityInScene) return;
            orig(position, direction, count, range, particleType);
        }

        private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
        {
            if (EntityInScene) return;
            orig(self, dir);
        }
        private static int Player_StartDash(On.Celeste.Player.orig_StartDash orig, Player self)
        {
            if (EntityInScene)
            {
                if (self.StateMachine.State == 0)
                {
                    self.Speed -= self.LiftBoost;
                }
                return self.StateMachine.State;
            }
            return orig(self);
        }
        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            bool prev = self.Collidable;
            if (EntityInScene)
            {
                self.Collidable = false;
            }
            orig(self);
            self.Collidable = prev;
        }
        private static bool PlayerCollider_Check(On.Celeste.PlayerCollider.orig_Check orig, PlayerCollider self, Player player)
        {
            if (EntityInScene) return false;
            return orig(self, player);
        }
        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            if (EntityInScene) return;
            orig(self);
        }

        private static void Player_OnSpawn(Player obj)
        {
            if (EntityInScene)
            {

            }
        }
        private static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

    }
}