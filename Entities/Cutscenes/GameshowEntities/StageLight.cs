using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
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
            Depth = -10501;
            Index = index;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/gameshow/stageLight/");
            Sprite.AddLoop("idle", "stageLight", 0.1f);
            Sprite.Play("idle");
            Light = new VertexLight(Color.White, 1, 30, 60);
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
        public override void Update()
        {
            base.Update();
            Light.InSolidAlphaMultiplier = 1;
        }
        public StageLight(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("index"))
        {

        }
    }
}
