using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PuzzleIslandSkinSetter")]
    [Tracked]
    public class PuzzleIslandSkinSetter : Entity
    {
        private EntityID id;
        private static bool state;
        public PuzzleIslandSkinSetter(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            Tag = Tags.Global;
            Tag |= Tags.TransitionUpdate;
            this.id = id;
        }

        internal static void Load()
        {
            Everest.Events.Level.OnTransitionTo += Transition;
        }
        internal static void Unload()
        {
            Everest.Events.Level.OnTransitionTo -= Transition;
        }
        private static void Transition(Level level, LevelData data, Vector2 dir)
        {
            state = level.Session.Level.Contains("digi");
            level.Session.SetFlag("PuzzleIslandSkinCheck", state);
            
            
        }
    }
}