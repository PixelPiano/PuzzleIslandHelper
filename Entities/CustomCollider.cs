using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;


//I give up this actually sucks so much
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomCollider")]
    [Tracked]
    public class CustomCollider : Entity
    {
        public CustomCollider(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
           
        }
       
    }
}
