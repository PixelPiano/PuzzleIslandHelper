using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using Microsoft.Xna.Framework.Input;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/InterfaceMonitor")]
    [Tracked]
    public class InterfaceMonitor : Entity
    {
        private const string path = "objects/PuzzleIslandHelper/interface/";
        private MTexture monitor;
        private Sprite icon;
        public Color MonitorColor;
        public bool CanActivate => PianoModule.Session.MonitorActivated;
        public bool State;
        public bool InRoutine;
        public InterfaceMonitor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            monitor = GFX.Game[path + "monitor"];
            Collider = new Hitbox(monitor.Width, monitor.Height);
            icon = new Sprite(GFX.Game, path);
            icon.AddLoop("idleSmall", "iconToCenter", 0.1f, 0);
            icon.AddLoop("idleBig", "iconBigSpin", 0.1f);
            icon.Add("iconToCenter", "iconToCenter", 0.03f, "idleBig");
            icon.Add("iconToCorner","iconToCorner",0.03f, "idleSmall");
            Add(icon);
            icon.OnChange = (string s1, string s2) =>
            {
                if (s2 == s1) return;
                if (s1 == "iconToCenter" && s2 == "idleBig")
                {
                    SetInCenter();
                }
                else
                {
                    SetInCorner();
                }
            };
            icon.Play("idleSmall");
            Add(new DebugComponent(Keys.L, routineTrue()));
        }
        private IEnumerator routineTrue()
        {
            yield return Routine(true);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            icon.Visible = CanActivate;
            MonitorColor = CanActivate ? Color.White : Color.Gray;
        }
        public override void Update()
        {
            base.Update();
            MonitorColor = Color.Lerp(MonitorColor, CanActivate ? Color.White : Color.Gray, Engine.DeltaTime);
            icon.Color = Color.Lerp(icon.Color, State ? Color.White : Color.Gray, Engine.DeltaTime);
        }

        private void SetInCorner()
        {
            icon.Position = new Vector2(0, monitor.Height / 2 - 1);
        }
        private void SetInCenter()
        {
            icon.Position = (new Vector2(monitor.Width, monitor.Height) / 2) - (Vector2.One * 7); //manually use the width and height since Sprite hasn't updated the width and height yet
        }
        public IEnumerator Routine(bool state)
        {
            InRoutine = true;
            if (state)
            {
                icon.Play("idleSmall");
            }
            else
            {
                icon.Play("idleBig");
            }
            State = state;
            icon.Visible = true;
            for (int i = 0; i < 5; i++)
            {
                icon.Visible = !icon.Visible;
                yield return 0.1f;
            }
            icon.Visible = true;
            if (state)
            {
                icon.Play("iconToCenter");
            }
            else
            {
                icon.Play("iconToCorner");
            }
            while (!icon.CurrentAnimationID.Contains("idle"))
            {
                yield return null;
            }
            InRoutine = false;
            yield return null;
        }
        public void Activate()
        {
            if (CanActivate)
            {
                Add(new Coroutine(Routine(true)));
            }
        }
        public void Deactivate()
        {
            if (CanActivate)
            {
                Add(new Coroutine(Routine(false)));
            }
        }
        public override void Render()
        {
            Draw.SpriteBatch.Draw(monitor.Texture.Texture_Safe, Position, Color.White);
            base.Render();
        }
    }
}