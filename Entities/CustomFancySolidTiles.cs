using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomFancySolidTiles")]
    [Tracked]
    public class CustomFancySolidTiles : FancySolidTiles
    {
        public FlagList Flag;
        public bool OnlyCheckFlagOnAwake;
        public bool FlagState;
        public CustomFancySolidTiles(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            Flag = data.FlagList("flag");
            OnlyCheckFlagOnAwake = data.Bool("onlyCheckFlagOnAwake");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            FlagState = Flag;
            if (!FlagState)
            {
                Collidable = false;
            }
        }
        public override void Update()
        {
            if (!OnlyCheckFlagOnAwake)
            {
                FlagState = Flag;
                Collidable = FlagState;
            }
            if (FlagState)
            {
                base.Update();
            }
        }
        public override void Render()
        {
            if (FlagState)
            {
                base.Render();
            }
        }

    }
}