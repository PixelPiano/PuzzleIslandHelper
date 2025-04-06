using Celeste.Mod.Entities;

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    public abstract class Monitor : Entity
    {
        private const string defaultpath = "objects/PuzzleIslandHelper/interface/";
        public bool Flickering => flickerCoroutine.Active;
        private string texturepath;
        public Image MonitorImage;
        public Sprite Icon;
        public Color MonitorColor;
        private float iconAlpha = 1;
        private float alphaFlickerInterval = 0.1f;
        private Coroutine flickerCoroutine;
        public abstract bool CanActivate();
        public bool State;
        public bool InRoutine;
        public bool Idle => Icon.CurrentAnimationID.Contains("idle");
        public Monitor(Vector2 position, string path = null) : base(position)
        {
            Add(flickerCoroutine = new Coroutine(false));
            texturepath = path ?? defaultpath;
            Depth = 1;
            MonitorImage = new Image(GFX.Game[texturepath + "monitor"]);
            Collider = new Hitbox(MonitorImage.Width, MonitorImage.Height);
            Icon = new Sprite(GFX.Game, texturepath);
            Icon.AddLoop("Corner", "iconToCenter", 0.1f, 0);
            Icon.AddLoop("Center", "iconBigSpin", 0.1f);
            Icon.Add("iconToCenter", "iconToCenter", 0.03f, "Center");
            Icon.Add("iconToCorner", "iconToCorner", 0.03f, "Corner");
            Icon.AddLoop("Battery", "noBattery", 0.2f);
            Tag |= Tags.TransitionUpdate;
            Add(MonitorImage, Icon);
            Icon.OnChange = (string s1, string s2) =>
            {
                if (s2 == s1) return;
                if (s1 == "iconToCenter" && s2 == "Center")
                {
                    CenterIcon();
                }
                else
                {
                    CornerIcon();
                }
            };
            Icon.Play("Corner");
        }
        private IEnumerator alphaFlicker()
        {
            while (true)
            {
                iconAlpha = Calc.Random.Range(0.7f, 1);
                yield return alphaFlickerInterval;
            }
        }
        public void Play(string anim)
        {
            Icon.Play(anim);
        }
        public Monitor(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Icon.Visible = CanActivate();
            MonitorColor = CanActivate() ? Color.White : Color.Gray;
            Icon.Color = State ? Color.White : Color.Gray;
        }
        public override void Update()
        {
            base.Update();
            MonitorColor = Color.Lerp(MonitorColor, CanActivate() ? Color.White : Color.Gray, Engine.DeltaTime);
            Icon.Color = Color.Lerp(Icon.Color, State ? Color.White : Color.Gray, Engine.DeltaTime) * iconAlpha;
        }

        public void CornerIcon()
        {
            Icon.Position = new Vector2(3, MonitorImage.Height / 2 + 3);
        }
        public void CenterIcon()
        {
            Icon.Position = new Vector2(MonitorImage.Width, MonitorImage.Height) / 2 - Vector2.One * 7; //manually use the width and height since Sprite hasn't updated the width and height yet
        }
        public void LowBattery()
        {
            Play("Battery");
            CenterIcon();
        }
        public void BigLogo()
        {
            Play("Center");
            CenterIcon();
        }
        public void SmallLogo()
        {
            Play("Corner");
            CornerIcon();
        }
        private IEnumerator cutscene(bool on)
        {
            InRoutine = true;
            yield return new SwapImmediately(Routine(on));
            InRoutine = false;
        }
        public virtual IEnumerator Routine(bool on)
        {
            Icon.Visible = true;
            Play(on ? "Corner" : "Center");
            State = on;
            Flicker(5, 0.1f);
            while (Flickering) yield return null;
            Icon.Visible = true;
            Icon.Play(on ? "iconToCenter" : "iconToCorner");
            while (!Idle)
            {
                yield return null;
            }
        }
        private IEnumerator intervalFlicker(int flickers, float? interval = null)
        {
            for (int i = 0; i < flickers; i++)
            {
                Icon.Visible = !Icon.Visible;
                yield return interval;
            }
        }
        public void StopFlickering(bool? state = null)
        {
            flickerCoroutine.Cancel();
            if (state.HasValue)
            {
                Icon.Visible = state.Value;
            }
        }
        public void Flicker(int flickers, float? interval = null)
        {
            flickerCoroutine.Replace(intervalFlicker(flickers, interval));

        }
        public void EndlessFlicker(float? interval = null)
        {
            flickerCoroutine.Replace(endlessFlicker(interval));
        }
        private IEnumerator endlessFlicker(float? interval = null)
        {
            while (true)
            {
                Icon.Visible = !Icon.Visible;
                yield return interval;
            }
        }
        public void Activate()
        {
            if (!InRoutine && CanActivate())
            {
                Add(new Coroutine(cutscene(true)));
            }
        }
        public void Deactivate()
        {
            if (!InRoutine && CanActivate())
            {
                Add(new Coroutine(cutscene(false)));
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/InterfaceMonitor")]
    [Tracked]
    public class InterfaceMonitor : Monitor
    {
        public static string InteractedFlag = "TriedToTurnOnLabComputer";
        public InterfaceMonitor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (InteractedFlag.GetFlag() && !PianoModule.Session.RestoredPower)
            {
                Play("Battery");
                CenterIcon();
            }
        }
        public override bool CanActivate()
        {
            return PianoModule.Session.MonitorActivated;
        }
    }
    [CustomEntity("PuzzleIslandHelper/TransitMonitor")]
    [TrackedAs(typeof(Monitor))]
    public class TransitMonitor : Monitor
    {
        public TransitMonitor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
        public override bool CanActivate()
        {
            return true;
        }
        public override IEnumerator Routine(bool on)
        {
            yield return base.Routine(on);
            if (on)
            {
                yield return 1;
                Flicker(5, 0.1f);
                while (Flickering) yield return null;
                Icon.Visible = false;
                Chat chat = new Chat(Position, Width, Height, Depth - 1);
                while (!chat.Finished)
                {
                    yield return null;
                }
                yield return 0.1f;
                Flicker(5, 0.1f);
                while (Flickering) yield return null;
                Icon.Visible = true;
            }

        }
    }
    public class Chat : Entity
    {

        private const string fontName = "pixelary";
        private static readonly Dictionary<string, List<string>> fontPaths;
        static Chat()
        {
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
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
        public bool Finished;

        public Chat(Vector2 position, float width, float height, int depth = 1) : base(position)
        {
            Collider = new Hitbox(width, height);
            Depth = depth;
            Add(new Coroutine(routine()));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ensureCustomFontIsLoaded();
        }
        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/MemoryTextscene", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }

        private IEnumerator routine()
        {
            yield return null;
        }
    }
}