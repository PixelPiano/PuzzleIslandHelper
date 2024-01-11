using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Gameshow
{
    [CustomEntity("PuzzleIslandHelper/StageLight")]
    [Tracked]
    public class StageLight : Entity
    {
        public Sprite Sprite;
        public int Index;
        public bool Activated;
        public VertexLight Light;
        public StageLight(Vector2 position, int index) : base(position)
        {
            Index = index;
            Sprite = new Sprite(GFX.Game,"objects/PuzzleIslandHelper/gameshow/stageLight/");
            Sprite.AddLoop("idle","stageLight",0.1f);
            Light = new VertexLight(Color.White,1,30,60);
            
            Add(Sprite);
            Add(Light);
            Light.Visible = false;
            Sprite.Visible = false;
        }
        public void Activate()
        {
            Sprite.Visible = true;
            Light.Visible = true;
        }
        public StageLight(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("index"))
        {

        }
    }
}
