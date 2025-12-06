using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomBlock")]
    [Tracked]
    public class CustomBlock : Solid
    {
        private EntityID id;
        private char tileType;
        private FlagList collidableFlags;
        private FlagList fadeFlags;
        private bool invertCollision;
        private bool fadeWhenInside;
        private bool waiting;
        private bool blendIn;
        private TileGrid tileGrid;
        private float alpha = 1;
        public const float FadeAlpha = 0.4f;
        public CustomBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            tileType = data.Char("tiletype", '3');
            fadeWhenInside = data.Bool("fadeWhenInside", true);
            collidableFlags = data.FlagList("collisionFlags");
            fadeFlags = data.FlagList("fadeFlags");
            invertCollision = data.Bool("invertCollision");
            blendIn = data.Bool("blendIn");
            Depth = -12999;
            this.id = id;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!blendIn)
            {
                tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            }
            else
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)(X / 8f) - tileBounds.Left;
                int y = (int)(Y / 8f) - tileBounds.Top;
                int tilesX = (int)Width / 8;
                int tilesY = (int)Height / 8;
                tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
                Add(new EffectCutout());
            }
            Add(new LightOcclude());
            Add(tileGrid);
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>())
            {
                Collidable = false;
                alpha = FadeAlpha;
                tileGrid.Color = Color.White * alpha;
                waiting = true;
            }
        }
        public bool FadeFlagsState => fadeFlags;
        public bool CollisionFlagState => collidableFlags;
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;

            if (FadeFlagsState || (fadeWhenInside && !Collidable && Collider.Collide(player.Collider.Bounds)))
            {
                alpha = Calc.Approach(alpha, FadeAlpha, Engine.DeltaTime);
            }
            else
            {

                alpha = Calc.Approach(alpha, 1, Engine.DeltaTime);
            }
            bool c = CollideCheck<Player>();
            if (waiting && c)
            {
                waiting = false;
            }
            if (waiting)
            {
                if (!CollideCheck<Player>())
                {
                    waiting = false;
                }
            }
            else
            {
                Collidable = CollisionFlagState;
            }
            tileGrid.Color = Color.White * alpha;
        }
    }
}