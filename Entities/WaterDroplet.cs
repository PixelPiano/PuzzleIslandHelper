using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
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
        private bool debugColl;
        private Rectangle PlayerRectangle;
        private float wait;
        private float startWait = Calc.Random.Range(0.1f, 0.5f);
        private bool runBefore;
        private ParticleSystem system;
        private bool InRoutine;
        private Water water;
        private bool NoGround;
        private float InitialProgress;
        private Vector2 Grounded;
        private Vector2 Ceiling;
        private bool wcol;
        private enum Directions
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
        private float Direction(Directions direction)
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
            Drip.Color = Color.Lerp(data.HexColor("baseColor", Color.Blue), Color.LightBlue, 0.7f);
            Drip.Color2 = Color.Lerp(data.HexColor("baseColor", Color.Blue), Color.LightBlue, 0.3f);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(system = new ParticleSystem(-1, 4));
            player = (scene as Level).Tracker.GetEntity<Player>();
            PlayerRectangle = new Rectangle((int)(player.X - player.Width / 2), (int)(player.Y - player.Height - 5), (int)(player.Width), (int)(player.Height + 5));
            SetLimits();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Rect(Collider, Color.Red);
            Draw.Rect(Grounded.X, Grounded.Y, 1, 1, Color.Green);
            Draw.Rect(Ceiling.X, Ceiling.Y, 1, 1, Color.Magenta);
            if (player is not null)
            {
                Draw.Rect(PlayerRectangle, Color.Black);
            }
        }
        private void SetLimits()
        {
            PlaceOnSolid(false);
            PlaceOnSolid(true);
        }
        private void PlaceOnSolid(bool placeOnCeiling = true)
        {
            bool waterCollide = false;
            Level level = Scene as Level;
            if (level is null) { return; }
            int offset = 0;

            Collider = new Hitbox(1, 1);
            bool Condition = true;

            while (!CollideCheck<Solid>())
            {
                if (CollideFirst<Water>() is Water water)
                {
                    this.water = water;
                    waterCollide = true;
                    wcol = true;
                    break;
                }
                Condition = placeOnCeiling ? Collider.AbsoluteBottom > level.Bounds.Top : Collider.AbsoluteTop < level.Bounds.Bottom;
                if (Condition)
                {
                    Position.Y = placeOnCeiling ? Position.Y - 1 : Position.Y + 1;
                }
                else
                {
                    if (!placeOnCeiling)
                    {
                        NoGround = true;
                        offset = 32;
                    }
                    break;
                }
            }
            Position.Y = placeOnCeiling ? Position.Y + 1 : Position.Y - 1;
            if (placeOnCeiling)
            {
                if (!Condition)
                {
                    Ceiling = new Vector2(Position.X, level.Bounds.Top);
                }
                else
                {
                    Ceiling = Position;
                }
            }
            else
            {
                Grounded = Position + (offset * Vector2.UnitY) + (Vector2.UnitY * 8);
                if (waterCollide)
                {
                    Grounded.Y += 4;
                }
            }
        }
        private IEnumerator DropletJourney()
        {
            bool playerCollide = false;
            InRoutine = true;
            if (!runBefore)
            {
                yield return wait;
                runBefore = true;
            }
            Position = Ceiling;
            //Slow ease before drop
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                system.Emit(Drip, 1, Position - Vector2.UnitY + (Vector2.UnitY * Ease.SineOut(i)), Vector2.Zero, Direction(Directions.Down));
                yield return null;
            }
            Vector2 collideOffset = new Vector2(0, 0);
            //Drop until hit floor
            float Distance = MathHelper.Distance(Position.Y, Grounded.Y);
            float Pos = Position.Y;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 3)
            {
                Position.Y = Calc.Approach(Pos, Pos + Distance, Distance * Ease.QuadIn(i));
                system.Emit(Drip, 1, Position, Vector2.Zero);
                if (CollideRect(PlayerRectangle))
                {
                    playerCollide = true;
                    collideOffset.Y = -5f;
                    break;
                }
                yield return null;
            }
            //Splash and play audio
            if(!playerCollide)
            {
                Audio.Play("event:/PianoBoy/Droplets/drip", Position);
            }
            if (wcol && !CollideRect(PlayerRectangle))
            {
                water.TopSurface.DoRipple(Position, 0.7f);
            }
            for (int i = 0; i < 3; i++)
            {
                Drip.Direction = (MathHelper.Pi / -2f) * ((float)Directions.Up + (float)Directions.Left);
                system.Emit(Drip, 1, Position + collideOffset, Vector2.One * 5);
                Drip.Direction = (MathHelper.Pi / -2f) * ((float)Directions.Up + (float)Directions.Right);
                system.Emit(Drip, 1, Position + collideOffset, Vector2.One * 5);
                yield return null;
            }
            //Wait a period of time
            yield return wait + Calc.Random.Range(-0.2f, 0.21f);
            InRoutine = false;
        }
        public override void Update()
        {
            base.Update();
            if (player is not null)
            {
                PlayerRectangle.X = (int)(player.X - player.Width / 2);
                PlayerRectangle.Y = (int)(player.Y - player.Height - 5);
                PlayerRectangle.Height = (int)(player.Height + 5);
                debugColl = CollideRect(PlayerRectangle);
            }
            if (!InRoutine)
            {
                Add(new Coroutine(DropletJourney()));
            }
        }
    }
}
