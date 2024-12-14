using Celeste.Mod.Backdrops;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomBackdrop("PuzzleIslandHelper/BackdropSafetyCheck")]
    public class BackdropSafetyCheck : Backdrop
    {
        public string Flag;
        public BackdropSafetyCheck(BinaryPacker.Element data) : this(data.Attr("area")) { }
        public BackdropSafetyCheck(string area) : base()
        {
            Flag = area;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Level level = scene as Level;
            if (IsVisible(level))
            {
                AreaFlagHelper.SetFlags(level,Flag);
            }
        }
    }
}
