using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System.Collections;
using System.Reflection;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [CustomEntity("PuzzleIslandHelper/PrologueBooster")]
    [Tracked]
    public class PrologueBooster : Booster
    {
        private static IDetour hook_Player_get_CanDash;
        private delegate bool orig_Player_get_CanDash(Player self);
        public PrologueBooster(EntityData data, Vector2 offset)
            : base(data.Position + offset, true)
        {
        }

        public static void Load()
        {
            On.Celeste.Input.GetAimVector += Input_GetAimVector;
            hook_Player_get_CanDash = new Hook(
               typeof(Player).GetProperty("CanDash").GetGetMethod(),
               typeof(PrologueBooster).GetMethod("Player_get_CanDash", BindingFlags.NonPublic | BindingFlags.Static)
           );
        }

        public static void Unload()
        {
            On.Celeste.Input.GetAimVector -= Input_GetAimVector;
            hook_Player_get_CanDash?.Dispose();
        }
        private static bool Player_get_CanDash(orig_Player_get_CanDash orig, Player self)
        {
            if (self.LastBooster is PrologueBooster && (self.StateMachine.State == Player.StRedDash)) return false;
            return orig(self);
        }
        private static Vector2 Input_GetAimVector(On.Celeste.Input.orig_GetAimVector orig, Facings defaultFacing)
        {
            if (Engine.Scene is not Level level || level.GetPlayer() is not Player player || player.CurrentBooster is not PrologueBooster)
            {
                return orig(defaultFacing);
            }
            return -Vector2.UnitY;
        }

    }
}
