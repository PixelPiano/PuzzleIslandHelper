using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.MovingPlatform
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MovingPlatform")]
    [Tracked]
    public class MovingPlatform : Solid
    {
        private char TileType;

        private TileGrid tiles;

        private Player player;

        private bool MovedOnce = false;

        private Vector2 node; //Position to move to

        private string flag;

        private bool previousState;

        private bool canReturn; //overrides: nothing

        private Ease.Easer Easer;

        private float moveTime;

        private bool invertOnPlayer;

        private Vector2 DetectRadius;

        private bool moving = false;

        private Vector2 startPosition;

        private Rectangle Detector;

        private MoveMethod MoveMeth; //jesse

        private float timer;

        private Vector2 offset;

        private Color outlineColor;

        private float Angle;

        private Vector2 prevPosition = Vector2.Zero;

        private bool Valid = false;
        private DetectMode detectMode;
        public enum Easing
        {
            Linear,
            Sine,
            Cube,
            Bounce,
            Elastic,
            Expo,
            Quad,
            Quint
        }

        public enum DetectMode
        {
            Position,
            Start,
            Destination
        }
        public enum EasingInOut
        {
            In,
            Out,
            InOut
        }

        public enum MoveMethod
        {
            OnFlag,
            OnPlayerNear,
            OnTouched,
            OnRiding,
            BackAndForth,
            OnDashed
        }
        public enum Direction
        {
            Right,
            Up,
            Left,
            Down
        }
        private bool PlayerNear
        {
            get
            {
                if (player is null)
                {
                    return false;
                }
                return player.CollideRect(Detector);
            }
        }

        private ParticleSystem particles;

        private ParticleType Dust = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/line00"],
            Size = 1f,
            Color = Color.Gray * 0.25f,
            Color2 = Color.White * 0.25f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.1f,
            LifeMax = 0.4f,
            SpeedMin = 0.1f,
            SpeedMultiplier = 0.5f,
            FadeMode = ParticleType.FadeModes.InAndOut,
            RotationMode = ParticleType.RotationModes.SameAsDirection
        };
        private void AppearParticles()
        {
            Angle = previousState ? (startPosition - node).Angle() : (node - startPosition).Angle();
            for (int i = 0; i < 4; i++)
            {
                particles.Emit(Dust, 1, Center, new Vector2(Width / 2, Height / 2), Angle);
            }
        }
        public MovingPlatform(EntityData data, Vector2 offset)
          : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            invertOnPlayer = data.Bool("invertOnPlayerDetect", false);
            outlineColor = data.HexColor("outline", Color.Black);
            MoveMeth = data.Enum("moveMethod", MoveMethod.OnFlag);
            detectMode = data.Enum("playerDetectArea", DetectMode.Destination);
            DetectRadius = new Vector2(data.Float("detectRadiusX"), data.Float("detectRadiusY"));

            Detector = new Rectangle((int)(Position.X - DetectRadius.X), (int)(Position.Y - DetectRadius.Y), (int)(Width + DetectRadius.X * 2), (int)(Height + DetectRadius.Y * 2));
            Easer = SetEase(data.Enum("ease", Easing.Linear), data.Enum("easeDirection", EasingInOut.InOut));
            moveTime = data.Float("moveTime");
            startPosition = Position;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
            Calc.PopRandom();
            Collider = new Hitbox(data.Width, data.Height);
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, false));
            //TileType = tile;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[data.Char("tiletype", '3')];
            node = data.Nodes[0] + offset;
            flag = data.Attr("flag");
            canReturn = data.Bool("canReturn");
            TileType = data.Char("tiletype", '3');
            OnDashCollide = OnDashed;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            prevPosition = Position;
            scene.Add(particles = new ParticleSystem(Depth + 1, 100));
            
            previousState = false;
            switch (MoveMeth)
            {
                case MoveMethod.OnFlag:
                    previousState = SceneAs<Level>().Session.GetFlag(flag);
                    if (SceneAs<Level>().Session.GetFlag(flag))
                    {
                        MoveTo(node);
                    }
                    break;
                case MoveMethod.BackAndForth:
                    Add(new Coroutine(Sequence(node)));
                    break;
            }
        }
        public override void Update()
        {
            base.Update();
            Dust.SpeedMax = Speed.X - Speed.Y;
            if (!moving)
            {
                Speed = Vector2.Zero;
            }
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                return;
            }
            offset = tiles.Position;
            timer = moving && timer > 0 ? timer - Engine.DeltaTime : 0;
            switch (detectMode)
            {
                case DetectMode.Position:
                    Detector.X = (int)(X - DetectRadius.X);
                    Detector.Y = (int)(Y - DetectRadius.Y);
                    break;
                case DetectMode.Start:
                    Detector.X = (int)(startPosition.X - DetectRadius.X);
                    Detector.Y = (int)(startPosition.Y - DetectRadius.Y);
                    break;
                case DetectMode.Destination:
                    Detector.X = (int)(node.X - DetectRadius.X);
                    Detector.Y = (int)(node.Y - DetectRadius.Y);
                    break;
            }
            switch (MoveMeth)
            {
                case MoveMethod.OnFlag:
                    Valid = previousState != SceneAs<Level>().Session.GetFlag(flag);
                    break;

                case MoveMethod.OnPlayerNear:
                    Valid =/* invertOnPlayer ? */previousState ? !PlayerNear : PlayerNear/* : PlayerNear*/;
                    break;

                case MoveMethod.OnTouched:
                    Valid = HasPlayerRider();
                    break;

                case MoveMethod.OnRiding:
                    Valid = HasPlayerOnTop();
                    break;
                case MoveMethod.OnDashed:
                    break;
            }

            if (Valid && !moving && !(MoveMeth == MoveMethod.OnDashed))
            {
                Add(new Coroutine(Sequence(previousState ? startPosition : node)));
            }

        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
        }
        private IEnumerator Sequence(Vector2 node)
        {
            if (moving || (!canReturn && MovedOnce))
            {
                yield break;
            }
            timer = 0.4f;
            moving = true;
            yield return null;
            timer = 0.5f;
            previousState = !previousState;
            Vector2 start = Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Easer, moveTime, start: true);
            StartShaking(0.3f);
            ShakeSfx();
            tween.OnUpdate = delegate (Tween t)
            {
                Speed = prevPosition - Position;
                prevPosition = Position;
                
                MoveTo(Vector2.Lerp(start, node, t.Eased));
            };

            Add(tween);
            for (float i = 0; i < moveTime; i += Engine.DeltaTime)
            {
                AppearParticles();
                yield return null;
            }
            MovedOnce = true;
            moving = false;
            if (MoveMeth == MoveMethod.BackAndForth)
            {
                Add(new Coroutine(Sequence(start)));
            }
            yield return null;
        }
        private void DrawRect(float x, float y, float width, float height, int thickness)
        {
            for (int i = 0; i < thickness; i++)
            {
                Draw.HollowRect(x + offset.X - i, y + offset.Y - i, width + i * 2, height + i * 2, outlineColor * timer);
            }
        }
        public override void Render()
        {
            base.Render();
            //Draw.HollowRect(Detector, Color.Red);
            if (moving)
            {
                DrawRect(X, Y, Width, Height, 3);
            }
        }
        private void ShakeSfx()
        {
            if (TileType == '3')
            {
                Audio.Play("event:/game/01_forsaken_city/fallblock_ice_shake", base.Center);
            }
            else if (TileType == '9')
            {
                Audio.Play("event:/game/03_resort/fallblock_wood_shake", base.Center);
            }
            else if (TileType == 'g')
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
            }
            else
            {
                Audio.Play("event:/game/general/fallblock_shake", base.Center);
            }
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (MoveMeth == MoveMethod.OnDashed && !moving)
            {
                Add(new Coroutine(Sequence(previousState ? startPosition : node)));
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        private Ease.Easer SetEase(Easing easing, EasingInOut easingInOut)
        {
            Ease.Easer output = Ease.Linear;
            switch (easing)
            {
                case Easing.Linear:
                    break;
                case Easing.Sine:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.SineIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.SineOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.SineInOut;
                            break;
                    }
                    break;
                case Easing.Cube:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.CubeIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.CubeOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.CubeInOut;
                            break;
                    }
                    break;
                case Easing.Bounce:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.BounceIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.BounceOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.BounceInOut;
                            break;
                    }
                    break;
                case Easing.Elastic:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.ElasticIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.ElasticOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.ElasticInOut;
                            break;
                    }
                    break;
                case Easing.Expo:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.ExpoIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.ExpoOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.ExpoInOut;
                            break;
                    }
                    break;
                case Easing.Quad:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.QuadIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.QuadOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.QuadInOut;
                            break;
                    }
                    break;
                case Easing.Quint:
                    switch (easingInOut)
                    {
                        case EasingInOut.In:
                            output = Ease.QuintIn;
                            break;
                        case EasingInOut.Out:
                            output = Ease.QuintOut;
                            break;
                        case EasingInOut.InOut:
                            output = Ease.QuintInOut;
                            break;
                    }
                    break;

            }
            return output;
        }
    }
}