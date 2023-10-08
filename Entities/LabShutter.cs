using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

// PuzzleIslandHelper.LabShutter
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabShutter")]
    [Tracked]
    public class LabShutter : Solid
    {
        private int Segments = 8;
        private Sprite Sprite;
        public static Rectangle Bounds;
        private ParticleType Sparks = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("fa7f00"),
            Color2 = Calc.HexToColor("ffbc47"),
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.Pi / 2f,
            DirectionRange = MathHelper.PiOver2,
            LifeMin = 0.06f,
            LifeMax = 0.5f,
            SpeedMin = 20f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 2f
        };
        private ParticleSystem system;
        public LabShutter(EntityData data, Vector2 offset)
           : base(data.Position + offset, data.Width, data.Height, false)
        {
            AddTag(Tags.TransitionUpdate);
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/shutter/");
            Sprite.AddLoop("middle", "middle", 0.1f);
            
            Sprite.Play("middle");
            Add(Sprite);
        }
        private void SparkParticles(Vector2 Center)
        {
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;

                particlesBG.Depth = Depth + 1;
                for (int i = 0; i < 120; i += 30)
                {
                    particlesBG.Emit(Sparks, 2, Center, Vector2.One * 7f, MathHelper.Pi / 2);
                }
        }
        private IEnumerator SpriteRoutine(bool instant)
        {
            
            while (!SceneAs<Level>().Session.GetFlag("lab_shutter") && !instant)
            {
                yield return null;
            }
            Remove(Sprite);
            for (int i = 0; i < Segments; i++)
            {
                bool cont = false;
                Sprite Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/shutter/");
                Sprite.AddLoop("middle", "middle", 0.1f);
                Sprite.AddLoop("end", "end", 0.1f);
                Sprite.AddLoop("extended", "extended", 0.1f);
                Sprite.Add("extend", "extend", 0.05f, "extended");
                Sprite.Position.X = i * 32;

                Add(Sprite);
                if (instant)
                {
                    Sprite.Play("extended");
                    cont = true;
                    //Collider.Width = 32 * Segments - 1;
                }
                else
                {
                    Sprite.Play("extend");
                    Sprite.OnLastFrame = (string s) =>
                    {
                        if (s == "extend" || s == "extended")
                        {
                            SpriteInPlace(Sprite);
                            cont = true;
                        }
                    };
                }


                while (!cont)
                {
                    yield return null;
                }
                Collider.Width += 32;
            }
            yield return null;
        }
        private void SpriteInPlace(Sprite sprite)
        {
            //play sound
            SparkParticles(sprite.Center + Position);
            //Emit spark particles or something
        }
        public override void Update()
        {
            base.Update();
            Bounds = Collider.Bounds;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Collider = new Hitbox(32, Sprite.Height);
            Collider.Height -= 12;
            Collider.Position.Y += 6;
            Bounds = Collider.Bounds;
            
            Add(new Coroutine(SpriteRoutine((scene as Level).Session.GetFlag("lab_shutter"))));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
    }
}