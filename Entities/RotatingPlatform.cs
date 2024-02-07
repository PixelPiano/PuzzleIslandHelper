using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System.Collections;
// PuzzleIslandHelper.MovingPlatform
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RotatingPlatform")]
    [Tracked]
    public class RotatingPlatform : Solid
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

        private float Rotation = 0;

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

        private float rotationTarget = 0;

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

        private MTexture[,] mTiles;

        private Matrix matrix;
        private InvisibleBarrier a;
        private Level l;
        private ParticleSystem particles;
        public VirtualRenderTarget Block;
        #region Done
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
            //Angle = previousState ? (startPosition - node).Angle() : (node - startPosition).Angle();
            for (int i = 0; i < 4; i++)
            {
                particles.Emit(Dust, 1, Center, new Vector2(Width / 2, Height / 2), Angle);
            }
        }
 
        public RotatingPlatform(EntityData data, Vector2 offset)
          : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            #region moving platform
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
            tiles.Visible = false;
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[data.Char("tiletype", '3')];
            flag = data.Attr("flag");
            canReturn = data.Bool("canReturn");
            TileType = data.Char("tiletype", '3');
            OnDashCollide = OnDashed;
            Add(new BeforeRenderHook(BeforeRender));
            #endregion
        }

        #region Rendering
        public override void Render()
        {
            base.Render();
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                return;
            }
            Draw.SpriteBatch.Draw(
                Block, Center + tiles.Position,  null, 
                Color.White, Rotation,
                new Vector2(Width/2,Height/2), 
                1, SpriteEffects.None, 0);
        }

        public void RenderAt(Vector2 position)
        {
            if (tiles.Alpha <= 0f)
            {
                return;
            }
            int tileWidth = tiles.TileWidth;
            int tileHeight = tiles.TileHeight;
            Color color = tiles.Color * tiles.Alpha;
            Vector2 position2 = new Vector2(position.X, position.Y);
            for (int i = 0; i < tiles.Tiles.Columns; i++)
            {
                for (int j = 0; j < tiles.Tiles.Rows; j++)
                {
                    MTexture mTexture = tiles.Tiles[i, j];
                    if (mTexture != null)
                    {
                        Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position2, mTexture.ClipRect, color);
                    }

                    position2.Y += tileHeight;
                }

                position2.X += tileWidth;
                position2.Y = position.Y;
            }
        }
        private void BeforeRender()
        {
            if (Scene as Level is null)
            {
                return;
            }
            l = Scene as Level;
            EasyRendering.DrawToObject(Block, () => RenderAt(tiles.Position), l, clear: true, useIdentity: true);
        }
        #endregion
        #endregion
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            prevPosition = Position;
            Block = VirtualContent.CreateRenderTarget("FancyBlock", (int)Width, (int)Height);
            scene.Add(particles = new ParticleSystem(Depth + 1, 100));
            previousState = false;
            /*            switch (MoveMeth)
            {
                case MoveMethod.OnFlag:
                    previousState = SceneAs<Level>().Session.GetFlag(flag);
                    if (SceneAs<Level>().Session.GetFlag(flag))
                    {
                        //MoveTo(node);
                    }
                    break;
            }*/
        }
        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                return;
            }
            if (HasPlayerRider() && !moving)
            {
                Add(new Coroutine(Sequence()));
            }
            return;
            /*
            Dust.SpeedMax = Speed.X - Speed.Y;
            if (!moving)
            {
                Speed = Vector2.Zero;
            }
            offset = newTiles.Position;
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
            }
            switch (MoveMeth)
            {
                case MoveMethod.OnFlag:
                    Valid = previousState != SceneAs<Level>().Session.GetFlag(flag);
                    break;

                case MoveMethod.OnPlayerNear:
                    Valid =previousState ? !PlayerNear : PlayerNear;
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
                Add(new Coroutine(FallSequence()));
            }
            */
        }
        private IEnumerator Sequence()
        {
            if (moving)
            {
                yield break;
            }
            moving = true;
            float _rotation = Rotation;
            StartShaking(0.2f);
            yield return 0.2f;
            ShakeSfx();
            timer = 0.4f;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Easer, moveTime, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                Rotation = Calc.Approach(_rotation, _rotation + MathHelper.PiOver2, t.Eased);
            };
            Add(tween);
            yield return moveTime + 0.05f;
            MovedOnce = true;
            moving = false;
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
        }
        private void DrawRect(float x, float y, float width, float height, int thickness)
        {
            for (int i = 0; i < thickness; i++)
            {
                Draw.HollowRect(x + offset.X - i, y + offset.Y - i, width + i * 2, height + i * 2, outlineColor * timer);
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