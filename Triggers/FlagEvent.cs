using Celeste.Mod.Entities;
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
        private EntityID ID;

        public FlagEventTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
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
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
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
