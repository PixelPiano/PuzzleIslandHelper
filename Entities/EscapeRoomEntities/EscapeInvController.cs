using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.EscapeRoomEntities
{
    public class EscapeInvController : Entity
    {
        private string flag;
        public EscapeInvController(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            flag = data.Attr("flag");
        }
        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Session.GetFlag(flag))
            {
                AddInventory(Scene);
            }
        }
        public void AddInventory(Scene scene)
        {
            if (PianoModule.Session.EscapeInv == null)
            {
                PianoModule.Session.EscapeInv = new EscapeInv();
            }
            scene.Add(PianoModule.Session.EscapeInv);
            RemoveSelf();
        }
    }
}
