using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities
{
    [Tracked]
    public class PianoContent : Entity
    {
        /*
         * Features to add:
         *      Sustain button
         *      Damp button
         *      Voice changer
         *      Melody detection
         *      Pattern Recording (If acceleration)
         *      Metronome (If acceleration)
         *      Resonance/Cutoff/Release (If acceleration)
         *      
         * Features added:
         *      Piano Keys
         *      1 Piano Voice,
         *      Key Dampening (Key area)
         */
        public List<PianoKey> Keys = new();
        public List<PianoKeySound> KeySounds = new();

        public static readonly string[] Scale = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
        public Cursor Mouse;
        public Sprite cursorSprite;
        public bool InControl = false;
        public static int BaseDepth = -1000001;
        public Rectangle MouseRectangle;
        public SoundSource Speaker;
        public PianoContent() : base(Vector2.Zero)
        {
            Depth = BaseDepth;
            cursorSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/piano/keys/");
            cursorSprite.AddLoop("idle", "Cursor", 0.1f);
            Add(cursorSprite);
            cursorSprite.Play("idle");
            Collider = new Hitbox(1, 1, 0, 0);
            MouseRectangle = new Rectangle(0, 0, 1, 1);
            Add(Speaker = new SoundSource());
        }

        public override void Render()
        {
            foreach (PianoKey key in Keys)
            {
                key.DrawSimpleOutline();
            }
            base.Render();
            cursorSprite.Render();
        }
        public override void Update()
        {
            base.Update();
            cursorSprite.Position = Cursor.MousePosition.ToInt() / 6;
            MouseRectangle.X = (int)cursorSprite.RenderPosition.X;
            MouseRectangle.Y = (int)cursorSprite.RenderPosition.Y;
        }
        public void AddKeys()
        {
            int octave = 3;

            float x = 0;
            bool prevNoteWasBlack = false;
            int scale = 2;
            while (Keys.Count < 88 / scale)
            {
                foreach (string note in Scale)
                {
                    if (Keys.Count < 88 / scale)
                    {
                        bool isBlack = note.Contains("#");
                        if (!prevNoteWasBlack && !isBlack)
                        {
                            x += 1 * scale;
                        }
                        else if (isBlack)
                        {
                            x -= 1 * scale;
                        }
                        else
                        {
                            x -= 2 * scale;
                        }
                        PianoKey key = new PianoKey(note, octave, scale);
                        if (isBlack)
                        {
                            prevNoteWasBlack = true;
                        }
                        else
                        {
                            prevNoteWasBlack = false;
                        }
                        key.Position.X = x;

                        key.Position.Y = 90 - (key.Height / 2 * scale);

                        Keys.Add(key);

                        x += key.Width * scale;
                    }
                    else break;
                }
                octave++;
            }
            Add(Keys.ToArray());
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Position = (scene as Level).Camera.Position;
            AddKeys();
            scene.Add(Mouse = new Cursor("objects/PuzzleIslandHelper/piano/Cursor"));
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose(scene);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose(scene);
        }
        private void Dispose(Scene scene)
        {
            if (Mouse != null)
            {
                scene.Remove(Mouse);
            }
            Mouse = null;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(MouseRectangle, Color.Aqua);
        }
    }
}