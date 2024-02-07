using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.CodeDom;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Observer")]
    [Tracked]
    public class Observer : Entity
    {
        [TrackedAs(typeof(ShaderOverlay))]
        public class ObserverRenderer : ShaderOverlay
        {
            public Vector2 SignalCenter;
            public float SignalWidth;
            public float SignalHeight;
            public float SignalTime;
            public bool On;
            public Observer Parent;
            public float TextAlpha;
            public ObserverRenderer(Observer parent, Vector2 center, float width, float height, float time, float alpha, bool on = true) : base("PuzzleIslandHelper/Shaders/curvePulse")
            {
                TextAlpha = alpha;
                Parent = parent;
                SignalCenter = center;
                SignalWidth = width;
                SignalHeight = height;
                SignalTime = time;
                On = on;
                Add(new Coroutine(Lerp()));
            }
            public IEnumerator Lerp()
            {
                while (true)
                {
                    bool spun = false;
                    for (float i = 0; i < 1; i += Engine.DeltaTime / SignalTime)
                    {
                        if (i > 0.3f && !spun)
                        {
                            Parent.Emit();
                            spun = true;
                        }
                        Amplitude = Ease.SineInOut(i);
                        yield return null;
                    }
                }
            }
            public override void ApplyParameters(bool identity)
            {
                base.ApplyParameters(identity);
                Effect.Parameters["MaxWidth"]?.SetValue(SignalWidth);
                Effect.Parameters["MaxHeight"]?.SetValue(SignalHeight);
                Effect.Parameters["Center"]?.SetValue(SignalCenter);
                Effect.Parameters["GrowTime"]?.SetValue(SignalTime);
                Effect.Parameters["On"]?.SetValue(On);
                Effect.Parameters["TextAlpha"]?.SetValue(TextAlpha);
            }
        }
        public ObserverRenderer Renderer;
        public Sprite Sprite;

        public enum Modes
        {
            Off,
            Scanning,
            Detected,
            Alert,
            Digital
        }
        public Modes Mode;
        public float SignalWidth;
        public float SignalHeight;
        public float SignalTime;
        public Vector2 UVCenter;
        public bool Digital;

        public Observer(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            SignalWidth = data.Float("signalWidth");
            SignalHeight = data.Float("signalHeight");
            SignalTime = data.Float("signalTime");
            Digital = data.Bool("digital");
            Mode = data.Enum<Modes>("mode");
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/observer/");
            Sprite.AddLoop("idle", "observer", 0.1f, 0);
            Sprite.Add("spin", "observer", 0.1f, "idle");
            Sprite.AddLoop("dIdle", "observerDigital", 0.1f, 0);
            Sprite.Add("dSpin", "observerDigital", 0.1f, "dIdle");
            Add(Sprite);
            Sprite.Play(Digital ? "dIdle" : "idle");
            Collider = new Hitbox(SignalWidth, SignalHeight + (Sprite.Height / 2), 0, -Sprite.Height / 2);
        }
        public void Emit()
        {
            Sprite.Play(Digital ? "dSpin" : "spin");
        }
        public override void Update()
        {
            base.Update();
            Renderer.On = Mode != Modes.Off;
            Renderer.SignalTime = Mode switch
            {
                Modes.Alert => SignalTime / 2,
                _ => SignalTime
            };
            Renderer.SignalWidth = SignalWidth;
            Renderer.SignalHeight = SignalHeight;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            UVCenter = Collider.AbsolutePosition + Collider.HalfSize - (scene as Level).LevelOffset;
            UVCenter /= new Vector2(320, 180);
            Renderer = new ObserverRenderer(this, UVCenter, SignalWidth, SignalHeight, SignalTime, 0.96f);
            scene.Add(Renderer);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Renderer);
        }
    }
}