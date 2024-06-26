using Celeste.Mod.Backdrops;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/ColorgradeOverlay")]
    public class ColorgradeOverlay : Backdrop
    {
        private string flag;
        private bool previousState;
        private bool State;
        public string OnColorGrade = "oldsite";
        public string OffColorGrade = "golden";
        private bool Fade;
        private float OnTime;
        private float OffTime;
        public ColorgradeOverlay(BinaryPacker.Element data)
            : this(data.Attr("colorgradeFlag"),
              data.Attr("colorgradeWhenTrue", "oldsite"),
              data.Attr("colorgradeWhenFalse", "none"),
              data.AttrBool("fadeOnFlagSwitch"),
              data.AttrFloat("timeModWhenTrue"),
              data.AttrFloat("timeModWhenFalse"))
        { }
        public ColorgradeOverlay(string flag, string colorgradeWhenTrue, string colorgradeWhenFalse, bool fade, float onTime, float offTime)
        {
            this.flag = flag;
            OnColorGrade = colorgradeWhenTrue;
            OffColorGrade = colorgradeWhenFalse;
            Fade = fade;
            OnTime = onTime;
            OffTime = offTime;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            previousState = State;
            State = (scene as Level).Session.GetFlag(flag);
            if (IsVisible(scene as Level))
            {
                if (previousState != State)
                {
                    if (Fade)
                    {
                        (scene as Level).NextColorGrade(State ? OnColorGrade : OffColorGrade);
                    }
                    else
                    {
                        (scene as Level).SnapColorGrade(State ? OnColorGrade : OffColorGrade);
                    }
                }

            }
        }
    }
}

