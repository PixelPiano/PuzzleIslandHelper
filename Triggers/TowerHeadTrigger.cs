using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/TowerHeadTrigger")]
    [Tracked]
    public class TowerHeadTrigger : Trigger
    {
        public readonly bool IsTalk;
        private readonly DotX3 Talk;
        private List<Entity> TowerHeads = [];
        public TowerHead Target;
        private Vector2 node;
        private bool setState;
        public TowerHeadTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            IsTalk = data.Bool("isTalk");
            setState = data.Bool("setState");
            Talk = new DotX3(Collider, Interact);
            Talk.Enabled = IsTalk;
            Talk.PlayerMustBeFacing = false;
            var nodes = data.NodesOffset(offset);
            if (nodes.Length > 0)
            {
                node = nodes[0];
            }
            Add(Talk);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collider c = Collider;
            Collider = new Hitbox(1, 1);
            Target = CollideFirst<TowerHead>(node);
            Collider = c;
            if (Target == null)
            {
                RemoveSelf();
            }
        }
        public void Interact(Player player)
        {
            TowerHead target = CollideFirst<TowerHead>();
            if (target != null)
            {
                Input.Dash.ConsumePress();
                Talk.Enabled = false;
                Alarm.Set(this, 0.7f, delegate { Talk.Enabled = true; });
                target.PlayerInside = !target.PlayerInside;
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (!IsTalk)
            {
                Collider c = Collider;
                Collider = new Hitbox(1, 1);
                TowerHead target = CollideFirst<TowerHead>(node);
                Collider = c;

                if(target != null)
                {
                    target.PlayerInside = setState;
                }
            }
        }
    }
}
