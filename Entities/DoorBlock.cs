using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/DoorBlock")]
    public class DoorBlock : Solid
    {
        private TileGrid tiles;
        private char tiletype;
        private bool blendin;
        private FlagData flag;
        private Vector2 node;
        private Vector2 start;
        private bool persistent;
        public DoorBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            tiletype = data.Char("tiletype");
            flag = data.Flag();
            blendin = data.Bool("blendIn");
            node = data.NodesOffset(offset)[0];
            persistent = data.Bool("persistent");
            start = Position;
            if (persistent)
            {
                Tag |= Tags.Persistent;
            }
            Tag |= Tags.TransitionUpdate;
            TransitionListener listener = new();
            listener.OnInBegin = delegate
            {
                MoveTo(flag.State ? node : start);
                if (CollideCheck<Player>())
                {
                    RemoveSelf();
                }
            };
            Depth = -10501;
            Add(listener);

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (blendin)
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)(X / 8f) - tileBounds.Left;
                int y = (int)(Y / 8f) - tileBounds.Top;
                int tilesX = (int)Width / 8;
                int tilesY = (int)Height / 8;
                tiles = GFX.FGAutotiler.GenerateOverlay(tiletype, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());

            }
            else
            {
                tiles = GFX.FGAutotiler.GenerateBox(tiletype, (int)Width / 8, (int)Height / 8).TileGrid;
            }
            Add(new LightOcclude());
            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: true));
        }
        public override void Update()
        {
            base.Update();
            Vector2 target = flag.State ? node : start;
            if (Position != target)
            {
                MoveTowardsX(target.X, 30f * Engine.DeltaTime);
                MoveTowardsY(target.Y, 30f * Engine.DeltaTime);
                StartShaking(Engine.DeltaTime * 2);
            }
            else if (shakeTimer > 0) StopShaking();
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
    }
}