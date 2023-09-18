using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.CherryHelper;
using FMOD.Studio;
using System;
using Microsoft.Xna.Framework.Audio;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using System.Collections.Generic;
using System.Linq;
// PuzzleIslandHelper.MovingPlatform
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/MovingPlatform")]
    [Tracked]
    public class MovingPlatform : Solid
    {

        private char TileType;
        private float Volume;
        private bool ForPuzzle;
        private int BlocksInLevel;
        private string flag;
        private bool OrigFlagState;

        private float moveTime;

        private float timer;

        private float Angle;
        private bool LowerVolume;

        private Color outlineColor;

        private Rectangle Detector;


        private Vector2 node; //Position to move to

        private Vector2 DetectRadius;

        private Vector2 startPosition;

        private Vector2 offset;

        private Vector2 prevPosition = Vector2.Zero;


        private bool Valid = false;

        private bool invertOnPlayer;

        private bool moving = false;

        private bool previousState;

        private bool canReturn; //overrides: nothing

        private bool MovedOnce = false;

        private bool outlineDestination;

        private bool affectCamera = true;

        private bool cameraRemoved = false;
        private Vector2 speed;
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

        private int buffer;


        private TileGrid tiles;

        private Player player;

        private Ease.Easer Easer;

        private Level l;

        private CameraAdvanceTargetTrigger Trigger;

        private EntityData Data;
        private Vector2 Offset;

        #region Enums
        private MoveMethod MoveMeth; //jesse
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
        #endregion
        private bool Gentle;
        #region Particles

        private ParticleType P_Arrive = new ParticleType
        {
            Size = 1f,
            Color = Color.White * 0.8f,
            Color2 = Color.Gray * 0.8f,
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = MathHelper.PiOver2,
            LifeMin = 0.1f,
            LifeMax = 0.5f,
            SpeedMin = 0.1f,
            SpeedMax = 0.5f,
            SpeedMultiplier = 0.5f,
            FadeMode = ParticleType.FadeModes.InAndOut,
        };
        private ParticleSystem particles;

        private ParticleType Dust = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/line00"],
            Size = 1f,
            Color = Color.Gray * 0.25f,
            Color2 = Color.White * 0.25f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 1f,
            LifeMax = 1.5f,
            SpeedMin = 2f,
            SpeedMultiplier = 2f,
            FadeMode = ParticleType.FadeModes.Linear,
            RotationMode = ParticleType.RotationModes.SameAsDirection
        };
        private void AppearParticles()
        {
            if (buffer == 0 && !ForPuzzle)
            {
                Angle = previousState ? (startPosition - node).Angle() : (node - startPosition).Angle();

                particles.Emit(Dust, 1, Center, new Vector2(Width / 2, Height / 2), Angle + MathHelper.Pi);
                buffer = 2;
            }
            buffer--;
        }
        private void ArriveParticles()
        {
            if (!Gentle)
            {
                for (int i = 0; i < 360; i += 90)
                {
                    particles.Emit(P_Arrive, 10, Center, new Vector2(Width, Height), P_Arrive.Direction + MathHelper.ToDegrees(i));
                }
            }
        }
        #endregion


        private List<ExpandingRect> Rects = new();
        public MovingPlatform(EntityData data, Vector2 offset)
          : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            Data = data;
            Offset = offset;
            Gentle = data.Bool("gentleMode");
            Volume = data.Float("volume", 1);
            ForPuzzle = data.Bool("isForPIPuzzle");
            affectCamera = data.Bool("cameraLookAhead", true);
            invertOnPlayer = data.Bool("invertOnPlayerDetect", false);
            outlineColor = data.HexColor("outline", Color.Black);
            MoveMeth = data.Enum("moveMethod", MoveMethod.OnFlag);
            detectMode = data.Enum("playerDetectArea", DetectMode.Destination);
            DetectRadius = new Vector2(data.Float("detectRadiusX"), data.Float("detectRadiusY"));
            Detector = new Rectangle((int)(Position.X - DetectRadius.X), (int)(Position.Y - DetectRadius.Y), (int)(Width + DetectRadius.X * 2), (int)(Height + DetectRadius.Y * 2));
            Easer = SetEase(data.Enum("ease", Easing.Linear), data.Enum("easeDirection", EasingInOut.InOut));
            moveTime = data.Float("moveTime");
            LowerVolume = moveTime <= 0.4f;
            Depth = 1;
            startPosition = Position;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
            Calc.PopRandom();
            Collider = new Hitbox(data.Width, data.Height);
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, false));
            TileType = data.Char("tile", '3');
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[data.Char("tiletype", '3')];
            node = data.Nodes[0] + offset;
            flag = data.Attr("flag");
            canReturn = data.Bool("canReturn");
            TileType = data.Char("tiletype", '3');
            OnDashCollide = OnDashed;
            outlineDestination = data.Bool("outlineDestination");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            BlocksInLevel = (scene as Level).Tracker.GetEntities<MovingPlatform>().Count;
            prevPosition = Position;
            scene.Add(particles = new ParticleSystem(Depth + 1, 100));

            previousState = false;
            if (outlineDestination)
            {
                AssistRectangle a = new AssistRectangle(node, (int)Width, (int)Height, Color.LightBlue);
                a.Depth = 3;
                scene.Add(a);
            }
            if (affectCamera)
            {
                scene.Add(Trigger = new CameraAdvanceTargetTrigger(Data, Offset));
                Trigger.LerpStrength = Vector2.Zero;
                Trigger.Collider = new Hitbox(Width + 16, Height + 16, -8, -8);
                Trigger.Active = false;
            }
            switch (MoveMeth)
            {
                case MoveMethod.OnFlag:
                    previousState = SceneAs<Level>().Session.GetFlag(flag);
                    OrigFlagState = previousState;
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
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (MoveMeth == MoveMethod.OnFlag)
            {
                (scene as Level).Session.SetFlag(flag, OrigFlagState);
            }
        }
        private IEnumerator ExpandingRects()
        {
            if (ForPuzzle)
            {
                yield break;
            }
            int spacingMax = 8;
            Vector2 pos = Position;
            Color baseColor = Color.Purple;
            for (int i = 0; i < 4; i++)
            {
                ExpandingRect a = new ExpandingRect(pos, Width, Height, baseColor, 5);
                SceneAs<Level>().Add(a);

                for (int j = 0; j < spacingMax; j++)
                {
                    yield return null;
                }
            }
            yield return null;
        }
        public override void Update()
        {
            base.Update();
            if (Scene as Level is null)
            {
                return;
            }
            l = Scene as Level;
            if (ForPuzzle)
            {
                Visible = InvertOverlay.State;
            }

            Dust.SpeedMax = speed.X - speed.Y;
            if (!moving)
            {
                speed = Vector2.Zero;
            }
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                return;
            }
            offset = tiles.Position;
            if (Gentle)
            {
                foreach (ExpandingRect r in Scene.Tracker.GetEntities<ExpandingRect>())
                {
                    r.offset = offset;
                }
            }
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
            StartShaking(Gentle ? 0.1f : 0.3f);
            if (!Gentle)
            {
                ShakeSfx();
            }
            tween.OnStart = delegate (Tween t)
            {
                if (affectCamera && !cameraRemoved)
                {
                    Trigger.Position = Position;
                    Trigger.Target = node - Vector2.UnitX * 90;
                    Trigger.Active = true;
                }
            };
            tween.OnUpdate = delegate (Tween t)
            {

                if (affectCamera && !cameraRemoved)
                {
                    Trigger.Position = Position;
                    Trigger.LerpStrength = Vector2.One * Calc.LerpClamp(0, 1.5f, t.Eased);
                }

                speed = prevPosition - Position;
                prevPosition = Position;
                Vector2 Lerp = Vector2.Lerp(start, node, t.Eased);
                MoveTo(Lerp);
            };
            tween.OnComplete = delegate (Tween t)
            {

                if (affectCamera && !cameraRemoved)
                {
                    Trigger.Position = node;
                    Trigger.Active = false;
                }
                if (!Gentle || ForPuzzle)
                {
                    ImpactSfx();
                }
                else
                {
                    Add(new Coroutine(ExpandingRects()));
                }
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                ArriveParticles();

                if (!canReturn && affectCamera && !cameraRemoved)
                {
                    Trigger.Active = false;
                    cameraRemoved = true;
                }
            };

            Add(tween);

            for (float i = 0; i < moveTime; i += Engine.DeltaTime)
            {
                if (InvertOverlay.State)
                {
                    tween.Active = false;

                    while (InvertOverlay.State)
                    {
                        yield return null;
                    }
                    tween.Active = true;
                }
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
        public override void OnShake(Vector2 amount)
        {
            if (!InvertOverlay.State)
            {
                base.OnShake(amount);
                tiles.Position += amount;
            }
        }
        private void DrawRect(float x, float y, float width, float height, int thickness)
        {
            if (!Gentle)
            {
                for (int i = 0; i < thickness; i++)
                {
                    Draw.HollowRect(x + offset.X - i, y + offset.Y - i, width + i * 2, height + i * 2, outlineColor * timer);
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (moving)
            {
                DrawRect(X, Y, Width, Height, 3);
            }
        }

        private void ImpactSfx()
        {
            Add(new Coroutine(WaitThenPlay(true)));

        }
        private void ShakeSfx()
        {
            Add(new Coroutine(WaitThenPlay(false)));

        }
        private IEnumerator WaitThenPlay(bool impact)
        {
            while (InvertOverlay.State)
            {
                yield return null;
            }
            float value = Volume;
            if (ForPuzzle)
            {
                if (impact)
                {
                    Audio.Play("event:/PianoBoy/movingPlatformImpact", BottomCenter, "VolumeAdjust",0.7f);
                }
                yield break;
            }
            if (impact)
            {
                Audio.Play("event:/PianoBoy/movingPlatformImpact", BottomCenter, "VolumeAdjust", 1 - value);
            }
            else
            {
                Audio.Play("event:/PianoBoy/movingPlatformShake", Center, "VolumeAdjust", 1 - value);
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

    [CustomEntity("PuzzleIslandHelper/ExpandingRect")]
    [Tracked]
    public class ExpandingRect : Entity
    {
        public Color Color;
        public int MaxThickness;
        public int CurrentThickness = 1;
        private double Percent;
        public Vector2 offset;
        public ExpandingRect(Vector2 position, float width, float height, Color color, int maxThickness)
        {
            X = position.X;
            Y = position.Y;
            Depth = 2;
            Collider = new Hitbox(width, height);
            Color = color;
            MaxThickness = maxThickness;
        }
        public override void Render()
        {
            base.Render();

            if (MaxThickness > 1)
            {
                DrawSelf(offset, CurrentThickness);
            }
            else
            {
                DrawSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            if (Percent < 1f)
            {
                X--;
                Y--;
                Collider.Width += 2;
                Collider.Height += 2;

                Percent += Engine.DeltaTime;
                CurrentThickness = (int)(MaxThickness * (1 - Percent));
            }
            else
            {
                RemoveSelf();
            }
        }
        public ExpandingRect(Collider collider, Color color, int maxThickness) : this(collider.Position, collider.Width, collider.Height, color, maxThickness) { }
        public void DrawSelf()
        {
            Draw.HollowRect(X, Y, Width, Height, Color * (1 - (float)Percent));
        }
        public void DrawSelf(Vector2 offset, int thickness)
        {
            for (int i = 0; i < thickness; i++)
            {
                Draw.HollowRect(X + offset.X - i, Y + offset.Y - i, Width + i * 2, Height + i * 2, Color * (1 - (float)Percent));
            }
        }
    }
}