using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Autotiler;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FlagConditionBlock")]
    [Tracked]
    public class FlagConditionBlock : Solid
    {
        private EntityID id;
        private char tileType;
        private bool blendIn;
        private FlagList Flag;
        private TileGrid tileGrid;
        private AnimatedTiles animatedTiles;
        private bool useAnimatedTiles;
        public FlagConditionBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, true)
        {
            tileType = data.Char("tileType", '3');
            Flag = data.FlagList();
            blendIn = data.Bool("blendIn");
            Depth = -12999;
            this.id = id;
            useAnimatedTiles = data.Bool("useAnimatedTiles");
            Tag |= Tags.TransitionUpdate;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Generated g;
            if (!blendIn)
            {
                g = PianoUtils.GetTileBox(Width, Height, tileType);
            }
            else
            {
                g = PianoUtils.GetTileOverlay(scene, X, Y, Width, Height, tileType);
                Add(new EffectCutout());
            }
            tileGrid = g.TileGrid;
            animatedTiles = g.SpriteOverlay;
            Add(new LightOcclude());
            Add(tileGrid);
            if (useAnimatedTiles)
            {
                Add(animatedTiles);
            }
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (scene.GetPlayer() is Player player && CollideCheck(player))
            {
                Collidable = false;
                if (Flag)
                {
                    tileGrid.Alpha = animatedTiles.Alpha = 0.5f;
                }
                else
                {
                    Visible = false;
                }
            }
        }
        private bool wasColliding;
        public override void Update()
        {
            base.Update();

            if (CollideCheck<Player>())
            {
                wasColliding = true;
                Collidable = false;
                if (Flag)
                {
                    Visible = true;
                    if (tileGrid.Alpha != 0.5f)
                    {
                        tileGrid.Alpha = animatedTiles.Alpha = Calc.Approach(tileGrid.Alpha, 0.5f, 2 * Engine.DeltaTime);
                    }
                }
                else
                {
                    Visible = false;
                }
            }
            else
            {
                if (wasColliding && Flag)
                {
                    Audio.Play("event:/game/general/passage_closed_behind", Center);
                }
                wasColliding = false;
                if (tileGrid.Alpha != 1)
                {
                    tileGrid.Alpha = animatedTiles.Alpha = Calc.Approach(tileGrid.Alpha, 1, 2 * Engine.DeltaTime);
                }
                Collidable = Visible = Flag;
            }
        }
    }
}