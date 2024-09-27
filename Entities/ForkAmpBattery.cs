using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ForkAmpBattery")]
    [Tracked]
    public class ForkAmpBattery : HoldableEntity
    {
        public string FlagOnFinish;
        public bool IsLastHeld => this == PianoModule.Session.LastHeld;
        public int ID => EntityID.ID;
        public ForkAmpBattery(EntityData data, Vector2 offset, EntityID entityID) :
            base(data.Position + offset, entityID, false, data.Bool("isLeader"), data.Attr("subId"),
                data.Attr("continuityID"), "objects/PuzzleIslandHelper/forkAmp/")
        {
            FlagOnFinish = data.Attr("flagOnFinish");
            Position.Y += Sprite.Height / 2;
            OrigRoom = data.Level.Name;
        }
        public override void OnPickup()
        {
            base.OnPickup();
            PianoModule.Session.LastHeld = this;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (PianoModule.Session.LastHeld == this)
            {
                PianoModule.Session.LastHeld = null;
            }
        }
        public IEnumerator Approach(float x)
        {
            while (Center.X != x)
            {
                CenterX = Calc.Approach(Center.X, x, 20f * Engine.DeltaTime);
                yield return null;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(FlagOnFinish) && (scene as Level).Session.GetFlag(FlagOnFinish))
            {
                RemoveSelf();
            }

        }
    }
    [CustomEntity("PuzzleIslandHelper/BatteryRespawn")]
    [Tracked]
    public class BatteryRespawn : Entity
    {
        public static EntityID RespawnID;
        public static bool RespawnNearPlayer;
        public static ForkAmpBattery Battery;
        public EntityID EntityID;
        public BatteryRespawn(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            EntityID = id;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (EntityID.Equals(RespawnID))
            {
                scene.Add(Battery);
                Battery.Position = Position;
            }

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            RespawnNearPlayer = false;
        }

        private static void Player_OnDie(Player obj)
        {
            Battery = PianoModule.Session.LastHeld;
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Player.OnDie += Player_OnDie;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }

        private static void Player_OnSpawn(Player obj)
        {
            if(Battery is null) return;
            float closest = int.MaxValue;
            BatteryRespawn toSet = null;
            foreach (BatteryRespawn b in obj.Scene.Tracker.GetEntities<BatteryRespawn>())
            {
                float dist = Vector2.DistanceSquared(b.Position, obj.Position);
                if (dist < closest)
                {
                    closest = dist;
                    toSet = b;
                }
            }
            foreach(ForkAmpBattery battery in obj.Scene.Tracker.GetEntities<ForkAmpBattery>())
            {
                if(battery.EntityID.Equals(Battery.EntityID) && !battery.IsLastHeld)
                {
                    battery.RemoveSelf();
                }
            }
            if (toSet != null && Battery != null)
            {
                obj.Scene.Add(Battery);
                Battery.Position = toSet.Position;
            }
        }
        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Player.OnDie -= Player_OnDie;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }
    }
}