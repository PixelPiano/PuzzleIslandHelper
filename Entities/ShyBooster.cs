using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ShyBooster")]
    public class ShyBooster : Booster
    {
        public bool ColorSpecific;
        public ShyBooster(EntityData data, Vector2 offset) : base(data.Position + offset, true)
        {
            TransitionListener listener = new TransitionListener();
            listener.OnInBegin = () =>
            {
                if (Engine.Scene.GetPlayer() is Player player && player.CurrentBooster != null && player.CurrentBooster != this && (!ColorSpecific || player.CurrentBooster.red == red))
                {
                    Visible = false;
                    Collidable = false;
                }
            };
            Add(listener);
        }
        public override void Update()
        {
            if (Scene.GetPlayer() is Player player)
            {
                if (player.CurrentBooster != null && player.CurrentBooster != this && (!ColorSpecific || player.CurrentBooster.red == red))
                {
                    Collidable = false;
                    Visible = false;
                }
                else
                {
                    Collidable = true;
                    Visible = true;
                }
            }
            base.Update();

        }
    }
}