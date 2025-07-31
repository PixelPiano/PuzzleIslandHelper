using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{

    [CustomEntity("PuzzleIslandHelper/WarpCapsule")]
    [TrackedAs(typeof(WarpCapsule))]
    [Tracked]
    public class WarpCapsuleAlpha : WarpCapsule
    {
        public bool IsFirstTime => WARPData.ObtainedRunes.Count < 1 && PianoModule.Session.TimesUsedCapsuleWarp < 1 && Marker.TryFind("isStartingWarpRoom", out _);
        public bool ReadyForBeam;
        public Image ShineTex;
        private Entity Shine;
        public InputMachine InputMachine;
        public WarpData RuneData => WARPData.GetWarpData(OwnWarpRune);
        public WarpData TargetData => WARPData.GetWarpData(TargetWarpRune);
        public string RuneString
        {
            get
            {
                if (OwnWarpRune == null) return "Rune is null";
                string tostring = OwnWarpRune.ToString();
                if (string.IsNullOrEmpty(tostring)) return "Rune is null";
                else return tostring;
            }
        }
        public WarpCapsuleAlpha(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, id, data.Flag("disableFlag", "invertFlag"), data.Attr("warpID"), null, true, true)
        {
            InputMachine = new InputMachine(this, data.NodesOffset(offset)[0]);
            ShineTex = new Image(GFX.Game[Path + "shine"]);
            ShineTex.Color = Color.White * 0;
            string warpid = data.Attr("warpID");
            if (!string.IsNullOrEmpty(warpid) && !string.IsNullOrEmpty(data.Attr("rune")))
            {
                OwnWarpRune = new(warpid, data.Attr("rune"));
            }
        }
        public override WarpData RetrieveWarpData(CapsuleList list)
        {
            return list.GetDataFromRune(OwnWarpRune);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Shine = new Entity(Position) { ShineTex };
            Shine.Depth = Floor.Depth - 1;
            scene.Add(Shine, InputMachine);
            if (PianoModule.Session.PersistentWarpLinks.TryGetValue(ID, out string value))
            {
                TargetID = value;
            }
            else
            {
                PianoModule.Session.PersistentWarpLinks.Add(ID, "");
            }
            /*            if (IsFirstTime)
                        {
                            WarpRune = Rune.Default;
                            Enabled = true;
                            InstantOpenDoors();
                        }
                        else*/
            if (WarpEnabled())
            {
                InstantOpenDoors();
            }
            else
            {
                InstantCloseDoors();
            }
        }
        public override void Update()
        {
            if (OwnWarpData != null && !string.IsNullOrEmpty(OwnWarpData.ID)) PianoModule.Session.PersistentWarpLinks[ID] = OwnWarpData.ID;
            base.Update();
            ShineTex.Scale = Bg.Scale;
            ShineTex.Color = Color.White * ShineAmount;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InputMachine.RemoveSelf();
            Shine.RemoveSelf();
        }
        public override void Interact(Player player)
        {
            WarpData data = TargetData;
            if (data != null)
            {
                Scene.Add(new CapsuleWarpHandler(this, data, player));
            }

        }
        [Command("reset_runes", "clears all collected runes from inventory")]
        public static void EraseRunes()
        {
            WARPData.ObtainedRunes.Clear();
        }

        public override bool WarpEnabled()
        {
            return RuneData != null && RuneData.HasRune && TargetData != null && TargetData.HasRune;// && Data != null && Data.HasRune && !Data.Rune.Match(RuneData.Rune);
        }
    }
}
