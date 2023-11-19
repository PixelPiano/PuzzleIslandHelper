//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.PandorasBox;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using ExtendedVariants.Variants;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ChargedWater")]
    [TrackedAs(typeof(ColoredWater))]
    public class ChargedWater : ColoredWater
    {
        private Player player;
        private ParticleType P_Deep = new ParticleType
        {
            SourceChooser = new Chooser<MTexture>(GFX.Game["objects/PuzzleIslandHelper/particles/bubble"], GFX.Game["objects/PuzzleIslandHelper/particles/bubbleSmall"]),
            Size = 1,
            Color = Color.DarkBlue * 0.5f,
            Color2 = Color.DarkSlateBlue * 0.5f,
            ColorMode = ParticleType.ColorModes.Choose,
            SpeedMin = 20,
            SpeedMax = 80,
            Direction = 270f.ToRad(),
            DirectionRange = 10f.ToRad(),
            LifeMin = 5f,
            LifeMax = 10f,
            SpeedMultiplier = 0.6f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 0.1f,
            Acceleration = new Vector2(2, -40)

        };
        private ParticleType P_Surface = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/arc"],
            Size = 1,
            SizeRange = 0.1f,
            Color = Color.DarkBlue * 0.5f,
            Color2 = Color.LightBlue * 0.5f,
            ColorMode = ParticleType.ColorModes.Choose,
            ScaleOut = true,
            SpeedMin = 4,
            SpeedMax = 6,
            LifeMin = 0.5f,
            LifeMax = 2f,
            FadeMode = ParticleType.FadeModes.InAndOut,

        };
        private ParticleType P_Dash = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/arc"],
            Size = 1,
            Color = Color.DarkBlue * 0.5f,
            Color2 = Color.LightBlue * 0.5f,
            ColorMode = ParticleType.ColorModes.Choose,
            ScaleOut = true,
            SpeedMin = 10,
            SpeedMax = 40,
            LifeMin = 2f,
            LifeMax = 4f,
            FadeMode = ParticleType.FadeModes.InAndOut,

        };
        private ParticleType P_SurfaceFast = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/chargedWater00"],
            Size = 1,
            Color = Color.DarkBlue,
            Color2 = Color.Blue,
            ColorMode = ParticleType.ColorModes.Choose,
            SpeedMin = 100,
            SpeedMax = 200,
            Direction = 270f.ToRad(),
            LifeMin = 0.2f,
            LifeMax = 1f,
            SpeedMultiplier = 3f,
            FadeMode = ParticleType.FadeModes.InAndOut,
            Friction = 2f,
            Acceleration = new Vector2(3, 30)

        };
        private ParticleSystem BubbleSystem;
        private ParticleSystem SurfaceSystem;
        private Rectangle TopDetect;
        private Rectangle RightDetect;
        private Rectangle LeftDetect;
        private Rectangle BottomDetect;
        private bool InBubble;

        private Bubble.BubbleType BubbleType;
        private bool LeftSide;
        private bool RightSide;
        private bool TopSide;
        private bool BottomSide;

        private bool Detected;
        public ChargedWater(EntityData data, Vector2 offset) : base(CreateData(data), offset)
        {
            BubbleType = data.Enum<Bubble.BubbleType>("bubbleType");
            LeftSide = data.Bool("bubbleLeft");
            RightSide = data.Bool("bubbleRight");
            TopSide = data.Bool("bubbleUp");
            BottomSide = data.Bool("bubbleDown");
            if (TopSide)
            {
                TopDetect = new Rectangle((int)Position.X, (int)Position.Y - 10, data.Width, 10);
            }
            if (LeftSide)
            {
                LeftDetect = new Rectangle((int)Position.X - 10, (int)Position.Y, 10, data.Height);
            }
            if (BottomSide)
            {
                BottomDetect = new Rectangle((int)Position.X, (int)Position.Y + (int)Height, data.Width, 10);
            }
            if (RightSide)
            {
                RightDetect = new Rectangle((int)Position.X + (int)Width, (int)Position.Y, 10, data.Height);
            }
            rayTopColor *= 0.3f;
            Add(new MaskRenderHook(Drawing, Mask));
        }
        private void Drawing()
        {
            if (BubbleSystem is not null)
            {
                BubbleSystem.Render();
            }
        }
        private void Mask()
        {
            Draw.Rect(Collider, Color.White);
        }
        private static EntityData CreateData(EntityData thisData)
        {
            EntityData data = new EntityData
            {
                Name = "pandorasBox/coloredWater",
                Position = thisData.Position,
                Width = thisData.Width,
                Height = thisData.Height,
                Values = new()
                {
                    {"color", "004e96"},
                    {"hasTop", thisData.Bool("bubbleUp") },
                    {"hasBottom", thisData.Bool("bubbleDown") },
                    {"hasLeft",  thisData.Bool("bubbleLeft")},
                    {"hasRight", thisData.Bool("bubbleRight") },
                    {"hasTopRays", thisData.Bool("bubbleUp") },
                    {"hasBottomRays", false},
                    {"hasLeftRays", false },
                    {"hasRightRays", false },
                    {"canJumpOnSurface", true }
                }
            };

            return data;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (TopSide)
            {
                Draw.HollowRect(TopDetect, Color.Orange);
            }
            if (BottomSide)
            {
                Draw.HollowRect(BottomDetect, Color.Orange);
            }
            if (LeftSide)
            {
                Draw.HollowRect(LeftDetect, Color.Orange);
            }
            if (RightSide)
            {
                Draw.HollowRect(RightDetect, Color.Orange);
            }

        }
        public bool DashingOut(Player player)
        {
            bool dashing = player.StateMachine.State == Player.StDash || player.StartedDashing || player.DashAttacking;
            bool detected = false;
            Rectangle bounds = player.Collider.Bounds;
            bounds.Height += 2;
            if (TopSide)
            {
                bool dashUp = dashing && player.DashDir.Y < 0;
                if (dashUp)
                {
                    detected = player.CollideRect(TopDetect);
                }
            }
            if (BottomSide && !detected)
            {
                bool dashDown = dashing && player.DashDir.Y > 0;
                if (dashDown)
                {
                    detected = player.CollideRect(BottomDetect);
                }
            }
            if (LeftSide && !detected)
            {
                bool dashLeft = dashing && player.DashDir.X < 0;
                if (dashLeft)
                {
                    detected = player.CollideRect(LeftDetect);
                }
            }
            if (RightSide && !detected)
            {
                bool dashRight = dashing && player.DashDir.X > 0;
                if (dashRight)
                {
                    detected = player.CollideRect(RightDetect);
                }
            }
            if (detected)
            {
                return Scene.CollideFirst<Water>(bounds) == this;
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            if (player is not null)
            {
                if (!player.Dead && !InBubble && DashingOut(player))
                {
                    Add(new Coroutine(CollideAdjust()));
                }
            }
            if (Scene.OnInterval(25 / 60f)) DeepParticles();
            if (Scene.OnInterval(30 / 60f)) SurfaceParticles();
        }
        private IEnumerator CollideAdjust()
        {
            Collidable = false;
            //player.Position.Y--;
            DashParticles(player.DashDir);
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            Vector2 position = new Vector2((int)player.Center.X - 16, (int)player.Center.Y - 16);
            Bubble bub = new Bubble(position, true, 1, BubbleType, false);
            bub.OnRemoved = delegate { InBubble = false; };
            InBubble = true;
            Scene.Add(bub);
            yield return 0.1f;
            Collidable = true;
        }
        private void SurfaceParticles()
        {
            if (TopSide)
            {
                float x = Calc.Random.Range(0, Width);
                P_Surface.Direction = 270f.ToRad();
                SurfaceSystem.Emit(P_Surface, Position + new Vector2(x, 3));
            }
            if (BottomSide)
            {
                float x = Calc.Random.Range(0, Width);
                P_Surface.Direction = 90f.ToRad();
                SurfaceSystem.Emit(P_Surface, Position + new Vector2(x, Height - 3));
            }
            if (LeftSide)
            {
                float y = Calc.Random.Range(0, Height);
                P_Surface.Direction = 180f.ToRad();
                SurfaceSystem.Emit(P_Surface, Position + new Vector2(3, y));
            }
            if (RightSide)
            {
                float y = Calc.Random.Range(0, Height);
                P_Surface.Direction = 0f.ToRad();
                SurfaceSystem.Emit(P_Surface, Position + new Vector2(Width - 3, y));
            }
        }
        private void DashParticles(Vector2 dir)
        {
            for (int i = 0; i < 7; i++)
            {
                SurfaceSystem.Emit(P_Dash, 1, player.Center, Vector2.One * 3, dir.Angle());
            }
        }
        private void DeepParticles()
        {

            float x = Calc.Random.Range(0, Width);
            BubbleSystem.Emit(P_Deep, Position + new Vector2(x, Height + 8));

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(BubbleSystem = new ParticleSystem(Depth + 1, 100));
            BubbleSystem.Visible = false;
            scene.Add(SurfaceSystem = new ParticleSystem(Depth + 1, 200));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (BubbleSystem != null) scene.Remove(BubbleSystem);
            if (SurfaceSystem != null) scene.Remove(SurfaceSystem);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
            if (PianoUtils.SeekController<RenderHelper>(scene) == null)
            {
                //scene.Add(new LabLightRenderer(scene));
            }
        }
    }
}
