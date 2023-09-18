using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;

//hi carl
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/HowToKeepAnIdiotBusy")]
    public class HowToKeepAnIdiotBusy : Entity
    {
        #region Variables

        private string flag;
        private static float opacity;
        private static float rate;
        private static bool wasSet;
        private static float timer;
        #endregion
        public HowToKeepAnIdiotBusy(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag = TagsExt.SubHUD;
            flag = data.Attr("flag");
        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level == null)
            {
                return;
            }

          
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                ActiveFont.Draw("How to keep an idiot busy", new Vector2(8,8), Vector2.Zero, Vector2.One, Color.White * opacity);
            }
        }
        public override void Update()
        {
            base.Update();
            if(Scene as Level is null)
            {
                return;
            }
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                if (!wasSet)
                {
                    timer = 8;
                    wasSet = true;
                }
               if (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                }
                else
                {
                    opacity = Calc.Approach(0, 1, rate += Engine.DeltaTime / 20);
                }
            }
            else
            {
                rate = 0;
                opacity = 0;
                timer = 0;
                wasSet = false;
            }
        }
    }

}
