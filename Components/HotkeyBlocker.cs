using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class HotkeyBlocker : Component
    {
        public Func<bool> Condition;
        public HotkeyBlocker(Func<bool> blockThisFrame) : base(true, true)
        {
            Condition = blockThisFrame;
        }
        public bool Check()
        {
            if (Condition != null)
            {
                return Condition.Invoke();
            }
            return false;
        }
        [OnLoad]
        public static void Load()
        {
            IL.Monocle.MInput.Update += MInput_Update;
        }
        [OnUnload]
        public static void Unload()
        {
            IL.Monocle.MInput.Update -= MInput_Update;
        }

        private static void MInput_Update(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.MatchCall<Engine>("get_Commands"),
                    instr => instr.MatchLdfld<Monocle.Commands>("Open")
                    ))
            {
                cursor.EmitDelegate(DisableHotkeys);
                cursor.Emit(OpCodes.Or);
            }
        }
        private static bool DisableHotkeys()
        {
            if(Engine.Scene is Level level)
            {
                foreach(HotkeyBlocker blocker in level.Tracker.GetComponents<HotkeyBlocker>())
                {
                    if (blocker.Check())
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
