using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/LabComputer")]
    [TrackedAs(typeof(Machine))]
    public class LabComputer : Machine
    {
        public const string ID = "LabComputer";
        public static bool Interacted
        {
            get
            {
                return (ID + "Interacted").GetFlag();
            }
            set
            {
                (ID + "Interacted").SetFlag();
            }
        }
        public LabComputer(EntityData data, Vector2 offset) : base(data.Position + offset, "objects/PuzzleIslandHelper/interface/keyboard", Color.Green)
        {
            UsesStartupMonitor = true;
            TalkEnabled = false;
        }
        public override IEnumerator OnBegin(Player player)
        {
            if (PianoModule.Session.RestoredPower)
            {
                SetSessionInterface();
                Interface.StartWithPreset("Default");
                yield return null;
            }
            else
            {
                if (Scene.Tracker.GetEntity<ComputerMonitor>() is var monitor)
                {
                    /*  if (PianoModule.Session.TimesUsedCapsuleWarp == 0)
                      {
                          SetSessionInterface();
                          Interface.StartWithPreset("Default");
                      }
                      else if (Interacted)
                      {*/
                    Interface.StartWhirring();
                    monitor.SmallLogo();
                    monitor.EndlessFlicker(0.4f);
                    yield return 1;
                    monitor.StopFlickering(false);
                    Interface.StopWhirring();
                    yield return 0.2f;
                    monitor.LowBattery();
                    monitor.Icon.Visible = true;
                    player.StateMachine.State = Player.StNormal;
                    //}
                }
            }
            Interacted = true;
        }
    }

}