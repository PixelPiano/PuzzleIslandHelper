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
        public LabComputer(EntityData data, Vector2 offset) : base(data.Position + offset, "objects/PuzzleIslandHelper/interface/keyboard", Color.Green)
        {
            UsesStartupMonitor = true;
            UsesFloppyLoader = true;
        }
        public override IEnumerator OnBegin(Player player)
        {
            if (Scene is not Level level) yield break;
            if (!PianoModule.Session.RestoredPower)
            {
                //DEBUG
                Interface.StartWithPreset("Default");
                yield break;
                //END DEBUG
                if (PianoModule.Session.TimesMetWithCalidus < 1)
                {
                    SetSessionInterface();
                    Interface.LoadModules(player.Scene);
                    YouHaveMail mail = new YouHaveMail(Interface, "mailTextA", "Important");
                    Scene.Add(mail);
                    Interface.LoadProgram("mail");
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
                    Interface.LoadModules(player.Scene);
                    YouHaveMail mail = new YouHaveMail(Interface, "mailTextB", "Power!");
                    Scene.Add(mail);
                    Interface.LoadProgram("mail");
                    yield return null;
                    Interface.Start();
                }
                else
                {
                    Interface.StartWithPreset("Default");
                }
                /*                else if (PianoModule.Session.CollectedDisks.Count == 0)
                                {
                                    yield return Interface.FakeStart();
                                }
                                else
                                {
                                    Interface.StartWithPreset(PianoModule.Session.CollectedDisks[0].Preset);
                                }*/
                yield return null;
            }
        }
    }

}