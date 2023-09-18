using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;

//hi carl
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FadeToColor")]
    public class FadeToColor : Entity
    {
        #region Variables

        private Level l;
        private bool onBegin = false;
        private string flag;
        private float opacity;
        private float rate;
        private float timer;
        private float Speed = 4;
        private Color color;
        private bool usesFlag;
        #endregion
        public FadeToColor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = TagsExt.SubHUD;
            flag = data.Attr("flag");
            Speed = data.Float("speed");
            color = data.HexColor("color");
            onBegin = data.Bool("onEnter");
            usesFlag = data.Bool("useFlag");
            if (onBegin)
            {
                opacity = 1;
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level == null)
            {
                return;
            }
            l = Scene as Level;
            if (!usesFlag || (usesFlag && SceneAs<Level>().Session.GetFlag(flag)))
            {
                Draw.Rect(0, 0, 1980, 1080, color * opacity);
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene as Level is null)
            {
                return;
            }
            if (onBegin && (!usesFlag || (usesFlag && SceneAs<Level>().Session.GetFlag(flag))))
            {
                if (opacity > 0)
                {
                    opacity -= Engine.DeltaTime * Speed;
                }
                else
                {
                    opacity = 0;
                }
            }else if (!usesFlag || (usesFlag && SceneAs<Level>().Session.GetFlag(flag)))
            {
                opacity = Calc.Approach(0, 1, rate += (Engine.DeltaTime * Speed));
            }
            else
            {
                rate = 0;
                opacity = 0;
                timer = 0;
            }
        }
        #region Finished
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        #endregion
    }

}
