using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using static Celeste.Autotiler;
using static Celeste.Mod.PuzzleIslandHelper.Effects.TilesColorgrade;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RemoteDashBlock")]
    [TrackedAs(typeof(DashBlock))]
    public class RemoteDashBlock : FlagDashBlock
    {
        private FlagList remoteShakeFlag;
        private FlagList remoteBreakFlag;

        private bool prevShakeState, prevBreakState;
        private float shakeTime = -1;
        private bool activateShakerBeforeBreaking;
        private bool useCutscene;
        private bool canShake = true;
        private bool cutsceneOnTransition;
        private BetterShaker shaker;
        [OnLoad]
        public static void Load2()
        {
            On.Celeste.Platform.StartShaking += Platform_StartShaking;
            On.Celeste.Platform.StopShaking += Platform_StopShaking;
        }
        [OnUnload]
        public static void Unload2()
        {
            On.Celeste.Platform.StartShaking -= Platform_StartShaking;
            On.Celeste.Platform.StopShaking -= Platform_StopShaking;
        }
        private static void Platform_StopShaking(On.Celeste.Platform.orig_StopShaking orig, Platform self)
        {
            if (self is RemoteDashBlock)
            {
                (self as RemoteDashBlock).shaker.StopShaking();
            }
            else
            {
                orig(self);
            }
        }

        private static void Platform_StartShaking(On.Celeste.Platform.orig_StartShaking orig, Platform self, float time)
        {
            if (self is RemoteDashBlock)
            {
                (self as RemoteDashBlock).shaker.ShakeFor(time);
            }
            else
            {
                orig(self, time);
            }
        }

        public RemoteDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            cutsceneOnTransition = data.Bool("cutsceneOnTransition");
            useCutscene = data.Bool("useCutscene");
            shakeTime = data.Float("shakeTime");
            activateShakerBeforeBreaking = data.Bool("shakeBeforeBreak");
            remoteShakeFlag = data.FlagList("forceShakeFlag");
            remoteBreakFlag = data.FlagList("forceBreakFlag");
            if (remoteShakeFlag.Empty) remoteShakeFlag.ForcedValue = false;
            if (remoteBreakFlag.Empty) remoteBreakFlag.ForcedValue = false;
            if (activateShakerBeforeBreaking && shakeTime < 0) shakeTime = 0;
            Add(shaker = new BetterShaker(OnShake) { UseRawDeltaTime = true });
            TransitionListener listener = new();
            listener.OnInBegin = () =>
            {
                if (remoteBreakFlag)
                {
                    if (!useCutscene || !cutsceneOnTransition)
                    {
                        if (permanent)
                        {
                            RemoveAndFlagAsGone();
                        }
                        else
                        {
                            RemoveSelf();
                        }
                    }
                    else if (cutsceneOnTransition)
                    {
                        canShake = false;
                        FreezeTimeBreak.Begin(this, tileType, shakeTime, flagOnBreak);
                    }
                }
            };
            Add(listener);
        }

        public void ShakeSfx()
        {
            if (tileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", base.Center);
            }
            else if (tileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", base.Center);
            }
            else if (tileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_shake", base.Center);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            prevBreakState = remoteBreakFlag;
            prevShakeState = remoteShakeFlag;
        }
        public void CustomBreak()
        {
            if (activateShakerBeforeBreaking)
            {
                canShake = false;
                if (useCutscene)
                {
                    FreezeTimeBreak.Begin(this, tileType, shakeTime, flagOnBreak);
                    return;
                }
                else if (shakeTime > 0)
                {
                    ShakeSfx();
                    StartShaking(shakeTime);
                    Alarm.Set(this, shakeTime, () =>
                    {
                        Break(Center, Vector2.Zero, true);
                    });
                    return;
                }
            }
            Break(Center, Vector2.Zero, true);
        }
        public override void Update()
        {
            base.Update();
            bool shakeFlag = remoteShakeFlag;
            bool breakFlag = remoteBreakFlag;
            if (shakeFlag)
            {
                if (!prevShakeState && canShake)
                {
                    StartShaking(shakeTime);
                }
            }
            if (breakFlag)
            {
                if (!prevBreakState)
                {
                    CustomBreak();
                }
            }
            prevBreakState = breakFlag;
            prevShakeState = shakeFlag;

        }
    }
    [CustomEntity("PuzzleIslandHelper/FlagDashBlock")]
    [TrackedAs(typeof(DashBlock))]
    public class FlagDashBlock : DashBlock
    {
        public Generated Generated;
        private bool allowAnimations;
        public FlagList flagOnBreak;
        private FlagList canDashFlag;
        private FlagList canBoosterFlag;
        private FlagList spriteVisibleFlag;
        private FlagList spritePlayFlag;
        private FlagList flag;
        private bool flagActive;
        private bool flagVisible;
        private bool flagCollision;
        private Sprite sprite;
        private VertexLight spriteLight;
        public FlagDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            DisableLightsInside = data.Bool("disableLightsInside", true);
            flagOnBreak = data.FlagList("flagOnBreak");
            canDashFlag = data.FlagList("canDashFlag");
            canBoosterFlag = data.FlagList("canBoosterFlag");
            flag = data.FlagList("flag");
            spritePlayFlag = data.FlagList("spritePlayFlag");
            spriteVisibleFlag = data.FlagList("spriteVisibleFlag");
            allowAnimations = data.Bool("allowAnimatedTiles");
            flagActive = data.Bool("flagAffectActive", true);
            flagVisible = data.Bool("flagAffectVisible", true);
            flagCollision = data.Bool("flagAffectCollision", true);
            blendIn = data.Bool("blendIn");
            OnDashCollide = NewOnDashed;
            string s = data.Attr("centerSprite");
            if (!string.IsNullOrEmpty(s) && (GFX.Game.Has(s) || GFX.Game.Has(s + "00")))
            {
                sprite = new Sprite(GFX.Game, s);
                sprite.AddLoop("idle", "", 0.1f);
                sprite.CenterOrigin();
                sprite.Position = Collider.HalfSize;
            }
        }
        private void base_Awake(Scene scene)
        {
            foreach (Component component in Components)
            {
                component.EntityAwake();
            }

            if (!AllowStaticMovers)
            {
                return;
            }

            bool collidable = Collidable;
            Collidable = true;
            foreach (StaticMover component in scene.Tracker.GetComponents<StaticMover>())
            {
                if (component.Platform == null && component.IsRiding(this))
                {
                    staticMovers.Add(component);
                    component.Platform = this;
                    if (component.OnAttach != null)
                    {
                        component.OnAttach(this);
                    }
                }
            }

            Collidable = collidable;
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            if (sprite != null)
            {
                sprite.Position += amount;
            }
            if (spriteLight != null)
            {
                spriteLight.Position += amount;
            }
            if (animTiles != null)
            {
                animTiles.Position += amount;
            }
            tiles.Position += amount;
        }
        private TileGrid tiles;
        private AnimatedTiles animTiles;
        public override void Awake(Scene scene)
        {
            base_Awake(scene);
            TileGrid tileGrid;
            AnimatedTiles animatedTiles;
            if (!blendIn)
            {
                Generated g = GFX.FGAutotiler.GenerateBox(tileType, (int)width / 8, (int)height / 8);
                tileGrid = g.TileGrid;
                animatedTiles = g.SpriteOverlay;
                Add(new LightOcclude());
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
                Generated g = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData);
                tileGrid = g.TileGrid;
                animatedTiles = g.SpriteOverlay;
                Add(new EffectCutout());
                base.Depth = -10501;
            }

            Add(tiles = tileGrid);
            if (allowAnimations)
            {
                Add(animTiles = animatedTiles);
            }
            Add(new TileInterceptor(tileGrid, highPriority: true));
            if (CollideCheck<Player>())
            {
                RemoveSelf();
                return;
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
            if (sprite != null)
            {
                Add(sprite);
                sprite.Play("idle");
                sprite.Active = spritePlayFlag;
                sprite.Visible = spriteVisibleFlag;
                float radius = Math.Max(sprite.Width, sprite.Height);
                spriteLight = new VertexLight(Color.White, 1, (int)(radius / 2), (int)radius);
                spriteLight.Position = Collider.HalfSize;
                Add(spriteLight);
            }

        }
        public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
        {
            if ((!canDash || !canDashFlag) && (!canBoosterFlag || (player.StateMachine.State != 5 && player.StateMachine.State != 10)))
            {
                return DashCollisionResults.NormalCollision;
            }
            Break(player.Center, direction, true);
            return DashCollisionResults.Rebound;
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool += DashBlock_Break_Vector2_Vector2_bool;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool += DashBlock_Break_Vector2_Vector2_bool_bool;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool -= DashBlock_Break_Vector2_Vector2_bool;
            On.Celeste.DashBlock.Break_Vector2_Vector2_bool_bool -= DashBlock_Break_Vector2_Vector2_bool_bool;
        }
        private static void DashBlock_Break_Vector2_Vector2_bool_bool(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound, bool playDebrisSound)
        {
            orig(self, from, direction, playSound, playDebrisSound);
            if (self is FlagDashBlock)
            {
                (self as FlagDashBlock).flagOnBreak.State = true;
            }
        }

        private static void DashBlock_Break_Vector2_Vector2_bool(On.Celeste.DashBlock.orig_Break_Vector2_Vector2_bool orig, DashBlock self, Vector2 from, Vector2 direction, bool playSound)
        {
            orig(self, from, direction, playSound);
            if (self is FlagDashBlock)
            {
                (self as FlagDashBlock).flagOnBreak.State = true;
            }
        }
        public override void Update()
        {
            if (sprite != null)
            {
                sprite.Active = spritePlayFlag;
                sprite.Visible = spriteVisibleFlag;
                if (spriteLight != null)
                {
                    spriteLight.Active = sprite.Active;
                    spriteLight.Visible = sprite.Visible;
                }
            }
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
    }
}