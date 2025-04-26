using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public abstract class AbstractMonitor : Entity
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
        public bool Idle => Icon.CurrentAnimationID is "Center" or "Corner";
        public AbstractMonitor(Vector2 position, string path = null) : base(position)
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
        public AbstractMonitor(EntityData data, Vector2 offset) : this(data.Position + offset)
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
            yield return null;
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
            if (!InRoutine)
            {
                Add(new Coroutine(cutscene(false)));
            }
        }
    }
}