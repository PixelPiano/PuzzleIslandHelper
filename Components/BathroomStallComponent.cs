using System;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class BathroomStallComponent : Component
    {
        public Action OnBlocked;
        public Action OnUnblocked;
        public Collider Collider;
        public bool Blocked;
        public bool wasBlocked;
        public int BlockedBy;
        private bool updatedOnce;
        public BathroomStallComponent(Collider collider = null, Action onBlocked = null, Action onUnblocked = null) : base(true, false)
        {
            OnBlocked = onBlocked;
            OnUnblocked = onUnblocked;
            Collider = collider;
        }
        public override void EntityAwake()
        {
            base.EntityAwake();
            UpdateComponent();
        }
        public override void Update()
        {
            base.Update();
            if (!updatedOnce)
            {
                UpdateComponent();
            }
        }
        public bool Check()
        {
            Collider prev = Entity.Collider;
            if (Collider != null)
            {
                Entity.Collider = Collider;
            }
            foreach (BathroomStall stall in Scene.Tracker.GetEntities<BathroomStall>())
            {
                if (stall != Entity && Collide.Check(stall, Entity))
                {
                    Entity.Collider = prev;
                    return true;
                }
            }
            Entity.Collider = prev;
            return false;
        }
        public int Count()
        {
            int count = 0;
            Collider prev = Entity.Collider;
            if (Collider != null)
            {
                Entity.Collider = Collider;
            }
            foreach (BathroomStall stall in Scene.Tracker.GetEntities<BathroomStall>())
            {
                if (stall != Entity && Collide.Check(stall, Entity))
                {
                    count++;
                }
            }
            Entity.Collider = prev;
            return count;
        }
        public void UpdateComponent()
        {
            if (!Active) return;
            updatedOnce = true;
            if (Entity != null)
            {
                int count = Count();
                if (BlockedBy <= 0 && count > 0)
                {
                    OnBlocked?.Invoke();
                }
                if (count <= 0 && BlockedBy > 0)
                {
                    OnUnblocked?.Invoke();
                }
                BlockedBy = count;
            }

        }

    }

}

