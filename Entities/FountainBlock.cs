using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FountainBlock")]
    [Tracked]
    public class FountainBlock : Entity
    {
        public class Fountain : Entity
        {
            public static readonly MTexture Texture = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/fountainBroken"];
            public Vector2 offset;
            public Fountain(Vector2 position, float width) : base(position)
            {
                Depth = 9000;
                offset = new Vector2(width / 2 - Texture.Width / 2, -Texture.Height);
                AddTag(Tags.TransitionUpdate);
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position + offset, Color.White);
            }
        }
        public class Light : Entity
        {
            private readonly MTexture Texture = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/light"];
            private readonly MTexture BackTexture = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/lightBack"];
            public ParticleType GlassShards = new ParticleType()
            {
                SpeedMax = 20f,
                SpeedMin = 5f,
                Direction = -Vector2.UnitY.Angle(),
                DirectionRange = 15f.ToRad(),
                LifeMin = 1,
                LifeMax = 3,
                FadeMode = ParticleType.FadeModes.None,
                RotationMode = ParticleType.RotationModes.SameAsDirection
            };
            private int CrackFrame = -1;
            private Color TexColor;
            private float BackOpacity;
            public Vector2 offset;
            public bool Broken;

            public Light(Vector2 position, float width) : base(position)
            {
                offset = new Vector2(width / 2 - Texture.Width / 2, 0);
                Depth = -13001;
                Tween colorTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 4, true);
                colorTween.OnUpdate = (Tween t) =>
                {
                    TexColor = Color.Lerp(Color.White, Color.Black, t.Eased / 4);
                    BackOpacity = t.Eased / 2;
                };
                Collider = new Hitbox(width, BackTexture.Height);
                Add(colorTween);
                AddTag(Tags.TransitionUpdate);
            }
            public void EmitGlassShards()
            {
                if (Scene is not Level level) return;
                ParticleSystem system = level.ParticlesBG;
                for (int i = 0; i < 12; i++)
                {
                    Vector2 pos = TopCenter + Vector2.UnitX * Calc.Random.Range(-8, 8);
                    Vector2 accell = new Vector2((pos.X - TopCenter.X) / 8, 1);
                    GlassShards.Acceleration = accell * 10;
                    system.Emit(GlassShards, pos);
                }
            }
            public override void Render()
            {
                base.Render();
                if (!Broken)
                {
                    Draw.SpriteBatch.Draw(BackTexture.Texture.Texture_Safe, Position + offset, Color.White * BackOpacity);
                }
                if (CrackFrame >= 0 && CrackFrame < 6)
                {
                    MTexture crack = GFX.Game["objects/PuzzleIslandHelper/fountainBlock/lightCrack0" + CrackFrame];
                    Color c = CrackFrame == 5 ? Color.Gray : Color.White;
                    Draw.SpriteBatch.Draw(crack.Texture.Texture_Safe, Position + offset, c);
                }
                else
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position + offset, TexColor);
                }

            }
            public IEnumerator CrackLight()
            {
                float between = 0.05f;
                CrackFrame = 0;
                yield return 0.8f;
                CrackFrame = 1;
                yield return between;
                CrackFrame = 2;
                yield return 1f;
                CrackFrame = 3;
                yield return 0.2f + between;
                CrackFrame = 4;
                yield return 1;
                Break(false);
                yield return null;
            }
            public void Break(bool instant)
            {
                Broken = true;
                CrackFrame = 5;
                if (!instant)
                {
                    //play sound
                    EmitGlassShards();
                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
            }
        }
        private Fountain fountain;
        private Light light;
        private bool Opened
        {
            get
            {
                return PianoModule.Session.OpenedFountain;
            }
            set
            {
                if (Scene is not Level level) return;
                level.Session.SetFlag("fountainPassage", value);
                PianoModule.Session.OpenedFountain = value;
            }
        }

        public FountainBlock(EntityData data, Vector2 offset)
          : base(data.Position + offset)
        {
            Depth = -13000;
            Collider = new Hitbox(data.Width, data.Height);
            Visible = false;
            fountain = new Fountain(Position, Width);
            light = new Light(Position, Width);
            AddTag(Tags.TransitionUpdate);
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(fountain, light);
            if (PianoModule.Session.OpenedFountain)
            {
                Open();
            }
        }

        public IEnumerator OpenPassage()
        {
            yield return 0.6f;
            yield return light.CrackLight();
            Open();
            yield return 1f;
        }
        public void Open()
        {
            Opened = true;
            light.Break(true);
        }
        public override void Render()
        {
            base.Render();
        }
    }
}