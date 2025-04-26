using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static MonoMod.InlineRT.MonoModRule;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
/*    [ConstantEntity("PuzzleIslandHelper/DebugEntity")]
    [Tracked]
    public class DebugEntity : Entity
    {
        public DebugEntity() : base()
        {
            Tag |= Tags.Persistent;
            Add(new DebugComponent(Microsoft.Xna.Framework.Input.Keys.L, addPianoUI, true));
        }
        private void addPianoUI()
        {
            if (PianoModule.Session.HasPiano)
            {
                if (Scene.Tracker.GetEntity<PianoUI>() == null)
                {
                    Scene.Add(new PianoUI());
                }
                if (Scene.Tracker.GetEntity<PianoCode>() == null)
                {
                    Scene.Add(new PianoCode("pianoTest", "1,2,3,5,6,4"));
                }
            }
        }
    }*/

    [Tracked]
    public class PianoCode : Entity
    {
        public bool Obtained;
        public int[] Code;
        public string Flag;
        public string progress;
        public string code = "";
        private int lastIndex;
        public PianoCode(EntityData data, Vector2 offset) : this(data.Attr("flag"), data.Attr("code"))
        {
        }
        public PianoCode(string flag, string code) : base()
        {
            List<int> codearray = [];
            foreach (string s in code.Split(','))
            {
                if (int.TryParse(s, out int result))
                {
                    codearray.Add(result);
                    this.code += result + ",";
                }
            }
            Code = [.. codearray];
            this.code = this.code.TrimEnd(',');
            Flag = flag;
        }
        public PianoCode(string flag, params int[] code) : base()
        {
            Code = code;
            Flag = flag;
        }
        public void AddNote(int i)
        {
            if (lastIndex < Code.Length - 1)
            {
                if (Code[lastIndex + 1] == i)
                {
                    lastIndex++;
                    progress += i.ToString() + ",";
                    if (lastIndex == Code.Length - 1)
                    {
                        Flag.SetFlag();
                        lastIndex = 0;
                    }
                }
                else
                {
                    lastIndex = 0;
                    progress = "";
                }
            }
            else
            {
                Flag.SetFlag();
                lastIndex = 0;
                progress = "";
            }
            Engine.Commands.Log("code: " + code);
            Engine.Commands.Log("progress: " + progress);
        }
    }


    [Tracked]
    public class PianoUI : Entity
    {
        public class Key : GraphicsComponent
        {
            public float Width;
            public float Height;
            public string Name;
            public Vector2 Target;
            public int OutlineThickness = 4;
            public bool Pressed;
            public bool WasPressed;
            public EventInstance Event;
            public Key(Vector2 position, float width, float height, string name, int outlineThickness) : base(true)
            {
                Position = position;
                Width = width;
                Height = height;
                Name = name;
                Target = position;
                OutlineThickness = outlineThickness;
            }
            public override void Update()
            {
                base.Update();
                WasPressed = Pressed;
            }
            public bool Colliding(Vector2 mouse, Vector2 offset)
            {
                Vector2 p = RenderPosition + offset;
                Rectangle r = new Rectangle((int)p.X, (int)p.Y, (int)Width, (int)Height);
                return Collide.RectToPoint(r, mouse);
            }
            public void PlaySound()
            {
                Event = Audio.Play("event:/PianoBoy/cha"); //todo: change
            }
            public void StopSound()
            {
                Event?.stop(STOP_MODE.ALLOWFADEOUT);
            }
            public void DrawOutline()
            {
                Draw.Rect(RenderPosition, Width, Height, Color.Black);
            }
            public void DrawKey()
            {
                Draw.Rect(RenderPosition + Vector2.One * OutlineThickness, Width - OutlineThickness * 2, Height - OutlineThickness * 2, Color.Lerp(Color.White, Color.Black, Pressed ? 0.3f : 0));
            }
        }
        public VirtualRenderTarget Target;
        public MouseComponent Mouse;
        public List<Key> Keys = [];
        public float Percent = 0;
        public float IntroRotation = 15f.ToRad();
        public bool InControl;
        private Tween tween;
        public PianoUI() : base()
        {
            Tag |= TagsExt.SubHUD;
            Mouse = new MouseComponent(true, true)
            {
                OnLeftClick = OnClick,
                OnLeftRelease = OnRelease,
                OnLeftHeld = OnHeld
            };
            Add(Mouse);
            int width = 100;
            int height = 400;
            Target = VirtualContent.CreateRenderTarget("PianoTarget", width * 14, height);
            string[] array = ["A", "B", "C", "D", "E", "F", "G"];
            for (int i = 0; i < 14; i++)
            {
                Key key = new Key(Vector2.UnitX * (width * i), width, height, array[i % array.Length], 4);
                Add(key);
                Keys.Add(key);
            }
            Add(new BeforeRenderHook(BeforeRender));
            tween = new Tween();
            Intro();
        }
        public void UpdateCodes(int index)
        {
            Engine.Commands.Log("Note " + index);
            foreach (PianoCode code in Scene.Tracker.GetEntities<PianoCode>())
            {
                code.AddNote(index);
            }
        }
        public void OnClick()
        {
            /*            Vector2 p = Mouse.MousePosition;
                        for (int i = 0; i < Keys.Count; i++)
                        {
                            Key key = Keys[i];
                            key.Pressed = key.Colliding(p, RenderPosition);
                            if (key.Pressed)
                            {
                                key.PlaySound();
                                UpdateCodes(i);
                            }
                        }*/
        }
        public void OnRelease()
        {
            for (int i = 0; i < Keys.Count; i++)
            {
                Key key = Keys[i];
                key.Pressed = false;
                key.StopSound();
            }
        }
        public void OnHeld()
        {
            Vector2 p = Mouse.MousePosition;
            for (int i = 0; i < Keys.Count; i++)
            {
                Key key = Keys[i];
                bool c = key.Colliding(p, RenderPosition);
                if (key.WasPressed && !c)
                {
                    key.Pressed = false;
                    key.StopSound();
                }
                else if (!key.WasPressed && c)
                {
                    key.Pressed = true;
                    key.PlaySound();
                    UpdateCodes(i);
                }
            }
        }

        public void Intro()
        {
            float from = Percent;
            Remove(tween);
            InControl = false;
            tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1.4f, Ease.SineOut, t => Percent = Calc.LerpClamp(from, 1, t.Eased), delegate { InControl = true; });
        }
        public void Outro()
        {
            float from = Percent;
            InControl = false;
            Remove(tween);
            tween = Tween.Set(this, Tween.TweenMode.Oneshot, 1.4f, Ease.SineIn, t => Percent = Calc.LerpClamp(from, 0, t.Eased), t => RemoveSelf());
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.GetPlayer()?.DisableMovement();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
            scene.GetPlayer()?.EnableMovement();
        }
        public override void Update()
        {
            base.Update();
            if (InControl)
            {
                if (Input.DashPressed)
                {
                    Outro();
                }
            }
            Vector2 target = new Vector2(160, 90) * 6 - Target.HalfSize();
            Vector2 start = new Vector2(target.X, 1080);
            RenderPosition = Vector2.Lerp(start, target, Percent);
            Rotation = IntroRotation * (1 - Percent);

        }
        public void BeforeRender()
        {
            Target.SetAsTarget(true);
            if (Scene is not Level level) return;
            Draw.SpriteBatch.StandardBegin();
            foreach (Key key in Keys)
            {
                key.DrawOutline();
            }
            foreach (Key key in Keys)
            {
                key.DrawKey();
            }
            Draw.SpriteBatch.End();
        }
        public Vector2 RenderPosition;
        public float Rotation;
        public override void Render()
        {
            base.Render();
            Draw.Rect(0, 0, 1920, 1080, Color.Black * Percent * 0.5f);
            Draw.SpriteBatch.Draw(Target, RenderPosition, null, Color.White, Rotation, Vector2.Zero, 1, SpriteEffects.None, 0);
            Draw.Rect(Mouse.MousePosition, 16, 16, Color.Black * Percent);
        }
    }
}