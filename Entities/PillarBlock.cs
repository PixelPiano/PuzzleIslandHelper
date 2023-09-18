using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.TSwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PillarBlock")]
    [Tracked]
    public class PillarBlock : Entity
    {
        private FancySolidTiles Block;
        private Sprite Cracks;
        private bool Activated;
        private bool Fallen;
        private string Tiles = "00000000000MMMMMMM," +
                               "000000MMMMMMMMMMMM," +
                               "MMMMMMMMMMMMMMMMM0," +
                               "MMMMMMMMMMMMM00MM0," +
                               "MMMMMMMMMMMMM00MM0," +
                               "MM0000000MM0000MM0," +
                               "000000000MM0000000," +
                               "000000000MM0000000," +
                               "000000000MM0000000," +
                               "000000000MM0000000," +
                               "000000000MM0000000";
        public PillarBlock(EntityData data, Vector2 offset)
          : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            EntityData BlockData = new EntityData
            {
                Name = "FancyTileEntities/FancySolidTiles",
                Position = data.Position
            };
            BlockData.Values = new()
            {
                {"randomSeed",0 },
                {"blendEdges",true },
                {"width",144 },
                {"height",88 },
                {"tileData",Tiles }
            };
            Block = new FancySolidTiles(BlockData, offset, new EntityID());
            Block.OnDashCollide = OnDash;

        }

        private DashCollisionResults OnDash(Player player, Vector2 direction)
        {
            if (Fallen)
            {
                for (int i = 0; i < Block.Width / 8f; i++)
                {
                    for (int j = 0; j < Block.Height / 8f; j++)
                    {
                        Scene.Add(Engine.Pooler.Create<Debris>().Init(Block.Position + new Vector2(4 + i * 8, 4 + j * 8), 'M', true).BlastFrom(player.Center));
                    }
                }
                Audio.Play("event:/game/general/wall_break_ice", Position);
                PianoModule.SaveData.PillarBlockState = 2;
                Block.RemoveSelf();
                RemoveSelf();
            }
            return DashCollisionResults.NormalCollision;
        }
        private void ShakeSfx()
        {
            Audio.Play("event:/game/general/fallblock_shake", Block.Center);
        }
        private void ImpactSfx()
        {
            Audio.Play("event:/game/general/fallblock_impact", Block.BottomCenter);
        }
        private IEnumerator FallSequence()
        {
            Activated = true;
            Vector2 Pos = Block.Position;
            Block.MoveHExact(1);
            yield return null;
            ShakeSfx();
            for (int i = 0; i < 4; i++)
            {
                Block.MoveHExact(-2);
                yield return Engine.DeltaTime * 2;
                Block.MoveHExact(2);
                yield return Engine.DeltaTime * 2;
            }
            Block.MoveHExact(-2);
            yield return null;
            Block.Position = Pos;
            float speed = 0f;
            float maxSpeed = 160f;

            while (Block.Position.Y < Pos.Y + 88)
            {
                speed = Calc.Approach(speed, maxSpeed, 12f * Engine.DeltaTime);
                Block.MoveTowardsY(Pos.Y + 88, speed);
                yield return null;
            }
            ImpactSfx();
            Cracks.Play("crack");
            Fallen = true;
            PianoModule.SaveData.PillarBlockState = 1;
            SceneAs<Level>().Session.SetFlag("pillarBlockSpinner", true);
            yield return 0.1f;
            SceneAs<Level>().Session.SetFlag("pillarBlockSpinnerFlag", true);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            switch (PianoModule.SaveData.PillarBlockState)
            {
                case 0:
                    scene.Add(Block);
                    break;
                case 1:
                    scene.Add(Block);
                    Block.Position.Y += 88;
                    Activated = true;
                    Fallen = true;
                    break;
                case 2:
                    RemoveSelf();
                    break;
            }
            Depth = -100001;
            Add(Cracks = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/pillarBlock/"));
            Cracks.AddLoop("idle", "cracks", 0.1f, 2);
            Cracks.Add("crack", "cracks", 0.05f, "idle");
            Cracks.Color = Color.White * 0.7f;

        }
        public override void Update()
        {
            base.Update();
            if(Block is not null)
            {
                Cracks.Position = Block.Position - Position;
            }
            if (!Activated && PianoModule.SaveData.BrokenPillars.Count == 3)
            {
                Add(new Coroutine(FallSequence()));
            }
        }
    }
}