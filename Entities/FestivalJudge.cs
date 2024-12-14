using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FestivalJudge")]
    [Tracked]
    public class FestivalJudge : Entity
    {
        public string Flag;

        public Chair Chair;
        public Table JudgeTable;
        public class Table : JumpthruPlatform
        {
            public Image Image;
            public Table(Vector2 position, Image image) : base(position, 0, "default")
            {
                Image = image;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Components.RemoveAll<Image>();
                Add(Image);
            }
        }
        public Image ChairImage;
        public Image TableImage;
        public FestivalJudge(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }
        public FestivalJudge(Vector2 position) : base(position)
        {

        }
        public FestivalJudge() : base()
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ChairImage = new Image(GFX.Game["objects/PuzzleIslandHelper/chairs/digiA"]);
            TableImage = new Image(GFX.Game["objects/PuzzleIslandHelper/chairs/digiTable"]);
            ChairImage.Position.X = 6;
            TableImage.Position.Y = ChairImage.Height - TableImage.Height;

            Chair = new Chair(Position, 1, "digiA", "", new Vector2(4, 16), Chair.SitFacings.RightOnly)
            {
                DisableTalk = true,
            };
            Chair.Image.Color = Color.Lerp(Color.White, Color.Black, 0.3f);
            JudgeTable = new Table(Position, TableImage);
            scene.Add(Chair, JudgeTable);
        }
        public void SetUp(Player player)
        {
           
            Chair.Depth = 1;
            JudgeTable.Depth = -1;
            Chair.InstantMoveToSeat(player);
        }
        public override void Update()
        {
            Chair.Visible = JudgeTable.Visible = Visible;
            Chair.Position = JudgeTable.Position = Position;
            Chair.Position.Y += Chair.SitOffset.Y;
            base.Update();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Chair.RemoveSelf();
            JudgeTable.RemoveSelf();
        }
    }
}