using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.EscapeRoomEntities
{

    [Tracked]
    public class EscapeInv : Entity
    {
        public int Selected;
        public bool Pushed => Input.DashPressed;
        public static bool Disabled;
        public Item CurrentItem
        {
            get
            {
                if (Items == null || Items.Count == 0) return null;
                else return Items[Selected];
            }
        }
        public List<Item> Items = new();
        public static readonly Vector2 Offset = new Vector2(0, 164);
        public EscapeInv() : base(Vector2.Zero)
        {

        }
        public void SetSelected()
        {
            if (Items.Count == 0)
            {
                return;
            }
            int val = MInput.Mouse.WheelDelta;
            Selected = Selected + val < 0 ? Items.Count - 1 : Selected + val >= Items.Count ? 0 : Selected + val;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Items = GetItems();
        }
        public List<Item> GetItems()
        {
            return Components.GetAll<Item>().ToList();
        }
        public override void Update()
        {
            if (Scene is not Level level)
            {
                base.Update();
                return;
            }
            SetSelected();
            Position = level.ScreenToWorld(Offset);
            base.Update();

            if (Pushed && !Disabled)
            {
                UseItem();
            }
        }
        public void UseItem()
        {
            Item current = CurrentItem;
            if (current != null && current.CanUse)
            {
                current.Use();
            }
        }
        public override void Render()
        {
            base.Render();
            float lerp = Disabled ? 0.5f : 0f;
            for (int i = 0; i < Items.Count; i++)
            {
                Draw.HollowRect(Position.X + 16 * i, Position.Y, 16, 16, Color.Black);
            }
            Draw.HollowRect(1 + Position.X + 16 * Selected, 1 + Position.Y, 14, 14, Color.Lerp(Color.White, Color.Gray, lerp));
        }
    }
}