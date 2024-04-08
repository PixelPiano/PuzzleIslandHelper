using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.CommunalHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{

    [CustomEntity("PuzzleIslandHelper/GrowingShiftArea")]
    [TrackedAs(typeof(ShiftArea))]
    public class GrowingShiftArea : ShiftArea
    {
        private bool Cracking;
        public class Crack : Entity
        {
            public MTexture[] Textures;
            public int CurrentFrame;
            public Action Action;
            public Crack(Vector2 position, int maxTextures, Action onComplete = null) : base(position)
            {
                Action = onComplete;
                Textures = new MTexture[maxTextures];
                for (int i = 0; i < maxTextures; i++)
                {
                    Textures[i] = GFX.Game["objects/PuzzleIslandHelper/shiftAreaCracks/cracks" + (i<10 ? "0":"") + i];
                }
                Visible = false;
                Depth = -10001;
                Add(new Coroutine(Animate()));
            }
            private IEnumerator Animate()
            {
                if (Textures.Length < 4) yield break;
                for(int i = 0; i<4; i++)
                {
                    CurrentFrame = i;
                    yield return i == 0 ? 0.1f : 0.7f; 
                }
                for(int i = 4; i<Textures.Length; i++)
                {
                    CurrentFrame = i;
                    yield return 0.1f;
                }
                Action?.Invoke();
                RemoveSelf();

            }
            public override void Render()
            {
                base.Render();
                Vector2 offset = new Vector2(Textures[CurrentFrame].Width, Textures[CurrentFrame].Height);
                Draw.SpriteBatch.Draw(Textures[CurrentFrame].Texture.Texture_Safe, Position - offset, Color.White);
            }
        }
        public Crack Cracks;
        public GrowingShiftArea(EntityData data, Vector2 offset) : base(data.Position, offset, data.Char("BgFrom", '0'), data.Char("BgTo", '0'), data.Char("FgFrom", '0'), data.Char("FgTo", '0'), data.NodesWithPosition(Vector2.One * 4), GetIndices(data, true))
        {
            Grows = true;
            Scale = data.Float("startPercent");
            GrowTime = data.Float("growTime", 2);
            Add(new Coroutine(Grow()));
            Collider = new Hitbox(8, 8);
            Cracks = new Crack(Vector2.Zero,7,switchMode);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Cracking = true;
            scene.Add(Cracks);
        }
        private void switchMode()
        {
            Cracking = false;
        }
        public override void DrawMask(Matrix matrix)
        {
            if (Cracking)
            {
                Cracks.Position = Box.Center;
                Cracks.Render();
            }
            else
            {
                base.DrawMask(matrix);
            }
        }
        public IEnumerator Grow()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / GrowTime)
            {
                Scale = Ease.BounceOut(Ease.SineIn(i));
                yield return null;
            }
        }

    }
}