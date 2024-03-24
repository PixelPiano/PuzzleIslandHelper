using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    public class DebugEater : Entity
    {

        private static ILHook Crimes;
        public static bool InScene;
        public DebugEater() : base(Vector2.Zero)
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            InScene = true;
        }
        public override void Update()
        {
            base.Update();
            InScene = true;
        }
        private static void ModUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.Match(OpCodes.Ldarg_0),
                    instr => instr.Match(OpCodes.Ldfld),
                    instr => instr.Match(OpCodes.Callvirt),
                    instr => instr.Match(OpCodes.Ldarg_0),
                    instr => instr.Match(OpCodes.Ldfld),
                    instr => instr.Match(OpCodes.Callvirt),
                    instr => instr.Match(OpCodes.Ldarg_0),
                    instr => instr.Match(OpCodes.Ldfld),
                    instr => instr.Match(OpCodes.Callvirt)
                    ))
            {
                ILLabel label = cursor.DefineLabel();
                cursor.EmitDelegate(NotInScene);
                cursor.Emit(OpCodes.Brtrue, label);
                if (cursor.TryGotoNext(MoveType.Before, instr => instr.Match(OpCodes.Ldarg_0)))
                {
                    cursor.MarkLabel(label);
                }
            }
        }
        private static bool NotInScene()
        {
            return !InScene;
        }
        internal static void Load()
        {
            InScene = false;
            //Crimes = new ILHook(typeof(Engine).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic), ModUpdate);
        }
        internal static void Unload()
        {
            InScene = false;
            //Crimes?.Dispose();
            //Crimes = null;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InScene = false;

        }
    }

}