using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using static Celeste.Autotiler;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CustomFakeWall")]
    [TrackedAs(typeof(FakeWall))]
    public class CustomFakeWall : Entity
    {
        public float FadeTo = 0;
        public float StartAlpha = 1;
        public float Alpha;
        public char fillTile;
        public TileGrid tiles;
        public EffectCutout cutout;
        public float transitionStartAlpha;
        public bool transitionFade;
        public EntityID eid;
        public bool playRevealWhenTransitionedInto = false;
        public FlagList Flag;
        public FlagList SolidFlag;
        public FlagList CanFadeFlag;
        public bool Permanent;
        public bool BlendIn;
        private string audioEventOnEnter = "event:/game/general/secret_revealed";
        private string audioEventOnLeave = "";
        private bool inside
        {
            get
            {
                Player player = CollideFirst<Player>();
                bool playerInside = player != null && player.StateMachine.State != 9;
                return CanFadeFlag && (!requirePlayerAndFlag || playerInside);
            }
        }
        private bool onlyCheckFlagOnAwake;
        private Solid dummySolid;
        private bool animated;
        private AnimatedTiles animatedTiles;
        private bool requirePlayerAndFlag;
        public CustomFakeWall(EntityID eid, Vector2 position, char tile, float width, float height, bool blendIn, bool animated, string flag, string canWalkThroughFlag, string canFadeFlag,
            bool requirePlayerAndFlag, float fadeFrom, float fadeTo, string audioEventOnEnter = "event:/game/general/secret_revealed", string audioEventOnLeave = "",
            bool transitionReveal = false, int depth = -13000, bool permanent = true, bool onlyCheckFlagOnAwake = false)
            : base(position)
        {
            this.requirePlayerAndFlag = requirePlayerAndFlag;
            this.animated = animated;
            SolidFlag = new FlagList(canWalkThroughFlag, true);
            CanFadeFlag = new FlagList(canFadeFlag);
            Flag = new FlagList(flag);
            BlendIn = blendIn;
            this.eid = eid;
            fillTile = tile;
            base.Collider = new Hitbox(width, height);
            base.Depth = depth;
            Add(cutout = new EffectCutout());
            playRevealWhenTransitionedInto = transitionReveal;
            FadeTo = fadeTo;
            this.audioEventOnEnter = audioEventOnEnter;
            this.audioEventOnLeave = audioEventOnLeave;
            Permanent = permanent;
            StartAlpha = Alpha = fadeFrom;
            this.onlyCheckFlagOnAwake = onlyCheckFlagOnAwake;
        }


        public CustomFakeWall(EntityData data, Vector2 offset, EntityID eid)
            : this(eid, data.Position + offset,
                  data.Char("tiletype", '3'),
                  data.Width, data.Height,
                  data.Bool("blendIn", true),
                  data.Bool("allowAnimatedTiles"),
                  data.Attr("flag"),
                  data.Attr("canWalkThroughFlag"),
                  data.Attr("canFadeFlag"),
                  data.Bool("requirePlayerAndFadeFlag"),
                  data.Float("fadeFrom", 1),
                  data.Float("fadeTo", 0),
                  data.Attr("audioEventOnEnter", "event:/game/general/secret_revealed"),
                  data.Attr("audioEventOnLeave", ""),
                  data.Bool("playTransitionReveal"),
                  data.Int("depth", -13000),
                  data.Bool("permanent"),
                  data.Bool("onlyCheckFlagOnAwake"))
        {
        }


        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(dummySolid = new Solid(Position, Width, Height, true));
            dummySolid.Collidable = !SolidFlag;
            int tilesX = (int)base.Width / 8;
            int tilesY = (int)base.Height / 8;
            Generated gen;
            if (BlendIn)
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = (int)base.X / 8 - tileBounds.Left;
                int y = (int)base.Y / 8 - tileBounds.Top;
                gen = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData);
            }
            else
            {
                gen = GFX.FGAutotiler.GenerateBox(fillTile, tilesX, tilesY);
            }
            tiles = gen.TileGrid;
            animatedTiles = gen.SpriteOverlay;
            Add(tiles);
            if (animated)
            {
                Add(animatedTiles);
            }
            Add(new TileInterceptor(tiles, highPriority: false));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Alpha = tiles.Alpha = cutout.Alpha = animatedTiles.Alpha = StartAlpha;
            if (CollideCheck<Player>() && CanFadeFlag)
            {
                dummySolid.Collidable = false;
                wasInside = true;
                Alpha = tiles.Alpha = animatedTiles.Alpha = FadeTo;
                cutout.Visible = false;
                if (playRevealWhenTransitionedInto && !string.IsNullOrEmpty(audioEventOnEnter))
                {
                    Audio.Play(audioEventOnEnter, base.Center);
                }
                if (Permanent)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(eid);
                }
            }
            else
            {
                TransitionListener transitionListener = new TransitionListener();
                transitionListener.OnOut = OnTransitionOut;
                transitionListener.OnOutBegin = OnTransitionOutBegin;
                transitionListener.OnIn = OnTransitionIn;
                transitionListener.OnInBegin = OnTransitionInBegin;
                Add(transitionListener);
            }
            if (!Flag)
            {
                Visible = false;
            }
        }

        public void OnTransitionOutBegin()
        {
            dummySolid.Collidable = !SolidFlag;
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            }
            else
            {
                transitionFade = false;
            }
        }



        public void OnTransitionOut(float percent)
        {
            dummySolid.Collidable = !SolidFlag;
            if (transitionFade)
            {
                tiles.Alpha = animatedTiles.Alpha = transitionStartAlpha * (1f - percent);
            }
        }



        public void OnTransitionInBegin()
        {
            dummySolid.Collidable = !SolidFlag;
            Level level = SceneAs<Level>();
            if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
            {
                transitionFade = true;
                tiles.Alpha = animatedTiles.Alpha = 0f;
            }
            else
            {
                transitionFade = false;
            }
        }



        public void OnTransitionIn(float percent)
        {
            dummySolid.Collidable = !SolidFlag;
            if (transitionFade)
            {
                tiles.Alpha = animatedTiles.Alpha = percent;
            }
        }
        private bool wasInside;
        public override void Update()
        {
            base.Update();
            dummySolid.Collidable = SolidFlag;
            if (!onlyCheckFlagOnAwake)
            {
                if (!Flag)
                {
                    Visible = false;
                    return;
                }
                else
                {
                    Visible = true;
                }
            }
            if (inside)
            {
                Alpha = tiles.Alpha = cutout.Alpha = animatedTiles.Alpha = Calc.Approach(Alpha, FadeTo, 2f * Engine.DeltaTime);
                if (!wasInside)
                {
                    if (Permanent)
                    {
                        SceneAs<Level>().Session.DoNotLoad.Add(eid);
                    }
                    if (!string.IsNullOrEmpty(audioEventOnEnter))
                    {
                        Audio.Play(audioEventOnEnter, base.Center);
                    }
                }
                wasInside = true;
            }
            else
            {
                Alpha = tiles.Alpha = cutout.Alpha = animatedTiles.Alpha = Calc.Approach(Alpha, StartAlpha, 2f * Engine.DeltaTime);
                if (wasInside)
                {
                    if (!string.IsNullOrEmpty(audioEventOnLeave))
                    {
                        Audio.Play(audioEventOnLeave, base.Center);
                    }
                }
                wasInside = false;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            dummySolid?.RemoveSelf();
        }

        public override void Render()
        {
            if (BlendIn)
            {
                Level level = base.Scene as Level;
                if (level.ShakeVector.X < 0f && level.Camera.X <= (float)level.Bounds.Left && base.X <= (float)level.Bounds.Left)
                {
                    tiles.RenderAt(Position + new Vector2(-3f, 0f));
                    if (animated)
                    {
                        animatedTiles.RenderAt(Position + new Vector2(-3f, 0f));
                    }
                }

                if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= (float)level.Bounds.Right && base.X + base.Width >= (float)level.Bounds.Right)
                {
                    tiles.RenderAt(Position + new Vector2(3f, 0f));
                    if (animated)
                    {
                        animatedTiles.RenderAt(Position + new Vector2(3f, 0f));
                    }
                }

                if (level.ShakeVector.Y < 0f && level.Camera.Y <= (float)level.Bounds.Top && base.Y <= (float)level.Bounds.Top)
                {
                    tiles.RenderAt(Position + new Vector2(0f, -3f));
                    if (animated)
                    {
                        animatedTiles.RenderAt(Position + new Vector2(0f, -3f));
                    }
                }

                if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= (float)level.Bounds.Bottom && base.Y + base.Height >= (float)level.Bounds.Bottom)
                {
                    tiles.RenderAt(Position + new Vector2(0f, 3f));
                    if (animated)
                    {
                        animatedTiles.RenderAt(Position + new Vector2(0f, 3f));
                    }
                }
            }
            base.Render();
        }
    }
}