using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WARP;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs
{
    //BETA - NOW OBSOLETE
    [CustomProgram("WarpProgram")]
    public class WarpProgram : TerminalProgram
    {
        public int TimesUsed => PianoModule.Session.TimesUsedCapsuleWarp;
        public string SelectedID
        {
            get
            {
                return Parent.RoomName;
            }
            set
            {
                Parent.RoomName = value;
            }
        }
        //public WarpCapsuleData SelectedCapsule => WarpCapsule.GetCapsuleData(SelectedID);
        public static Dictionary<string, WarpCapsuleData> Data;
        public WarpCapsule2 Parent;
        public WarpProgram(WarpCapsule2 parent, FakeTerminal terminal) : base(terminal, "Please enter the destination node ID.")
        {
            Parent = parent;
            AddCommand("id", null, OnID, "destination", "target", "travel");
            AddCommand("list", null, OnList);
            AddCommand("debug", OnDebug);
        }
        public void OnDebug(string input)
        {
            bool added = false;
            foreach (KeyValuePair<string, WarpCapsuleData> pair in Data)
            {
                string text = pair.Key;
/*                if (!string.IsNullOrEmpty(pair.Value.Password))
                {
                    text += ", " + pair.Value.Password;
                }
                else
                {
                    text += " {NO PASSWORD PROVIDED}";
                }*/
                AddText(text, Color.Gray);
                added = true;
            }
            if (!added)
            {
                AddText("WarpLinks is empty (???)", Color.Red);
            }
        }
        public IEnumerator OnList(string input)
        {
            AddText("Verifying unlocked capsules list", Color.Orange);
            yield return 1;
            AddText("Red IDs indicate a password is required.", Color.Red);
            MultiGroup[] groups = new MultiGroup[5];
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = new MultiGroup(Terminal, "", Color.Yellow);
            }
            int index = 0;
            int columns = 1;
            foreach (KeyValuePair<string, bool> pair in PianoModule.Session.LoggedCapsules)
            {
                groups[index].AddLine(new CapsuleLine(Terminal, this, pair.Key));
                index++;
                if (index >= groups.Length)
                {
                    index = 0;
                    columns++;
                }
            }
            if (PianoModule.Session.LoggedCapsules.Count == 0)
            {
                Error("No capsules found in list.");
            }
            else
            {
                for (int i = 1; i < groups.Length; i++)
                {
                    groups[0].LinkToChild(groups[i]);
                }
                foreach (MultiGroup g in groups)
                {
                    int linecount = g.Lines.Count;
                    for (int i = linecount; i < columns; i++)
                    {
                        g.AddLine(new EmptyLine(Terminal));
                    }
                    AddGroup(g);
                }
            }
            yield return null;
        }

        public IEnumerator OnID(string input)
        {
            //keys are no spaces and lowercase
            //passwords are no spaces and lowercase

            //store the name as it is, compare it as simple
            //store the password as it is, compare it as simple
            string[] array = input.Split(' ');
            string name = array[0].Trim(' ').ToLower();
            if (string.IsNullOrEmpty(name))
            {
                SelectedID = null;
                Error("Error: No Link ID provided.");
                yield break;
            }
            if (!string.IsNullOrEmpty(SelectedID) && SelectedID.Equals(name))
            {
                Error("Error: Parent warp chamber already using Link ID.");
                yield break;
            }
            if (!Data.ContainsKey(name))
            {
                SelectedID = null;
                Error("Error: Origin of Link ID could not be traced.");
                yield break;
            }
/*            if (name == Parent.WarpID)
            {
                Error("Error: Link ID cannot be identical {n}to the parent warp chamber's Link ID.");
                yield break;
            }*/
            WarpCapsuleData data = Data[name];
/*            if (!string.IsNullOrWhiteSpace(data.Password))
            {
                string dataPass = data.Password.Replace(" ", "").ToLower();
                string password = "";
                for (int i = 1; i < array.Length; i++)
                {
                    password += array[i].ToLower();
                }
                password = password.Replace(" ", "");
                bool empty = string.IsNullOrEmpty(password);
                bool invalid = dataPass != password;
                if (empty || invalid)
                {
                    Error(empty ? "Error: Valid Link ID but no password provided." : "Error: Password is invalid.");
                    if (!PianoModule.Session.LoggedCapsules.ContainsKey(data.Name))
                    {
                        PianoModule.Session.LoggedCapsules.Add(data.Name, false);
                        AddText("Capsule id \"" + data.Name + "\" registered to list.", Color.Gray);
                    }
                    yield break;
                }
            }*/
            SetTarget(name, data.Name);
            yield return 0.7f;
            AddText("To exit the console, use the \"exit\" command.");
        }
        public override bool Continue()
        {
            return !Closed;
        }
        public static bool ValidateID(string id, WarpCapsule2 parent)
        {
            return !string.IsNullOrEmpty(id) && /*parent.WarpID != id &&*/ Data.ContainsKey(id);
        }
        public override void AfterWelcome()
        {
            AddText("Use the \"list\" command to view all{n}registered capsules.", Color.Yellow);
        }
        public void SetTarget(string id, string realName = null)
        {
            Parent.RoomName = id;
            SelectedID = id;
            AddText("Link established. Opening warp chamber.", Color.Lime);
            if (realName != null && !PianoModule.Session.LoggedCapsules.ContainsKey(realName))
            {
                PianoModule.Session.LoggedCapsules.Add(realName, true);
                AddText("Capsule id \"" + realName + "\" registered to memory.", Color.Gray);
            }
            else
            {
                PianoModule.Session.LoggedCapsules[realName] = true;
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