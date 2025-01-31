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
        public int Thresh = 4;
        public int Offset = 2;
        public bool First;
        public bool Last;
        public ListButton(WindowContent parent, string text, Func<string> customText = null) : base(parent.Window, "list")
        {
            Text = text;
            TextSize = 35f;
            TextOffset = new Vector2(16, 8);
            CustomText = customText;
            Program = parent;
            Alpha = GetAlpha(Thresh, Offset);
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
            Alpha = GetAlpha(Thresh, Offset);
        }
        public override void Render()
        {
            if(Alpha <= 0) return;
            Draw.Rect(ButtonCollider, (Pressing ? Color.Blue : Color.LightBlue) * Alpha);
            base.Render();
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Text = CustomText is null ? Text : CustomText.Invoke();
        }
        public override void Update()
        {
            base.Update();
            if (CustomText is not null)
            {
                Text = CustomText.Invoke();
            }
            Alpha = GetAlpha(Thresh, Offset);
            TextRenderer.Alpha = Alpha;
            Disabled = Alpha <= 0;
        }

        public float GetAlpha(float thresh, float offset)
        {
            float top = Position.Y;
            float bottom = Position.Y + Height;
            if (top < thresh)
            {
                return Math.Max(top / thresh, 0);
            }
            else if (bottom > Window.CaseHeight - thresh)
            {
                return Math.Max((Window.CaseHeight - bottom) / thresh, 0);
            }
            else
            {
                return 1;
            }
        }
    }
}