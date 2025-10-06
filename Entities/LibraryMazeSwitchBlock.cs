using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LibraryMazeSwitchBlock")]
    [Tracked]
    public class LibraryMazeSwitchBlock : DashBlock
    {
        public FlagList RequiredFlags;
        public FlagList FlagsToSet;
        private string flag => "LibraryMazeSwitchBlock:" + id.Key;
        private bool disabled;
        public LibraryMazeSwitchBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Char("tiletype", '9'), data.Width, data.Height, false, false, true, id)
        {
            RequiredFlags = data.FlagList("requiredFlags");
            FlagsToSet = data.FlagList("flagsToSet");
            OnDashCollide = NewOnDashed;
            Tag |= Tags.TransitionUpdate;
        }
        public override void OnShake(Vector2 amount)
        {
            if (Components.Get<TileGrid>() is var grid)
            {
                grid.Position += amount;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.GetFlag(flag))
            {
                FlagsToSet.State = true;
                disabled = true;
            }
        }
        public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
        {
            if (!RequiredFlags || disabled)
            {
                return DashCollisionResults.NormalCollision;
            }
            if (tileType == '1')
            {
                Audio.Play("event:/game/general/wall_break_dirt", Position);
            }
            else if (tileType == '3')
            {
                Audio.Play("event:/game/general/wall_break_ice", Position);
            }
            else if (tileType == '9')
            {
                Audio.Play("event:/game/general/wall_break_wood", Position);
            }
            else
            {
                Audio.Play("event:/game/general/wall_break_stone", Position);
            }
            FlagsToSet.State = true;
            SceneAs<Level>().Session.SetFlag(flag);
            disabled = true;
            StartShaking(0.5f);
            return DashCollisionResults.Rebound;
        }
    }
}