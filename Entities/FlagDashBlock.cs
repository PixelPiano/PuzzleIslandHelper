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

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FlagDashBlock")]
    [Tracked]
    public class FlagDashBlock : DashBlock
    {
        public Generated Generated;
        private bool allowAnimations;
        private FlagList flagOnBreak;
        private FlagList canDashFlag;
        private FlagList canBoosterFlag;
        private FlagList flag;
        private bool flagActive;
        private bool flagVisible;
        private bool flagCollision;
        public FlagDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            allowAnimations = data.Bool("allowAnimatedTiles");
            flagOnBreak = data.FlagList("flagOnBreak");
            canDashFlag = data.FlagList("canDashFlag");
            canBoosterFlag = data.FlagList("canBoosterFlag");
            flag = data.FlagList("flag");
            flagActive = data.Bool("flagAffectActive", true);
            flagVisible = data.Bool("flagAffectVisible", true);
            flagCollision = data.Bool("flagAffectCollision", true);
            blendIn = data.Bool("blendIn");
            OnDashCollide = NewOnDashed;
        }
        public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
        {
            if (!canDash && (!canBoosterFlag || (player.StateMachine.State != 5 && player.StateMachine.State != 10)))
            {
                return DashCollisionResults.NormalCollision;
            }
            Break(player.Center, direction, true);
            flagOnBreak.State = true;
            return DashCollisionResults.Rebound;
        }
        public override void Update()
        {
            bool flag = this.flag.State;
            if (flagVisible)
            {
                Visible = flag;
            }
            if (flagCollision)
            {
                Collidable = flag;
            }
            canDash = canDashFlag;
            if (flagActive && !flag)
            {
                return;
            }
            base.Update();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (allowAnimations)
            {
                if (!blendIn)
                {
                    Generated = GFX.FGAutotiler.GenerateBox(tileType, (int)width / 8, (int)height / 8);
                }
                else
                {
                    Level level = SceneAs<Level>();
                    Rectangle tileBounds = level.Session.MapData.TileBounds;
                    VirtualMap<char> solidsData = level.SolidsData;
                    int x = (int)(base.X / 8f) - tileBounds.Left;
                    int y = (int)(base.Y / 8f) - tileBounds.Top;
                    int tilesX = (int)base.Width / 8;
                    int tilesY = (int)base.Height / 8;
                    Generated = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData);
                }
                Add(Generated.SpriteOverlay);
            }
            canDash = canDashFlag;
            bool flag = this.flag.State;
            if (flagVisible)
            {
                Visible = flag;
            }
            if (flagCollision)
            {
                Collidable = flag;
            }
        }
    }
}