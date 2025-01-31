using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class DesktopComponent : Component
    {
        public Action<Scene> OnPrepare;
        public Action<Scene> OnBegin;
        public Action OnClick;
        public int Priority;
        public DesktopComponent(int priority = 0, Action<Scene> onPrepare = null, Action<Scene> onBegin = null, Action onClick = null) : base(true, true)
        {
            OnPrepare = onPrepare;
            OnBegin = onBegin;
            OnClick = onClick;
            Priority = priority;
        }
        public bool CollideCheck(Entity other)
        {
            return Entity.CollideCheck(other);
        }
        public void Prepare(Scene scene)
        {
            OnPrepare?.Invoke(scene);
        }
        public void Begin(Scene scene)
        {
            OnBegin?.Invoke(scene);
        }
        public void Click()
        {
            OnClick?.Invoke();
        }
    }
    [TrackedAs(typeof(InterfaceEntity))]
    public class DesktopClickable : InterfaceEntity
    {

        public Interface Interface;
        private int delayFrames;
        private float timer;
        public bool AlwaysClickable;
        public int Priority;
        public DesktopClickable(Interface @interface, int priority, int delayFrames = 0, bool alwaysClickable = false) : base(@interface.Position)
        {
            Add(new DesktopComponent(priority,Prepare,Begin,OnClick));
            Priority = priority;
            Interface = @interface;
            Depth = Interface.BaseDepth - 1;
            this.delayFrames = delayFrames;
            AlwaysClickable = alwaysClickable;
            Visible = false;
        }
        public override void InterfaceRender()
        {
            base.InterfaceRender();
        }
        /// <summary>
        /// Called just before the computer scene loads up (the poor man's awake)
        /// </summary>
        public virtual void Begin(Scene scene)
        {
        }
        /// <summary>
        /// Prepare anything that might need to interact with other desktop clickables (the poor man's added)
        /// </summary>
        public virtual void Prepare(Scene scene)
        {

        }

        /// <summary>
        /// Runs when the player clicks on the clickable
        /// </summary>
        public virtual void OnClick()
        {
            if (timer < delayFrames * Engine.DeltaTime) return;
            timer = 0;
        }
        public override void Update()
        {
            Position = Position.Floor();
            base.Update();
            if (timer < delayFrames * Engine.DeltaTime)
            {
                timer += Engine.DeltaTime;
            }
        }
    }
}