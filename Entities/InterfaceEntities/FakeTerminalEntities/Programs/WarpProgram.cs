using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs
{
    [TerminalProgram("WarpProgram")]
    public class WarpProgram : TerminalProgram
    {
        public string SelectedID
        {
            get
            {
                return Parent.TargetID;
            }
            set
            {
                Parent.TargetID = value;
            }
        }

        public WarpCapsule Parent;
        public WarpProgram(WarpCapsule parent, FakeTerminal terminal) : base(terminal, "Please enter the destination node ID.")
        {
            Parent = parent;
            AddCommand("id", null, OnID, "destination", "target", "travel");
        }

        public override bool Continue()
        {
            return !Closed;
        }
        public static bool ValidateID(string id, WarpCapsule parent)
        {
            return !string.IsNullOrEmpty(id) && parent.WarpID != id && PianoMapDataProcessor.WarpLinks.ContainsKey(id);
        }
        public IEnumerator OnID(string input)
        {
            input = input.Trim(' ');
            if (string.IsNullOrEmpty(input))
            {
                SelectedID = null;
                yield return Error("Error: No Link ID provided.");
            }
            else if (SelectedID == input)
            {
                yield return Error("Error: Parent warp chamber already using Link ID.");
            }
            else if (PianoMapDataProcessor.WarpLinks.ContainsKey(input))
            {
                if (input.Equals(Parent.WarpID, StringComparison.OrdinalIgnoreCase))
                {
                    yield return Error("Error: Link ID cannot be identical {n}to the parent warp chamber's Link ID.");
                    yield break;
                }
                //Parent.DoorEvent(wasValid ? WarpCapsule.DoorRoutines.CloseOpen : WarpCapsule.DoorRoutines.Open, 0.7f);
                SelectedID = input;
                Parent.SetWarpTarget(input);
                AddText("Link established. Opening warp chamber.", Color.Lime);
                AddText("To exit the console, use the \"exit\" command.");
            }
            else
            {
                SelectedID = null;
                yield return Error("Error: Link \"" + input + "\" origin could not be traced.");
            }
        }
        public override IEnumerator Help(string input)
        {
            AddText("Merry Christmas ya filthy animal");
            yield return null;
        }

        public override void OnClose()
        {

        }
    }

}