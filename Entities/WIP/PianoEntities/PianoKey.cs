using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP.PianoEntities
{


    [TrackedAs(typeof(PianoKey))]
    public class PianoKey : Image
    {
        public MTexture BaseTex;
        public MTexture PressedTex;
        public PianoContent Piano => Entity as PianoContent;
        public PianoKeySound Sound;
        public bool Pressed;
        private bool prevPressed;
        public int FadeLevel;
        public float DampLevel
        {
            get
            {
                float clickedPosY = (Entity as PianoContent).MouseRectangle.Center.Y - Colliders[0].Position.Y;
                float limit = Colliders[0].Height;
                if (Note == "black")
                {
                    limit -= Colliders[0].Height * 0.2f;
                }
                return Calc.Clamp(clickedPosY / limit, 0, 1);
            }
        }
        public string Note;
        public int Octave;
        public string EventSuffix
        {
            get
            {
                return Key + Octave;
            }
        }
        public float PressedTime;
        public string Voice = "piano-keys-b";
        public string Key;
        public string EventName
        {
            get
            {
                return "event:/PianoBoy/piano/" + Voice + "/" + EventSuffix;
            }
        }

        public List<Collider> Colliders = new();
        public const string Path = "objects/PuzzleIslandHelper/piano/keys/";
        public PianoKey(string key, int octave, int scale) : base(GFX.Game[Path + NoteTexture(key)], true)
        {
            Key = key;
            Note = NoteTexture(key);
            Scale = Vector2.One * scale;
            BaseTex = GFX.Game[Path + Note];
            PressedTex = GFX.Game[Path + Note + "Pressed"];
            Octave = octave;
            Sound = new PianoKeySound();

        }
        private void PlayNote()
        {
            Sound.Play(EventName, DampLevel);
        }
        private void StopNote()
        {
            Sound.Release();
        }

        public override void EntityAwake()
        {
            base.EntityAwake();
            CreateCollider();
        }
        public bool WasClicked()
        {
            bool collided = false;
            if (Cursor.LeftClicked)
            {
                foreach (Collider c in Colliders)
                {
                    if (Piano.MouseRectangle.Intersects(c.Bounds))
                    {
                        collided = true;
                        break;
                    }
                }
            }
            return collided;

        }

        public override void Update()
        {
            prevPressed = Pressed;
            base.Update();
            if (WasClicked())
            {
                Press();
                if (PressedTime == 0)
                {
                    PlayNote();
                }
            }
            else
            {
                Release();
                if (PressedTime > 0)
                {
                    StopNote();
                }
            }
            if (Pressed)
            {
                PressedTime += Engine.DeltaTime;
            }
            else
            {
                PressedTime = 0;
            }
        }
        public void Release()
        {
            PressedTime = 0;
            Pressed = false;
            Texture = BaseTex;
        }
        public void Press()
        {
            Pressed = true;
            Texture = PressedTex;
        }
        public void CreateCollider()
        {

            switch (Note)
            {
                case "start":
                    Colliders.Add(new Hitbox(5 * Scale.X, 11 * Scale.Y, 0, 0));
                    Colliders.Add(new Hitbox(6 * Scale.X, 7 * Scale.Y, 0, 11 * Scale.Y));
                    break;
                case "end":
                    Colliders.Add(new Hitbox(5 * Scale.X, 11 * Scale.Y, Scale.X, 0));
                    Colliders.Add(new Hitbox(6 * Scale.X, 7 * Scale.Y, 0, 11 * Scale.Y));
                    break;
                case "mid":
                    Colliders.Add(new Hitbox(4 * Scale.X, 11 * Scale.Y, Scale.X, 0));
                    Colliders.Add(new Hitbox(6 * Scale.X, 7 * Scale.Y, 0, 11 * Scale.Y));
                    break;
                case "black":
                    Colliders.Add(new Hitbox(3 * Scale.X, 11 * Scale.Y, 0, 0));
                    break;
                default:
                    break;
            }
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].Position += Entity.Position + Position;
            }
        }
        public static string NoteTexture(string key)
        {
            return key switch
            {
                "F" or "C" => "start",
                "B" or "E" => "end",
                "G" or "A" or "D" => "mid",
                _ => "black"
            };
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            for (int i = 0; i < Colliders.Count; i++)
            {
                Draw.HollowRect(Colliders[i], Color.Red);
            }
        }
    }
}