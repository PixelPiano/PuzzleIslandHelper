using Celeste.Mod.Entities;
using Celeste.Mod.PandorasBox;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using FrostHelper;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.WaterDroplet
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WaterDroplet")]
    [Tracked]
    public class WaterDroplet : Entity
    {
        private Player player;
        private const float MaxSpeed = 230f;
        private Rectangle PlayerRectangle = new(0, 0, 0, 0);
        private float wait;
        private float startWait = Calc.Random.Range(0.1f, 0.5f);
        private ParticleSystem system;
        private Water water;
        private float InitialProgress;
        private Vector2 End;
        private Vector2 Start;
        private bool wcol;
        private enum Dir
        {
            Right = 0,
            Up = 1,
            Left = 2,
            Down = 3
        }
        private ParticleType Drip = new ParticleType
        {
            Size = 1f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.1f,
            LifeMax = 0.5f,
            SpeedMin = 20f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.2f,
            FadeMode = ParticleType.FadeModes.Linear,
            Friction = 0
        };
        private Dir MoveDirection;
        private float Direction(Dir direction)
        {
            return (MathHelper.Pi / -2f) * (float)direction;
        }
        public WaterDroplet(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            if (data.Bool("randomWaitTime"))
            {
                if (Calc.Random.Range(1, 3) == 1)
                {
                    wait = Calc.Random.Range(0.5f, 1.5f);
                }
                else
                {
                    wait = Calc.Random.Range(1.5f, 5);
                }
            }
            else
            {
                wait = data.Float("waitTime");
            }
            MoveDirection = data.Enum<Dir>("direction");
            Drip.Color = Color.Lerp(data.HexColor("baseColor", Color.Blue), Color.LightBlue, 0.7f);
            Drip.Color2 = Color.Lerp(data.HexColor("baseColor", Color.Blue), Color.LightBlue, 0.3f);
            Collider = new Hitbox(1, 1);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(system = new ParticleSystem(-1, 4));
            player = (scene as Level).Tracker.GetEntity<Player>();
            if (player is not null)
            {
                PlayerRectangle = new Rectangle((int)(player.X - player.Width / 2), (int)(player.Y - player.Height - 5), (int)(player.Width), (int)(player.Height + 5));
            }
            SetLimits();
            Add(new Coroutine(DropletJourney()));
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Collider is not null) Draw.Rect(Collider, Color.Red);
            Draw.Rect(End.X, End.Y, 1, 1, Color.Green);
            Draw.Rect(Start.X, Start.Y, 1, 1, Color.Magenta);
        }
        private void SetLimits()
        {
            SetStart(true);
            SetEnd();
        }
        private void SetStart(bool snap)
        {
            Vector2 check = Position;
            Vector2 amount = MoveDirection switch
            {
                Dir.Up => -Vector2.UnitY,
                Dir.Down => Vector2.UnitY,
                Dir.Left => -Vector2.UnitX,
                Dir.Right => Vector2.UnitX,
                _ => Vector2.Zero
            };
            if (snap)
            {
                while (CollideCheck<Water>(check))
                {
                    check += amount;
                }
            }
            Start = check;
            Position = Start;
        }
        private void SetEnd()
        {
            Vector2 check = Position;
            Vector2 amount = MoveDirection switch
            {
                Dir.Up => -Vector2.UnitY,
                Dir.Down => Vector2.UnitY,
                Dir.Left => -Vector2.UnitX,
                Dir.Right => Vector2.UnitX,
                _ => Vector2.Zero
            };
            if (Scene is not Level level) return;
            bool outside = false;
            while (!CollideCheck<Solid>(check)) //while no collision found
            {
                if (!level.Bounds.Contains(check.ToPoint())) //if outside the level
                {
                    outside = true;
                    break;
                }
                check += amount; //move in MoveDirection direction
                if (CollideFirst<Water>(check) is Water water)
                {
                    wcol = true;
                    this.water = water;
                    break;
                }
            }
            if (outside)
            {
                check += amount * 32; //extend the end point to offscreen
            }
            End = check;
        }
        private Vector2 DirectionAmount(float amount)
        {
            float x = MoveDirection switch
            {
                Dir.Left => -1,
                Dir.Right => 1,
                _ => 0
            };
            float y = MoveDirection switch
            {
                Dir.Up => -1,
                Dir.Down => 1,
                _ => 0
            };
            return new Vector2(x, y) * amount;
        }
        private IEnumerator DropletJourney()
        {
            bool playerCollide = false;
            Vector2 collideOffset = Vector2.Zero;
            while (true)
            {
                yield return wait + Calc.Random.Range(0.2f, 0.4f);
                Position = Start;
                Vector2 amount = DirectionAmount(1);
                //Slow ease before drop
                for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                {
                    system.Emit(Drip, 1, Start - amount * 3 + amount * Ease.SineOut(i), Vector2.Zero, Direction(MoveDirection));
                    yield return null;
                }
                Vector2 pos = Start - amount;
                float lerp = 0.55f;
                Vector2 distance = pos - End;
                Vector2 sign = new Vector2(Math.Sign(distance.X), Math.Sign(distance.Y));
                while (pos != End)
                {
                    distance = pos - End;
                    if (Math.Sign(distance.X) != sign.X || Math.Sign(distance.Y) != sign.Y)
                    {
                        break;
                    }
                    if (CollideRect(PlayerRectangle, pos))
                    {
                        playerCollide = true;
                        collideOffset = DirectionAmount(-5);
                        yield return null;
                        break;
                    }
                    pos = Calc.Approach(pos, End, MaxSpeed * Engine.DeltaTime * Ease.QuadIn(lerp));
                    system.Emit(Drip, 1, pos, Vector2.Zero);
                    lerp += Engine.DeltaTime;
                    yield return null;
                }
                //Splash and play audio
                if (!playerCollide)
                {
                    pos = End;
                    Audio.Play("event:/PianoBoy/Droplets/drip", pos);
                    if (wcol && water is not null)
                    {
                        switch (MoveDirection)
                        {
                            case Dir.Down: water.TopSurface?.DoRipple(pos, 0.7f); break;
                            case Dir.Up: water.BottomSurface?.DoRipple(pos, 0.7f); break;
                            case Dir.Left:
                                if (water is ColoredWater)
                                {
                                    (water as ColoredWater).RightSurface?.DoRipple(pos, 0.7f);
                                }
                                break;
                            case Dir.Right:
                                if (water is ColoredWater)
                                {
                                    (water as ColoredWater).LeftSurface?.DoRipple(pos, 0.7f);
                                }
                                break;
                        }
                    }

                }
                for (int i = 0; i < 3; i++)
                {
                    Drip.Direction = (MathHelper.Pi / -2f) * (Opposite() + (float)Dir.Left);
                    system.Emit(Drip, 1, pos + collideOffset, Vector2.One * 5);
                    Drip.Direction = (MathHelper.Pi / -2f) * (Opposite() + (float)Dir.Right);
                    system.Emit(Drip, 1, pos + collideOffset, Vector2.One * 5);
                    yield return null;
                }
            }
        }
        private float Opposite()
        {
            return MoveDirection switch
            {
                Dir.Up => (float)Dir.Down,
                Dir.Down => (float)Dir.Up,
                Dir.Left => (float)Dir.Right,
                Dir.Right => (float)Dir.Left,
                _ => 0
            };
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player)
            {
                return;
            }
            PlayerRectangle.X = (int)(player.X - player.Width / 2);
            PlayerRectangle.Y = (int)(player.Y - player.Height - 5);
            PlayerRectangle.Height = (int)(player.Height + 5);
            PlayerRectangle.Width = (int)player.Width;
        }
    }
}
