using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Entities.PlayerCalidus;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/CalidusUpgradeTrigger")]
    public class CalidusUpgradeTrigger : Trigger
    {
        public Upgrades Upgrade;
        public bool Instant;
        public bool OncePerSession;
        public EntityID ID;
        public CalidusUpgradeTrigger(EntityData data, Vector2 offset, EntityID id)
    : base(data, offset)
        {
            Tag |= Tags.TransitionUpdate;
            ID = id;
            Instant = data.Bool("skipCutscene");
            Upgrade = data.Enum<Upgrades>("upgrade");
            OncePerSession = data.Bool("oncePerSession");
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Scene.Add(new CalidusUpgradeCutscene(Upgrade,Instant));
            if (OncePerSession)
            {
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
            }
            RemoveSelf();
        }


    }
}
