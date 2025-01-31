using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [TrackedAs(typeof(Icon))]
    public class YouHaveMail : DesktopEntity
    {
        public bool State;
        private Sprite sprite;
        private bool inRoutine;
        private string TextID, TabText;
        public YouHaveMail(Interface inter, string textId, string tabText) : base(inter, (int)Interface.Priority.Power)
        {
            TextID = textId;
            TabText = tabText;
        }
        public override void Update()
        {
            base.Update();
            if (Parent.Monitor != null)
            {
               // Position = Interface.monitor.Center - new Vector2(Width / 2, Height / 2);
            }
        }
        public override void Prepare(Scene scene)
        {
            base.Prepare(scene);
            sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/mail/");
            sprite.AddLoop("anim", "notif", 0.05f);
            sprite.AddLoop("idle", "empty", 0.1f);
            sprite.Play("idle");
            Add(sprite);
            Collider = new Hitbox(sprite.Width, sprite.Height);
            sprite.CenterOrigin();
            sprite.Position = new Vector2(Width / 2, Height / 2);
        }
        public override void Begin(Scene scene)
        {
            base.Begin(scene);
            Position = Parent.Monitor.Center - new Vector2(Width / 2, Height / 2);
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 1, false);
            tween.OnUpdate = (Tween t) =>
            {
                sprite.Scale.Y = t.Eased;
                sprite.Scale.X = 0.5f + t.Eased / 2;
            };
            tween.OnComplete = delegate { sprite.Scale = Vector2.One; sprite.Play("anim"); };
            Add(tween);
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, tween.Start, 0.3f, true));
        }
        public override void OnClick()
        {
            base.OnClick();
            if (!inRoutine)
            {
                inRoutine = true;
                Add(new Coroutine(sequence()));
            }
        }
        private IEnumerator sequence()
        {
            Parent.Window.Alpha = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                sprite.Scale.Y = 1 - Ease.BigBackIn(i);
                yield return null;
            }
            sprite.Scale.Y = 0;
            yield return null;
            sprite.Visible = false;
            WindowContent content = Parent.GetProgram("mail");
            if (content is null) yield break;
            content.Visible = false;
            Parent.CreateAndOpenIcon("mail", TextID, TabText);
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                Parent.Window.Alpha = Ease.SineInOut(i);
                yield return null;
            }
            Parent.Window.Alpha = 1;
            content.Visible = true;
        }
    }
}