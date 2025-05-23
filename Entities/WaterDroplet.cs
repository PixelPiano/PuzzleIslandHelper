using Celeste.Mod.Entities;
using Celeste.Mod.PandorasBox;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WaterDroplet")]
    [Tracked]
    public class WaterDroplet : Entity
    {
        private const float MaxSpeed = 350f;
        private Rectangle PlayerRectangle = new(0, 0, 0, 0);
        private float wait;
        private bool randomWaitTime;
        private float delay;
        private ParticleSystem system;
        private Vector2 End;
        private Vector2 Start;
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
        : base(data.Position + offset + Vector2.One)
        {

            MoveDirection = data.Enum<Dir>("direction");
            randomWaitTime = data.Bool("randomWaitTime");
            if (!randomWaitTime)
            {
                wait = data.Float("interval");
            }
            delay = data.Float("delay");
            Drip.Color = Color.Lerp(data.HexColor("baseColor", Color.Blue), Color.LightBlue, 0.7f);
            Drip.Color2 = Color.Lerp(data.HexColor("baseColor", Color.Blue), Color.LightBlue, 0.3f);
            Collider = new Hitbox(1, 1);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (randomWaitTime) wait = Calc.Random.Chance(0.3f) ? Calc.Random.Range(0.5f, 1.5f) : Calc.Random.Range(1.5f, 5);
            scene.Add(system = new ParticleSystem(-1, 4));
            if (scene.GetPlayer() is Player player)
            {
                PlayerRectangle.Create(player.X - player.Width / 2, player.Y - player.Height - 5, player.Width, player.Height + 5);
            }
            SetLimits();
            Add(new Coroutine(DropletJourney()));
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
                //snaps the droplet to the directional surface of any water it's touching
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
            if (Scene is not Level level) return;
            Vector2 start = Position;
            Vector2 end = MoveDirection switch
            {
                Dir.Up => new Vector2(start.X, level.Bounds.Top - 16),
                Dir.Down => new Vector2(start.X, level.Bounds.Bottom + 16),
                Dir.Left => new Vector2(level.Bounds.Left - 16, start.Y),
                Dir.Right => new Vector2(level.Bounds.Right + 16, start.Y),
                _ => start
            };
            End = PianoUtils.DoRaycast(level, start, end) ?? end;
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
            Vector2 collideOffset = Vector2.Zero;
            Water collidedWater = null;
            bool collidedWithPlayer = false;
            bool collidedWithWater = false;
            yield return delay;
            while (true)
            {
                yield return wait + (randomWaitTime ? Calc.Random.Range(0.2f, 0.4f) : 0);
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
                        collidedWithPlayer = true;
                        break;
                    }
                    else if (CollideFirst<Water>(pos) is Water water)
                    {
                        collidedWater = water;
                        collidedWithWater = true;
                        break;
                    }
                    pos = Calc.Approach(pos, End, MaxSpeed * Engine.DeltaTime * Ease.QuadIn(lerp));
                    system.Emit(Drip, 1, pos, Vector2.Zero);
                    lerp += Engine.DeltaTime;
                    yield return null;
                }
                if (collidedWithPlayer || collidedWithWater)
                {
                    collideOffset = DirectionAmount(-5);
                }
                //Splash and play audio
                if (collidedWithWater || (!collidedWithWater && !collidedWithPlayer))
                {
                    pos = End;
                    Audio.Play("event:/PianoBoy/Droplets/drip", pos);
                    if (collidedWater is not null)
                    {
                        switch (MoveDirection)
                        {
                            case Dir.Down: collidedWater.TopSurface?.DoRipple(pos, 0.7f); break;
                            case Dir.Up: collidedWater.BottomSurface?.DoRipple(pos, 0.7f); break;
                            case Dir.Left:
                                if (collidedWater is ColoredWater)
                                {
                                    (collidedWater as ColoredWater).RightSurface?.DoRipple(pos, 0.7f);
                                }
                                break;
                            case Dir.Right:
                                if (collidedWater is ColoredWater)
                                {
                                    (collidedWater as ColoredWater).LeftSurface?.DoRipple(pos, 0.7f);
                                }
                                break;
                        }
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    Drip.Direction = (MathHelper.Pi / -2f) * (OppositeDirection() + (float)Dir.Left);
                    system.Emit(Drip, 1, pos + collideOffset, Vector2.One * 5);
                    Drip.Direction = (MathHelper.Pi / -2f) * (OppositeDirection() + (float)Dir.Right);
                    system.Emit(Drip, 1, pos + collideOffset, Vector2.One * 5);
                    yield return null;
                }
            }
        }
        private float OppositeDirection() => ((int)MoveDirection + 2) % 4;
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
