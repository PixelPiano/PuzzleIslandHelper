using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/FlagEventTrigger")]
    [TrackedAs(typeof(EventTrigger))]
    public class FlagEventTrigger : EventTrigger
    {
        private readonly List<(string, bool)> RequiredFlags;
        private readonly List<(string, bool)> StartFlags;
        private readonly List<(string, bool)> InvertFlags;
        private readonly bool oncePerLevel;
        private readonly bool oncePerSession;
        private EntityID ID;
        public FlagEventTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
            RequiredFlags = PianoUtils.ParseFlagsFromString(data.Attr("requiredFlags"));
            StartFlags = PianoUtils.ParseFlagsFromString(data.Attr("flagsOnBegin"));
            InvertFlags = PianoUtils.ParseFlagsFromString(data.Attr("flagsToInvert"));
            oncePerLevel = data.Bool("oncePerLevel");
            oncePerSession = data.Bool("oncePerSession");
        }
        public override void OnEnter(Player player)
        {
            if (RequiredFlags.CheckAll())
            {
                if (!triggered)
                {
                    foreach (var item in StartFlags)
                    {
                        item.Item1.SetFlag(item.Item2);
                    }
                    foreach (var item in InvertFlags)
                    {
                        item.Item1.InvertFlag();
                    }
                }
                base.OnEnter(player);
                if (oncePerSession) SceneAs<Level>().Session.DoNotLoad.Add(ID);
                if (oncePerLevel) RemoveSelf();
            }
        }
    }
}
