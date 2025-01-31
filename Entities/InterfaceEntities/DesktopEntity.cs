using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class DesktopEntity : Entity
    {
        public Interface Parent;
        private int delayFrames;
        private float timer;
        public bool AlwaysClickable;
        public int Priority;
        public bool SubHUD;
        public DesktopEntity(Interface @interface,bool subhud, int depth, int delayFrames = 0, bool alwaysClickable = false) : base(@interface.Position)
        {
            Priority = depth;
            Parent = @interface;
            //Depth = Interface.BaseDepth - 1;
            this.delayFrames = delayFrames;
            AlwaysClickable = alwaysClickable;
            SubHUD = subhud;
            if (subhud)
            {
                Tag |= TagsExt.SubHUD;
            }
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
        public virtual void InterfaceRender(Scene scene)
        {
            base.Render();
        }
        public override void Render()
        {
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