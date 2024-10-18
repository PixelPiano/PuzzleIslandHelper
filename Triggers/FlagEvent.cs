using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/FlagEventTrigger")]
    [TrackedAs(typeof(EventTrigger))]
    public class FlagEventTrigger : EventTrigger
    {
        private string[] requiredFlags;
        private bool[] requiredFlagStates;
        private string flagOnStart;
        private bool flagOnStartState;

        public FlagEventTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            PianoUtils.ParseFlagsFromString(data.Attr("requiredFlags"), out requiredFlags, out requiredFlagStates);
            flagOnStart = data.Attr("flagOnBegin");
            flagOnStartState = data.Bool("flagOnBeginState");
        }
        public override void OnEnter(Player player)
        {
            if (CheckRequiredFlags())
            {
                if (!triggered && !string.IsNullOrEmpty(flagOnStart))
                {
                    SceneAs<Level>().Session.SetFlag(flagOnStart, flagOnStartState);
                }
                base.OnEnter(player);
            }
        }
        public bool CheckRequiredFlags()
        {
            Level level = Scene as Level;

            if (arrayValid(requiredFlags) && arrayValid(requiredFlagStates))
            {
                for (int i = 0; i < requiredFlags.Length && i < requiredFlagStates.Length; i++)
                {
                    if (level.Session.GetFlag(requiredFlags[i]) != requiredFlagStates[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }
        private bool arrayValid<T>(T[] array)
        {
            return array != null && array.Length > 0;
        }
    }
}
