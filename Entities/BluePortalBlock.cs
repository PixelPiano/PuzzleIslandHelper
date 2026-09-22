using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.TSwitch
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/BluePortalBlock")]
    [Tracked]
    public class BluePortalBlock : Solid
    {
        private TileGrid tiles;
        private Vector2 visualOffset;
        private string ShakeSFX = "event:/game/general/fallblock_shake";
        public EntityID ID;
        private Image image;
        public BluePortalBlock(Vector2 position, int width, int height, EntityID id)
            : base(position, width, height, safe: false)
        {
            ID = id;
            Add(new LightOcclude());
        }
        public BluePortalBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Width, data.Height, id)
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(image = new Image(GFX.Game["objects/PuzzleIslandHelper/wipBluePortalDoor"]));
            image.JustifyOrigin(0.5f, 0.5f);
            image.Position = Collider.HalfSize;
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            visualOffset += amount;
        }
        public IEnumerator Sequence()
        {
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
            Audio.Play(ShakeSFX, Position);
            StartShaking(1);
            yield return 1;
            Audio.Play("event:/game/general/wall_break_stone", Position);
            for (int i = 0; (float)i < Width / 8f; i++)
            {
                for (int j = 0; (float)j < Height / 8f; j++)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '1', true).BlastFrom(Center));
                }
            }
            RemoveSelf();
        }
        public override void Render()
        {
            Position += visualOffset;
            Draw.HollowRect(Collider, Color.Gray);
            Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Black);
            base.Render();
            Position -= visualOffset;
        }
    }
}