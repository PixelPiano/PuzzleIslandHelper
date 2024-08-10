using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/LabComputer")]
    [TrackedAs(typeof(InterfaceMachine))]
    public class LabComputer : InterfaceMachine
    {
        public LabComputer(EntityData data, Vector2 offset) : base(data.Position + offset, "objects/PuzzleIslandHelper/interface/keyboard", Color.Green)
        {
            UsesStartupMonitor = true;
            UsesFloppyLoader = true;
        }
        public override IEnumerator OnBegin(Player player, Level level)
        {
            if (!PianoModule.Session.RestoredPower)
            {
                if (PianoModule.Session.TimesMetWithCalidus < 1)
                {
                    SetSessionInterface();
                    Interface.LoadModules(level);
                    YouHaveMail mail = new YouHaveMail(Interface, "mailTextA", "Important");
                    Scene.Add(mail);
                    Interface.AddProgram("mail");
                    yield return null;
                    Interface.Start();
                }
                else
                {
                    yield return Textbox.Say("interfaceNoPower");
                    player.StateMachine.State = Player.StNormal;
                }
            }
            else
            {
                SetSessionInterface();
                if (PianoModule.Session.TimesMetWithCalidus < 2)
                {
                    Interface.LoadModules(level);
                    YouHaveMail mail = new YouHaveMail(Interface, "mailTextB", "Power!");
                    Scene.Add(mail);
                    Interface.AddProgram("mail");
                    yield return null;
                    Interface.Start();
                }
                else if (PianoModule.Session.CollectedDisks.Count == 0)
                {
                    yield return Interface.FakeStart();
                }
                else
                {
                    Interface.StartPreset(PianoModule.Session.CollectedDisks[0].Preset);
                }
                yield return null;
            }
        }
    }

}