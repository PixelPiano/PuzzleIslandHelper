using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class FlagListener : Component
    {
        public FlagData Flag;
        private bool prevState;
        public Action<bool> OnNewState;
        public Action WhileFalse;
        public Action WhileTrue;
        public bool AddedCheck;
        private bool? startValue;

        public FlagListener(FlagData data, Action<bool> onNewState, Action whileFalse = null, Action whileTrue = null, bool? startValue = null) : base(true, false)
        {
            Flag = data;
            OnNewState = onNewState;
            WhileFalse = whileFalse;
            WhileTrue = whileTrue;
            this.startValue = startValue;
        }
        public FlagListener(string flag, bool inverted, Action<bool> onNewState, Action whileFalse, Action whileTrue) : this(new FlagData(flag, inverted), onNewState, whileFalse, whileTrue)
        {

        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            prevState = Flag.State;
            if (startValue.HasValue)
            {
                Flag.State = startValue.Value;
            }
            if (AddedCheck)
            {
                Check();
            }
        }
        public override void Update()
        {
            base.Update();
            Check();
        }
        public void Check()
        {
            bool state = Flag.State;
            if (prevState != state)
            {
                OnNewState?.Invoke(state);
            }
            if (state)
            {
                WhileTrue?.Invoke();
            }
            else
            {
                WhileFalse?.Invoke();
            }
            prevState = state;
        }
    }
}
