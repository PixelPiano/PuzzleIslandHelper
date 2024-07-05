using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/EscapeTimer")]
    [Tracked]
    public class EscapeTimer : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        static EscapeTimer()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private Alarm alarm;
        private string text;
        private Player player;
        private static float limit = 120f;
        private static float remaining;
        private static bool isReturning;
        private float staticTime = 0;
        private Level l;
        private bool RoutineRan = false;
        private bool Randomizing = false;
        private Color color = Color.White;
        private const string fontName = "alarm clock";
        private EntityID id;
        public EscapeTimer(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset)
        {
            this.id = id;
            Tag |= TagsExt.SubHUD;
            Tag |= Tags.Global;
            Depth = -1000001;
            if (!isReturning)
            {
                remaining = data.Float("startFrom", 120f);
                limit = remaining;
            }
        }
        private void SceneCheck(Scene scene)
        {
            bool flag = (scene as Level).Session.GetFlag("cameFromDigi");
            bool flag2 = (scene as Level).Session.Level.Contains("digiEscape");
            bool flag3 = PianoModule.Session.Escaped;
            bool flag4 = SaveData.Instance.DebugMode;

            if (!flag && !flag4 || !flag2 || flag3 && !flag4)
            {
                Tag -= Tags.Global;
                RemoveSelf();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            (scene as Level).Session.DoNotLoad.Add(id);
            RoutineRan = false;
            SceneAs<Level>().Session.SetFlag("BigGlitching", false);
            alarm = Alarm.Set(this, remaining, delegate
            {
                if (!SceneAs<Level>().Session.GetFlag("TimerEvent"))
                {
                    player?.Die(Vector2.One);
                }
                alarm.RemoveSelf();
            });
            SetText();
            ensureCustomFontIsLoaded();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneCheck(scene);
        }
        public static void Load()
        {
            remaining = limit;
        }
        private void SetText(float time = -1)
        {
            remaining = time == -1 ? alarm.TimeLeft : time;
            text = remaining.ToString();
            if (!Randomizing)
            {
                text = text.Length > text.IndexOf('.') + 3 ? text.Remove(text.IndexOf('.') + 3) : text;
            }

        }
        public override void Update()
        {
            base.Update();

            player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
            {
                return;
            }
            //isReturning = SceneAs<Level>().Session.GetFlag("leftFirstRoom");
            if (!SceneAs<Level>().Session.GetFlag("TimerEvent"))
            {
                SetText();
            }
            else if (!RoutineRan)
            {
                Add(new Coroutine(TimeRoutine(true), true));
            }
            if (SceneAs<Level>().Session.GetFlag("GlitchCutsceneEnd"))
            {
                remaining = 120f;
                alarm.Stop();
                Tag -= Tags.Global;
                RemoveSelf();
            }
        }

        private IEnumerator TimeRoutine(bool ending)
        {
            RoutineRan = true;
            if (!ending)
            {
                staticTime = alarm.TimeLeft;
                SetText(staticTime);
            }
            else
            {
                float _amount = alarm.TimeLeft;
                float _time = 0.01f;
                float _amountCache = _amount;
                while (!SceneAs<Level>().Session.GetFlag("startWaiting"))
                {
                    SetText(_amountCache = Calc.LerpClamp(_amount, _amount - 5, _time));
                    yield return _time;
                    _time *= 1.01f;
                }
                _amount = _amountCache;
                yield return 0.5f;

                for (float i = 0; i < 1; i += 0.005f)
                {
                    SetText(Calc.LerpClamp(_amount, _amount + 100, Ease.SineIn(i)));
                    yield return null;
                }
                Randomizing = true;
                while (true)
                {
                    SetText(Calc.Random.Range(102f, 9000f));
                    yield return null;
                }
            }
            yield return null;
        }
        public override void Render()
        {
            base.Render();
            if (Scene as Level == null)
            {
                return;
            }
            l = Scene as Level;
            if (!SceneAs<Level>().Session.GetFlag("BigGlitching"))
            {
                Draw.Rect(-1, 0, 460, 190, Color.Red);
                Draw.Rect(-1, 0, 450, 180, Color.Black);
                Fonts.Get(fontName).Draw(180f, text, new Vector2(30, 25), Vector2.Zero, Vector2.One * 1f, color * 0.5f);
                Fonts.Get(fontName).Draw(180f, text, new Vector2(40, 20), Vector2.Zero, Vector2.One * 1f, color);
            }
            else
            {
                float X;
                float Y;
                float rand;
                for (int i = 0; i < 30; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        X = Calc.Random.Range(0, l.Camera.Right - text.Length * 10);
                        Y = Calc.Random.Range(0, l.Camera.Bottom);
                        rand = Calc.Random.Range(1f, 4f);
                        Fonts.Get(fontName).Draw(180f, text, new Vector2(X - 10, Y + 25), Vector2.Zero, Vector2.One * rand, color * 0.5f);
                        Fonts.Get(fontName).Draw(180f, text, new Vector2(X, Y), Vector2.Zero, Vector2.One * rand, Color.White);
                    }
                }
            }
        }

        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/EscapeTimer", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }

        // a small entity that just ensures the font loaded by the scaleTimer unloads upon leaving the map.
        private class FontHolderEntity : Entity
        {
            public FontHolderEntity()
            {
                Tag = Tags.Global;
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Fonts.Unload(fontName);
            }
        }
    }
}

