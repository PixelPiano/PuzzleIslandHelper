using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Components.CustomTalkComponent;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/FlagEventTrigger")]
    [TrackedAs(typeof(EventTrigger))]
    public class FlagEventTrigger : EventTrigger
    {
        private FlagList Required;
        private FlagList OnStart;
        private FlagList ToInvert;
        private readonly bool oncePerLevel;
        private readonly bool oncePerSession;
        private EntityID ID;
        private bool talker;
        public TalkComponent Talk;
        public FlagEventTrigger(EntityData data, Vector2 offset, EntityID id) : base(data, offset)
        {
            ID = id;
            Required = data.FlagList("requiredFlags");
            OnStart = data.FlagList("flagsOnBegin");
            ToInvert = data.FlagList("flagsToInvert");
            oncePerLevel = data.Bool("oncePerLevel");
            oncePerSession = data.Bool("oncePerSession");
            talker = data.Bool("talker");
            Add(Talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Vector2.UnitX * Collider.Width / 2, TryActivate));
            Talk.PlayerMustBeFacing = false;
        }
        public override void Update()
        {
            base.Update(); 
            Talk.Enabled = talker && CanActivate();
        }
        public bool CanActivate()
        {
            if (oncePerLevel && triggered) return false;
            if (oncePerSession && (SceneAs<Level>().Session.DoNotLoad.Contains(ID) || triggered)) return false;
            return Required;
        }
        public override void OnEnter(Player player)
        {
            if (talker) return;
            TryActivate(player);
        }
        public void TryActivate(Player player)
        {
            if (CanActivate())
            {
                foreach(FlagData data in OnStart)
                {
                    data.Set(!data.Inverted);
                }
                foreach(FlagData data in ToInvert)
                {
                    data.Invert();
                }
                base.OnEnter(player);
                if (oncePerSession) SceneAs<Level>().Session.DoNotLoad.Add(ID);
                triggered = oncePerLevel;
            }
        }
    }
}
