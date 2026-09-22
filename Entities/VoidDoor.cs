using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/VoidDoor")]
    [Tracked]
    public class VoidDoor : Entity
    {
        //wip
        public FlagData FlagOnActivate;
        public FlagData[] Flags;
        public class Lock : GraphicsComponent
        {
            public float Size;
            public Lock(Vector2 position, float size) : base(true)
            {
                Position = position;
                Size = size;
                Color = Color.Red;
            }
            public override void Render()
            {
                base.Render();
                Vector2 p = RenderPosition - new Vector2(Size) / 2;
                Draw.HollowRect(p - Vector2.One, Size + 2, Size + 2, Color.Black);
                Draw.Rect(p, Size, Size, Color);
            }
        }
        public Lock[] Locks;
        private float spacing = 4;
        private float size = 8;
        private bool activated;

        public VoidDoor(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            FlagOnActivate = data.Flag("flagOnActivate");
            string[] array = data.Attr("flags").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            Flags = new FlagData[array.Length];
            Locks = new Lock[array.Length];
            Collider = new Hitbox(array.Length * size + spacing * 2, 40);
            for (int i = 0; i < array.Length; i++)
            {
                Flags[i] = new FlagData(array[i]);
                Lock newLock = new Lock(CenterLeft + Vector2.UnitX * (spacing + i * (size + spacing / 2)), size);
                Add(newLock);
                Locks[i] = newLock;
            }
        }
        public override void Update()
        {
            base.Update();
            int flagsActivated = 0;
            for (int i = 0; i < Flags.Length; i++)
            {
                if (Flags[i]) flagsActivated++;
            }
            for (int i = 0; i < Locks.Length; i++)
            {
                Locks[i].Color = flagsActivated > i ? Color.Lime : Color.Red;
            }
            if (!activated && flagsActivated == Locks.Length)
            {
                Activate(false);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (FlagOnActivate)
            {
                Activate(true);
            }
        }
        public void Activate(bool instant)
        {
            if (!activated)
            {
                activated = true;
                FlagOnActivate.State = true;
                if (instant)
                {
                    SnapOpen();
                }
                else
                {
                    Add(new Coroutine(activateRoutine()));
                }
            }
        }
        private IEnumerator activateRoutine()
        {
            yield return null;
            SnapOpen();
        }
        public void SnapOpen()
        {

        }
        public override void Render()
        {
            Draw.Rect(Collider, Color.DarkGray);
            base.Render();
        }
    }
}