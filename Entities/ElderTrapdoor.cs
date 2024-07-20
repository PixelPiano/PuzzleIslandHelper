using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ElderTrapdoor")]
    [Tracked]
    public class ElderTrapdoor : Solid
    {
        public static MTexture TextureFG => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/textureFG"];
        public static MTexture TextureBG => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/textureBG"];
        public static MTexture TextureBlock => GFX.Game["objects/PuzzleIslandHelper/elderTrapdoor/block"];
        private Vector2 from;
        private Vector2 to;
        private Vector2 prev;
        private Solid Floor;
        private Solid Ceiling;
        private Image Image;
        public ElderTrapdoor(EntityData data, Vector2 offset) : base(data.Position + offset, TextureFG.Width, 8, true)
        {
            Tag |= Tags.TransitionUpdate;
            Image = new Image(TextureBG);
            Add(Image);
            Depth = 1;
            Position -= new Vector2(TextureFG.Width, TextureFG.Height) / 2;
            from = Position;
            to = data.NodesOffset(offset)[0] - new Vector2(TextureFG.Width, TextureFG.Height) / 2;
            Floor = new Solid(Position + Vector2.UnitY * (TextureFG.Height - 8), Width, Height, true)
            {
                new Image(TextureBlock),
                new LightOcclude()
            };
            Floor.Tag = Tags.TransitionUpdate;

            Ceiling = new Solid(Position, Width, Height, true)
            {
                new Image(TextureFG),
                new Image(TextureBlock),
                new LightOcclude()
            };
            Ceiling.Tag = Tags.TransitionUpdate;

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Floor);
            scene.Add(Ceiling);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Floor);
            scene.Remove(Ceiling);
        }
        public override void Update()
        {
            base.Update();
            SetProgress(TrapdoorChandelier.GlobalUpdater.Amount);
        }
        public void SetProgress(float percent)
        {
            prev = Position;
            Vector2 target = Vector2.Lerp(from, to, percent);
            Vector2 amount = target - prev;
            int x = (int)Math.Round(amount.X);
            int y = (int)Math.Round(amount.Y);
            MoveH(x);
            MoveV(y);
            Floor.MoveH(x);
            Floor.MoveV(y);
            Ceiling.MoveH(x);
            Ceiling.MoveV(y);
        }
    }
}