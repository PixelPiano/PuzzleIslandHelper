//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
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
                base.Depth = 9500;
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

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new Bg(Position));
        }

        public override void Render()
        {
            base.Render();
            if (buffer != null)
            {
                Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Position + new Vector2((0f - base.Collider.Width) / 2f, (0f - base.Collider.Height) / 2f), Color.White * bufferAlpha);
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
