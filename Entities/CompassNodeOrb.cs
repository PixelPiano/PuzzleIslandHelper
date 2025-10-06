using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CompassNodeOrb")]
    [Tracked]
    public class CompassNodeOrb : HoldableEntity
    {
        public string OrigLevel;
        public Vector2 OrigPosition;
        private bool InHolder => Holder != null;
        public CompassNode Holder;
        private Collider DetectHitbox;
        public bool CanHold;
        [Command("giveorb", "gives the player an orb")]
        public static void GiveOrb()
        {
            if (Engine.Scene.GetPlayer() is Player player)
            {
                player.Scene.Add(new CompassNodeOrb(player.Position, new EntityID(Guid.NewGuid().ToString(), 0), true));
            }
        }
        public CompassNodeOrb(EntityData data, Vector2 offset, EntityID entityID) : this(data.Position + offset, entityID, true)
        {
        }

        public CompassNodeOrb(Vector2 position, EntityID id, bool canHold) : base(position, id, "objects/PuzzleIslandHelper/", "wallbuttonOrb")
        {
            OrigPosition = Position;
            CanHold = canHold;
            Depth = 1;
            AddTag(Tags.Persistent);
            ModifyPersistence = false;
            Hold.SlowRun = false;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            (scene as Level).Session.DoNotLoad.Remove(EntityID);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            OrigLevel = (scene as Level).Session.Level;
            (scene as Level).Session.DoNotLoad.Add(EntityID);
            DetectHitbox = new Hitbox(Width - 4, Height - 4, -Sprite.Width * Justify.X + 2, -Sprite.Height * Justify.Y + 2);
        }
        #region Finished Methods
        public override void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f && TimePassed > 1 && !InHolder)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {
                    //todo: replace with new sound
                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", 0f);
                }
            }

            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }
        public override void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (!InHolder)
            {
                //todo: replace with new sound
                Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            }
            Speed.X *= -0.4f;
        }
        public override void OnPickup()
        {
            Sprite.Play("idle");
            Holder?.OnOrbTaken();
            Holder = null;

            base.OnPickup();
        }
        #endregion
        public override void Render()
        {
            if (Holder == null || !Holder.OrbAtCenter)
            {
                DrawOrb();
            }
        }
        public void DrawOrb()
        {
            Sprite.DrawSimpleOutline();
            base.Render();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider prev = Collider;
            Collider = DetectHitbox;
            Draw.HollowRect(Collider, Color.Lime);
            Collider = prev;
        }
        public override void Update()
        {
            if (!CanHold)
            {
                Hold.cannotHoldTimer = Engine.DeltaTime;
                Hold.Active = false;
                return;
            }
            else
            {
                Hold.Active = true;
            }
            if (InHolder)
            {
                Hold.gravityTimer += Engine.DeltaTime;
                Collider = DetectHitbox;
                Hold.CheckAgainstColliders();
                Hold.Update();
                if (!Hold.IsHeld)
                {
                    Collider = IdleHitbox;
                }
            }
            else
            {
                Sprite.Color = Color.Lerp(Sprite.Color, Color.White, 5 * Engine.DeltaTime);
                base.Update();
            }
        }
    }
}