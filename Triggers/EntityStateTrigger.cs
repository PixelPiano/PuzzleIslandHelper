
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/EntityStateTrigger")]
    [Tracked]
    public class EntityStateTrigger : Trigger
    {
        public enum FlagModes
        {
            OnChange,
            EveryFrame,
            OnlyOnce
        }
        public enum EntityTypes
        {
            DashBlock,
            ExitBlock,
            Bumper,
            Refill,
            Feather,
            Booster
        }
        public string Flag;
        public bool Inverted;
        public bool TiedToTarget;
        public EntityTypes TargetedType;
        public FlagModes FlagMode;
        public Entity Entity;
        public Vector2 Node;
        private bool state;
        public bool Changed;
        public bool CanChange => !(FlagMode == FlagModes.OnlyOnce && Changed);
        public float prevFloat;
        public bool prevBool;
        public EntityStateTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
            TargetedType = data.Enum<EntityTypes>("target");
            FlagMode = data.Enum<FlagModes>("flagMode");
            TiedToTarget = data.Bool("tiedToTarget");
            Node = data.NodesOffset(offset)[0];
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += DashBlock_Break_Vector2_Vector2_bool_bool;
        }

        private static void DashBlock_Break_Vector2_Vector2_bool_bool(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound, bool playDebrisSound)
        {
            foreach (EntityStateTrigger trigger in self.Scene.Tracker.GetEntities<EntityStateTrigger>())
            {
                if (trigger.TargetedType == EntityTypes.DashBlock && self == trigger.Entity)
                {
                    trigger.SetState(true);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (Entity != null)
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
                    case EntityTypes.Bumper: //hook
                        break;
                    case EntityTypes.Refill: //hook
                        break;
                    case EntityTypes.Feather: //hook
                        break;
                    case EntityTypes.Booster: //hook
                        break;
                }
            }
        }
        public void SetState(bool setOn = true)
        {
            if (CanChange && !string.IsNullOrEmpty(Flag))
            {
                SceneAs<Level>().Session.SetFlag(Flag, setOn != Inverted);
            }
        }
    }
}
