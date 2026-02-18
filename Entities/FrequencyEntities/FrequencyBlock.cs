using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.Meta;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using static Celeste.Autotiler;
using static Celeste.Mod.PuzzleIslandHelper.Effects.TilesColorgrade;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities
{
    [CustomEntity("PuzzleIslandHelper/FrequencyBlock")]
    [Tracked]
    public class FrequencyBlock : DashBlock
    {
        public float ShakeMult;
        public FrequencyCodeComponent Code;
        public TileGrid Grid;
        public AnimatedTiles AnimatedTiles;
        public FrequencyBlock(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Char("tiletype"), data.Width, data.Height, data.Bool("blendIn"), data.Bool("permenant"), id,data.Int("rate1"),data.Int("rate2"),data.Int("rate3"),data.Int("rate4"))
        {

        }
        
        public FrequencyBlock(Vector2 position, char tiletype, float width, float height, bool blendIn, bool permanent, EntityID id, int rate1,int rate2, int rate3, int rate4) : base(position, tiletype, width, height, blendIn, permanent, false, id)
        {
            Code = new FrequencyCodeComponent(rate1,rate2,rate3,rate4);
            Code.OnPowerChange = (prev, current) =>
            {
                ShakeMult = Calc.Approach(ShakeMult, current > prev ? 1 : 0, Engine.DeltaTime);
            };
            Code.OnFullPower = () =>
            {
                Break(Center, Vector2.Zero, true);
            };
            Code.MarginOfError = 5;
            Add(Code);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Grid = Components.Get<TileGrid>();
            AnimatedTiles = Components.Get<AnimatedTiles>();
            StartShaking();
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            if(Grid != null)
            {
                Grid.Position += amount * ShakeMult;
            }
            if(AnimatedTiles != null)
            {
                AnimatedTiles.Position += amount * ShakeMult;
            }
            
        }
    }
}