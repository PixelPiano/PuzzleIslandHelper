using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.CommunalHelper.Entities;
using Celeste.Mod.CommunalHelper;

// PuzzleIslandHelper.MovingPlatform
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RotatingCassetteBlock")]
    [Tracked]
    public class ffffCassetteBlock : CustomCassetteBlock
    {
        public ffffCassetteBlock(Vector2 position, EntityID id, int width, int height, int index, float tempo, Color? overrideColor)
        : base(position, id, width, height, index, tempo, dynamicHitbox: true, overrideColor)
        {
            //Add(new Coroutine(Sequence()));
        }

        public ffffCassetteBlock(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f), data.HexColorNullable("customColor"))
        {
        }

      /*  private char TileType;

        private TileGrid tiles;

        private Player player;

        private float moveTime;

        private float timer;

        private Vector2 offset;

        private Color outlineColor;

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
            for (int i = 0; i < 4; i++)
            {
                particles.Emit(Dust, 1, Center, new Vector2(Width / 2, Height / 2), 0);
            }
        }
        */
      /*        public RotatingCassetteBlock(EntityData data, Vector2 offset)
                  : base(data.Position + offset, data.Width, data.Height)
                {
                    int newSeed = Calc.Random.Next();
                    Calc.PushRandom(newSeed);
                    Add(tiles = GFX.FGAutotiler.GenerateBox(data.Char("tiletype", '3'), data.Width / 8, data.Height / 8).TileGrid);
                    Calc.PopRandom();
                    Collider = new Hitbox(data.Width, data.Height);
                    Add(new LightOcclude());
                    Add(new TileInterceptor(tiles, false));
                    //TileType = tile;
                    SurfaceSoundIndex = SurfaceIndex.TileToIndex[data.Char("tiletype", '3')];
                    TileType = data.Char("tiletype", '3');
                    OnDashCollide = OnDashed;
                }*/
      /*

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(particles = new ParticleSystem(Depth + 1, 100));
        }
        public override void Update()
        {
            base.Update();
            Dust.SpeedMax = Speed.X - Speed.Y;
            player = Scene.Tracker.GetEntity<Player>();
            if (player is null)
            {
                return;
            }
            offset = tiles.Position;
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            tiles.Position += amount;
        }
        private IEnumerator Sequence()
        {
            timer = 0.4f;
            yield return null;
            timer = 0.5f;
            Vector2 start = Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, moveTime, start: true);
            StartShaking(0.3f);
            ShakeSfx();
            Add(tween);
            for (float i = 0; i < moveTime; i += Engine.DeltaTime)
            {
                AppearParticles();
                yield return null;
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
        }*/
    }
}