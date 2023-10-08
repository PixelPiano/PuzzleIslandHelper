using Celeste.Mod.Backdrops;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{


    [CustomEntity("PuzzleIslandHelper/LabTubeLight")]
    [Tracked]
    public class LabTubeLight : Solid
    {
        public readonly int Length;
        private List<Image> images = new List<Image>();
        private SoundSource sfx;
        private BloomPoint bloom;
        private VertexLight light;
        private bool State = true;
        private bool Flickering;
        public LabTubeLight(Vector2 position, int length) : base(position, length, 8, false)
        {
            Tag |= Tags.TransitionUpdate;

            Position = position;
            Length = Math.Max(16, length);
            Depth = -1;
            MTexture mTexture = GFX.Game["objects/PuzzleIslandHelper/machines/gizmos/tubeLight"];
            Image image;
            Add(image = new Image(mTexture.GetSubtexture(0, 0, 8, 8)));
            images.Add(image);
            for (int i = 0; i < Length - 16; i += 8)
            {
                Add(image = new Image(mTexture.GetSubtexture(8, 0, 8, 8)));
                image.Position.X = i + 8;
                images.Add(image);
            }
            Add(image = new Image(mTexture.GetSubtexture(16, 0, 8, 8)));
            image.Position.X = Length - 8;
            images.Add(image);

            Add(sfx = new SoundSource());
            Collider = new Hitbox(Length, 8);
            Add(bloom = new BloomPoint(new Vector2(Length / 2, 7), 1f, Length / 2));
            Add(light = new VertexLight(new Vector2(Length / 2, 7), Color.White, 0.5f, Length / 2, Length));
            OnDashCollide = DashCollision;
        }
        private DashCollisionResults DashCollision(Player player, Vector2 direction)
        {
            if (PianoModule.Session.RestoredPower)
            {
                //play spark sound
                //emit tiny electricity
                Audio.Play("event:/PianoBoy/TubeLightSparks", Center);
                return DashCollisionResults.Bounce;
            }
            return DashCollisionResults.NormalCollision;
        }
        public LabTubeLight(EntityData e, Vector2 position)
            : this(e.Position + position, Math.Max(16, e.Width))
        {
        }
        public override void Update()
        {
            base.Update();
            UpdateVisuals();
        }
        private void UpdateVisuals()
        {
            light.Visible = bloom.Visible = State;
            foreach (Image i in images)
            {
                if (!Flickering)
                {
                    i.Color = Color.Lerp(Color.White, Color.Black, State ? 0 : 0.2f);
                }
            }
        }
        public override void Render()
        {
            foreach (Component component in Components)
            {
                (component as Image)?.DrawOutline();
            }
            base.Render();

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            UpdateVisuals();
            if (State)
            {
                Add(new Coroutine(Flicker()));
            }
        }
        private IEnumerator Flicker()
        {
            while (true)
            {
                yield return Calc.Random.Range(1, 6f);
                Flickering = true;
                foreach (Image i in images)
                {
                    i.Color = Color.Lerp(Color.White, Color.Black, 0.2f);
                }
                yield return Calc.Random.Range(0.05f, 0.1f);
                foreach (Image i in images)
                {
                    i.Color = Color.White;
                }
                Flickering = false;
                if (Calc.Random.Chance(0.2f))
                {
                    yield return Calc.Random.Range(0.2f, 0.5f);
                    Flickering = true;
                    foreach (Image i in images)
                    {
                        i.Color = Color.Lerp(Color.White, Color.Black, 0.2f);
                    }
                    yield return Calc.Random.Range(0.05f, 0.1f);
                    foreach (Image i in images)
                    {
                        i.Color = Color.White;
                    }
                    Flickering = false;
                }
                yield return null;
            }
        }
    }
}