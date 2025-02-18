
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/EntityStateTrigger")]
    [Tracked]
    public class EntityStateTrigger : Trigger
    {
        public enum EntityTypes
        {
            DashBlock,
            ExitBlock,
            Bumper,
            Refill,
            Feather,
            Booster
        }
        public string RemoveFlag;
        public string Flag;
        public bool Inverted;
        public bool TiedToTarget;
        public EntityTypes TargetedType;
        public Entity Entity;
        public Vector2 Node;
        public bool Changed;
        public float prevFloat;
        public bool prevBool;
        public bool condition;
        public float delay;
        public bool hasNode;
        public bool state;
        private bool onLevelStart;
        private bool startState;
        private bool persistent;
        private EntityID id;
        private bool removedByForce;
        private bool onlyOnContact;
        private bool swapStatesOnChange;
        public EntityStateTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            RemoveFlag = data.Attr("removeFlag");
            this.id = id;
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
            onLevelStart = data.Bool("setFlagOnTransition");
            startState = data.Bool("startState");
            TargetedType = data.Enum<EntityTypes>("target");
            TiedToTarget = data.Bool("tiedToTarget");
            persistent = data.Bool("persistent");
            onlyOnContact = data.Bool("onlyOnContact");
            swapStatesOnChange = data.Bool("invertFlagOnContact");
            var nodes = data.NodesOffset(offset);
            if (nodes.Length > 0)
            {
                Node = nodes[0];
                hasNode = true;
            }
            else
            {
                hasNode = false;
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Entity != null && Entity.Collider != null)
            {
                Draw.Line(Center, Entity.Position, Color.Cyan);
                Draw.HollowRect(Entity.Collider, Color.Cyan);
            }
        }
        public Entity GetFirstOfEntities<T>(Scene scene, Vector2 at) where T : Entity
        {
            foreach (Entity entity in GetEntities<T>(scene))
            {
                if (entity.Collider != null && entity.Collidable && CollideCheck(entity, at))
                {
                    return entity;
                }
            }
            return null;
        }
        public List<Entity> GetEntities<T>(Scene scene) where T : Entity
        {
            List<Entity> list = [];
            foreach (Entity e in scene.Entities)
            {
                if (e is T)
                {
                    list.Add(e);
                }
            }
            return list;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (removedByForce && persistent && !(scene as Level).Session.DoNotLoad.Contains(id))
            {
                (scene as Level).Session.DoNotLoad.Add(id);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (onLevelStart)
            {
                SetState(startState);
            }
            Collider prev = Collider;
            Vector2 position = Position;
            if (hasNode)
            {
                Collider = new Hitbox(1, 1);
                position = Node;
            }
            Entity = TargetedType switch
            {
                EntityTypes.DashBlock => CollideFirst<DashBlock>(position),
                EntityTypes.ExitBlock => CollideFirst<ExitBlock>(position),
                EntityTypes.Feather => CollideFirst<FlyFeather>(position),
                EntityTypes.Bumper => GetFirstOfEntities<Bumper>(scene, position),
                EntityTypes.Refill => GetFirstOfEntities<Refill>(scene, position),
                EntityTypes.Booster => GetFirstOfEntities<Booster>(scene, position),
                _ => null
            };
            Collider = prev;
        }
        public override void Update()
        {
            base.Update();
            state = string.IsNullOrEmpty(Flag) ? !Inverted : SceneAs<Level>().Session.GetFlag(Flag);
            if (Entity != null)
            {
                if (Entity.Scene == null && TiedToTarget)
                {
                    removedByForce = true;
                    RemoveSelf();
                    return;
                }
                if (Entity.Active)
                {
                    switch (TargetedType)
                    {
                        case EntityTypes.ExitBlock:
                            float alpha = (Entity as ExitBlock).tiles.Alpha;
                            if (prevFloat < alpha)
                            {
                                SetState(false);
                            }
                            else if (prevFloat > alpha)
                            {
                                SetState(true);
                            }
                            prevFloat = alpha;
                            break;
                        case EntityTypes.Bumper:
                            if (Entity is Bumper bumper && !onlyOnContact && condition && !bumper.fireMode && bumper.respawnTimer <= 0f)
                            {
                                SetState(true);
                                condition = false;
                            }
                            break;
                        case EntityTypes.Feather:
                            if (Entity is FlyFeather feather && !onlyOnContact && condition && feather.respawnTimer <= 0f)
                            {
                                SetState(true);
                                condition = false;
                            }
                            break;
                        case EntityTypes.Booster:
                            if (Entity is Booster booster && !onlyOnContact && condition && booster.respawnTimer <= 0f)
                            {
                                SetState(true);
                                condition = false;
                            }
                            break;
                    }
                }
            }
        }
        public void InvertState()
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                bool state = SceneAs<Level>().Session.GetFlag(Flag);
                SceneAs<Level>().Session.SetFlag(Flag, !state);
            }
        }
        public void SetState(bool setTo = true)
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                if (Inverted)
                {
                    setTo = !setTo;
                }
                SceneAs<Level>().Session.SetFlag(Flag, setTo);
            }
        }

        [OnLoad]
        public static void Load()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += DashBlock_Break_Vector2_Vector2_bool_bool;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool += DashBlock_Break_Vector2_Vector2_bool;
            On.Celeste.Refill.OnPlayer += Refill_OnPlayer;
            On.Celeste.Refill.Respawn += Refill_Respawn;
            On.Celeste.Bumper.OnPlayer += Bumper_OnPlayer;
            On.Celeste.FlyFeather.OnPlayer += FlyFeather_OnPlayer;
            On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
        }


        [OnUnload]
        public static void Unload()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool -= DashBlock_Break_Vector2_Vector2_bool_bool;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool -= DashBlock_Break_Vector2_Vector2_bool;
            On.Celeste.Refill.OnPlayer -= Refill_OnPlayer;
            On.Celeste.Refill.Respawn -= Refill_Respawn;
            On.Celeste.Bumper.OnPlayer -= Bumper_OnPlayer;
            On.Celeste.FlyFeather.OnPlayer -= FlyFeather_OnPlayer;
            On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
        }
        public void TriggerChange(bool activateCondition = true)
        {
            if (swapStatesOnChange)
            {
                InvertState();
            }
            else
            {
                SetState(false);
            }
            if (!onlyOnContact && activateCondition)
            {
                condition = true;
            }
        }
        private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
        {
            orig(self);
            foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
            {
                if (trigger.Entity == self)
                {
                    trigger.SetState(false);
                    trigger.condition = true;
                }
            }
        }
        private static void FlyFeather_OnPlayer(On.Celeste.FlyFeather.orig_OnPlayer orig, FlyFeather self, Player player)
        {
            orig(self, player);
            if (!(self.shielded && !player.DashAttacking))
            {
                foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
                {
                    if (trigger.Entity == self)
                    {
                        trigger.TriggerChange();
                    }
                }
            }
        }
        private static void Bumper_OnPlayer(On.Celeste.Bumper.orig_OnPlayer orig, Bumper self, Player player)
        {
            if (!self.fireMode && self.respawnTimer <= 0f)
            {
                foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
                {
                    if (trigger.Entity == self)
                    {
                        trigger.TriggerChange();
                    }
                }
            }
            orig(self, player);
        }
        private static void Refill_Respawn(On.Celeste.Refill.orig_Respawn orig, Refill self)
        {
            foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
            {
                if (trigger.Entity == self && trigger.condition)
                {
                    trigger.TriggerChange();
                    trigger.condition = false;
                }
            }
            orig(self);
        }
        private static void Refill_OnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player)
        {
            int num = player.MaxDashes;
            if (self.twoDashes)
            {
                num = 2;
            }

            if (player.Dashes < num || player.Stamina < 20f)
            {
                foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
                {
                    if (trigger.Entity == self)
                    {
                        trigger.TriggerChange();
                    }
                }
            }
            orig(self, player);
        }
        private static void DashBlock_Break_Vector2_Vector2_bool_bool(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound, bool playDebrisSound)
        {
            foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
            {
                if (self == trigger.Entity)
                {
                    trigger.SetState(true);
                }
            }
            orig(self, from, direction, playSound, playDebrisSound);
        }
        private static void DashBlock_Break_Vector2_Vector2_bool(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound)
        {
            foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
            {
                if (self == trigger.Entity)
                {
                    trigger.SetState(true);
                }
            }
            orig(self, from, direction, playSound);
        }
    }
}
