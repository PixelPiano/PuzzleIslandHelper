using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VivHelper.Entities;
using VivHelper.Entities.Spinner2;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/InvertBlock")]
    [Tracked]
    public class InvertBlock : Solid
    {
        private TileGrid tiles;
        
        private readonly string flag;
        private readonly bool startInverted;
        private readonly bool solidWhenOn;
        private readonly bool solidWhenOff;
        private float opacity;
        private Player player;
        private bool CanSwitch;
        private bool InvertFlag;
        private static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        private EntityID id;
        private bool Prev;
        private bool AfterAwake;
        private LightOcclude LightOcclude;
        private bool State
        {
            get
            {
                if (Scene as Level is null)
                {
                    return false;
                }
                if (CanSwitch)
                {
                    if (InvertFlag)
                    {
                        return !SceneAs<Level>().Session.GetFlag(flag);
                    }
                    else
                    {
                        return SceneAs<Level>().Session.GetFlag(flag);
                    }
                }
                else
                {
                    return startInverted;
                }
            }
        }
        private Level l;
        private static readonly BlendState Inverter = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceColor,
        };
        private VirtualRenderTarget InvertTiles;
        private VirtualRenderTarget InvertMask;
        public InvertBlock(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, false)
        {
            Tag = Tags.TransitionUpdate;

            flag = data.Attr("flag");
            this.id = id;
            startInverted = data.Bool("startState");
            InvertFlag = data.Bool("invertFlag");
            solidWhenOn = data.Bool("solidWhenNormal");
            solidWhenOff = data.Bool("solidWhenFlipped");
            CanSwitch = data.Bool("canSwitch");
            InvertTiles = VirtualContent.CreateRenderTarget("InvertTiles" + id.Key, (int)Width, (int)Height);
            InvertMask = VirtualContent.CreateRenderTarget("InvertMask" + id.Key, (int)Width, (int)Height);
            Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
            Add(new BeforeRenderHook(BeforeRender));
            AllowStaticMovers = true;
            Add(LightOcclude = new LightOcclude());
        }

        public void UpdateSpikeColor()
        {
            if (AllowStaticMovers)
            {
                foreach (StaticMover mover in staticMovers)
                {
                    if (mover.IsRiding(this))
                    {
                        switch (mover.Entity)
                        {
                            case Spikes spikes:
                                spikes.DisabledColor = spikes.EnabledColor * 0.3f;
                                spikes.VisibleWhenDisabled = true;
                                break;

                            case RainbowSpikes rspikes:
                                rspikes.DisabledColor = rspikes.oneColor * 0.3f;
                                rspikes.VisibleWhenDisabled = true;
                                break;
                        }
                    }
                }
            }
        }
       
      
        public override void Update()
        {
            base.Update();
            if (!AfterAwake)
            {
                UpdateSpikeColor();
                AfterAwake = true;
            }
            tiles.Visible = false;
            Prev = Collidable;
            Collidable = State ? solidWhenOn : solidWhenOff;
            LightOcclude.Visible = Collidable;
            opacity = Calc.Approach(opacity, Collidable ? 1 : 0.5f, Engine.DeltaTime * 2);
            if (Prev != Collidable && AllowStaticMovers)
            {

                if (Collidable)
                {

                    EnableStaticMovers();
                }
                else
                {
                    DisableStaticMovers();
                }
            }
            if (Collidable && CollideCheck<Player>())
            {
                player.Die(Vector2.Zero);
            }

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = SceneAs<Level>().Tracker.GetEntity<Player>();
            opacity = Collidable ? 1 : 0.5f;

        }
        private void Drawing()
        {
            Draw.Rect(Collider, Color.White);
        }
        private void BeforeRender()
        {
            if (Scene as Level == null)
            {
                return;
            }
            l = Scene as Level;


            #region SetRenderTarget
            Engine.Graphics.GraphicsDevice.SetRenderTarget(InvertMask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            Drawing();
            Draw.SpriteBatch.End();
            #endregion


            #region DrawToObject
            // EasyRendering.DrawToObject(InvertTiles, newTiles.Render, level,true);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(InvertTiles);
            Engine.Graphics.GraphicsDevice.Clear(State ? Color.White : Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, State ? Inverter : BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            tiles.RenderAt(Vector2.Zero);
            Draw.SpriteBatch.End();
            #endregion


            #region MaskToObject
            Engine.Graphics.GraphicsDevice.SetRenderTarget(InvertTiles);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null, l.Camera.Matrix);
            Draw.SpriteBatch.Draw(InvertMask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            #endregion
        }

        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(InvertTiles, Position, Color.White * opacity);
        }

    }
}