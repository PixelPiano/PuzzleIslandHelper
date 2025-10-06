using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{
    [CustomEntity("PuzzleIslandHelper/CalidusFollowTrigger")]
    [Tracked]
    public class CalidusFollowTrigger : Trigger
    {
        public enum TriggerModes
        {
            OnEnter,
            OnStay,
            OnLeave,
            OnLevelStart,
        }
        public TriggerModes TriggerMode;
        public string Flag;
        public bool Inverted;
        private bool state => (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag)) != Inverted;
        public CalidusFollowTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            TriggerMode = data.Enum<TriggerModes>("mode");
            Flag = data.Attr("flag");
            Inverted = data.Bool("inverted");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (state)
            {
                StartFollowing();
            }
            else
            {
                StopFollowing();
            }
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (state)
            {
                StartFollowing();
            }
            else
            {
                StopFollowing();
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (state)
            {
                StartFollowing();
            }
            else
            {
                StopFollowing();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (state)
            {
                StartFollowing();
            }
            else
            {
                StopFollowing();
            }
        }
        public void StartFollowing()
        {
            Scene.Tracker.GetEntity<Calidus>()?.StartFollowing();
        }
        public void StopFollowing()
        {
            Scene.Tracker.GetEntity<Calidus>()?.StopFollowing();
        }
    }
}
