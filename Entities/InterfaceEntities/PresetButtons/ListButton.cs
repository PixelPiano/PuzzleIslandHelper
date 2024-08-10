using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(Button))]
    public class ListButton : Button
    {
        public static string CurrentEventName;
        public Func<string> CustomText;
        public WindowContent Program;
        public Vector2 origPosition;
        public ListButton(WindowContent parent, string text, Func<string> customText = null) : base(parent.Window, "list")
        {
            Text = text;
            TextSize = 35f;
            TextOffset = new Vector2(16, 8);
            CustomText = customText;
            Program = parent;
        }
        public override void RunActions()
        {
            base.RunActions();

            if (Program is not null)
            {
                if (ForcePressed)
                {
                    Pressing = false;
                }
            }
        }
        public override void OnOpened(Scene scene)
        {
            base.OnOpened(scene);

        }
        public override void Render()
        {
            TextRenderer.Alpha = Alpha;
            Draw.Rect(ButtonCollider, (Pressing ? Color.Blue : Color.LightBlue) * Alpha);
            base.Render();
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            base.Text = CustomText is null ? Text : CustomText.Invoke();
        }
        public override void Update()
        {
            base.Update();
            if (CustomText is not null)
            {
                base.Text = CustomText.Invoke();
            }
            Alpha = GetAlpha(8, 2);
            Disabled = Alpha <= 0;
        }
        public float GetAlpha(float thresh, float offset)
        {
            float top = Position.Y + offset;
            float bottom = Position.Y + Height - offset;
            if (top < 0 || bottom > Window.CaseHeight) return 0;
            else if (top < thresh) return 1 - MathHelper.Distance(top, thresh) / thresh;
            else if (bottom > Window.CaseHeight - thresh) return MathHelper.Distance(bottom, Window.CaseHeight) / thresh;
            else return 1;
        }
    }
}