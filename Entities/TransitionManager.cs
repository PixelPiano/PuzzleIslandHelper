using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TransitionManager")]
    public class TransitionManager : Entity
    {
        private Type Transition;
        private string roomName;
        public static bool Finished;
        public bool Started;
        public enum Type
        {
            BeamMeUp,
            Fold,
            Elevator,
            Gremlins
        }
        public TransitionManager(Type type, string room)
        : base(Vector2.Zero)
        {
            Finished = false;
            Transition = type;
            roomName = room;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            AddTransition(scene);
        }
        public void AddTransition(Scene scene)
        {
            switch (Transition)
            {
                case Type.BeamMeUp: (scene as Level).Add(new BeamMeUp(roomName, true)); break;
                //case Type.Elevator: (scene as Level).Add(new ElevatorTransition(roomName)); break;
                //case Type.Gremlins: (scene as Level).Add(new BeamMeUp(roomName, true)); break;
                //case Type.Fold: (scene as Level).Add(new FoldTransition(roomName)); break;
            }
            Started = true;
        }
    }
}
