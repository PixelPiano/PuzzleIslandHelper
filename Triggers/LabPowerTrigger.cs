using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    public enum TriggerMode
    {
        OnEnter,
        OnLeave,
        OnStay,
        OnLevelStart,
        OnUpdate,
        OnRemoved
    }
    [CustomEntity("PuzzleIslandHelper/LabPowerTrigger")]
    [Tracked]
    public class LabPowerTrigger : Trigger
    {
        public LabPowerState State;
        public TriggerMode TriggerMode;
        public FlagList Flag;
        public LabPowerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            TriggerMode = data.Enum<TriggerMode>("mode");
            State = data.Enum<LabPowerState>("state");
            Flag = data.FlagList("flag");
            Tag |= Tags.TransitionUpdate;
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (TriggerMode == TriggerMode.OnEnter && Flag)
            {
                PianoModule.Session.PowerState = State;
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (TriggerMode == TriggerMode.OnLeave && Flag)
            {
                PianoModule.Session.PowerState = State;
            }
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (TriggerMode == TriggerMode.OnStay && Flag)
            {
                PianoModule.Session.PowerState = State;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((TriggerMode == TriggerMode.OnLevelStart || TriggerMode == TriggerMode.OnUpdate) && Flag)
            {
                PianoModule.Session.PowerState = State;
            }
        }
        public override void Update()
        {
            base.Update();
            if (TriggerMode == TriggerMode.OnUpdate && Flag)
            {
                PianoModule.Session.PowerState = State;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (TriggerMode == TriggerMode.OnRemoved && Flag)
            {
                PianoModule.Session.PowerState = State;
            }
        }
    }
}
