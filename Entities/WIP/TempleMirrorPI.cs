//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TempleMirrorPI")]
    [Tracked]
    public class TempleMirrorPI : Entity
    {
        public static float Rotation = 0;//15f.ToRad();

        public class Bg : Entity
        {

            public MirrorSurface surface;
            public Vector2[] offsets;


            public List<MTexture> textures;

            public Bg(Vector2 position)
                : base(position)
            {
                Depth = 9500;
                textures = GFX.Game.GetAtlasSubtextures("objects/temple/portal/reflection");
                Vector2 vector = new Vector2(10f, 4f);
                offsets = new Vector2[textures.Count];
                for (int i = 0; i < offsets.Length; i++)
                {
                    offsets[i] = vector + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
                }

                Add(surface = new MirrorSurface());
                surface.OnRender = delegate
                {
                    for (int j = 0; j < textures.Count; j++)
                    {
                        surface.ReflectionOffset = offsets[j];
                        textures[j].DrawCentered(Position, surface.ReflectionColor, 1, Rotation);
                    }
                };

            }

            public override void Render()
            {
                GFX.Game["objects/temple/portal/surface"].DrawCentered(Position, Color.White, 1, Rotation);
            }
        }
        public class Inside : Entity
        {
            public MirrorReflection reflection;
            public const string Path = "objects/PuzzleIslandHelper/templeMirror/symbol";
            public List<MTexture> textures = new()
            {
                GFX.Game[Path + "A"],
                GFX.Game[Path + "B"],
                GFX.Game[Path + "C"],
                GFX.Game[Path + "D"],
                GFX.Game[Path + "E"],
                GFX.Game[Path + "F"]
            };
            public class Ring : Image
            {
                public Vector2 Center;
                public float Radius;
                public Ring(char type, Vector2 center, float radius) : base(GFX.Game[Path + type])
                {
                    Center = center;
                    Radius = radius;
                    CenterOrigin();
                    Position = new Vector2(Width / 2, Height / 2);
                }
                public override void Render()
                {
                    if (Texture != null)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Texture.Draw(RenderPosition.RotateAround(Center, 360 / 8 * i), Origin, Color, Scale, Rotation, Effects);
                        }

                    }
                }
            }
            public Inside(Vector2 position) : base(position)
            {
                Depth = 9499;

                Add(reflection = new MirrorReflection()
                {
                    IgnoreEntityVisible = true
                });
            }
            public override void Render()
            {
                base.Render();

            }
        }
        public TemplePortalTorch leftTorch;
        public TemplePortalTorch rightTorch;
        public VirtualRenderTarget buffer;
        public float bufferAlpha;
        public float bufferTimer;

        public TempleMirrorPI(Vector2 position)
            : base(position)
        {
            Depth = 2000;
            Collider = new Hitbox(120f, 64f, -60f, -32f);
        }

        public TempleMirrorPI(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }
        public void FlashSymbol()
        {

        }
        public IEnumerator Shimmer()
        {
            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new Bg(Position));
            scene.Add(leftTorch = new TemplePortalTorch(Position + new Vector2(-90f, 0f)));
            scene.Add(rightTorch = new TemplePortalTorch(Position + new Vector2(90f, 0f)));
            leftTorch.Add(leftTorch.loopSfx = new SoundSource());
            rightTorch.Add(rightTorch.loopSfx = new SoundSource());
            leftTorch.sprite.Play("lit");
            rightTorch.sprite.Play("lit");
            leftTorch.Add(leftTorch.bloom = new BloomPoint(1f, 16f));
            leftTorch.Add(leftTorch.light = new VertexLight(Color.LightSeaGreen, 0f, 32, 128));
            rightTorch.Add(rightTorch.bloom = new BloomPoint(1f, 16f));
            rightTorch.Add(rightTorch.light = new VertexLight(Color.LightSeaGreen, 0f, 32, 128));
            leftTorch.loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
            rightTorch.loopSfx.Play("event:/game/05_mirror_temple/mainmirror_torch_loop");
        }

        public override void Render()
        {
            base.Render();
            if (buffer != null)
            {
                Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Position + new Vector2((0f - Collider.Width) / 2f, (0f - Collider.Height) / 2f), Color.White * bufferAlpha);
            }
            GFX.Game["objects/PuzzleIslandHelper/templeMirror/goop00"].DrawCentered(Position, Color.White, 1, Rotation);
            GFX.Game["objects/temple/portal/portalframe"].DrawCentered(Position, Color.White, 1, Rotation);
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }
        public void Dispose()
        {
            if (buffer != null)
            {
                buffer.Dispose();
            }

            buffer = null;
        }
    }

}
