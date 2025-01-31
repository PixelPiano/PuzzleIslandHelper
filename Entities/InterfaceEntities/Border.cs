using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class Border : Entity
    {
        public Image Image;
        public Interface Parent;
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (Image is not null)
                {
                    Image.Color = Color.White * value;
                }
                alpha = value;
            }
        }
        private float alpha;
        public const int BorderEdgeWidth = 96;
        public const int BorderEdgeHeight = 60;
        public Border(Interface parent) : base()
        {
            Tag = TagsExt.SubHUD;

            Add(Image = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/border00"]));
            Image.Color = Color.White * 0;
            Depth = Interface.BaseDepth - 7;
            Parent = parent;
        }
        public override void Render()
        {
            if (Parent.ForceHide) return;
            base.Render();
        }
    }
}