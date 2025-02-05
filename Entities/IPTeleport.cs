using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/IPTeleport")]
    public class IPTeleport : Entity
    {
        [Command("testaddress", "")]
        public static string GetAddress(string first, string second, string third, string fourth)
        {
            string result = (GetNumber(first, 3) is string s ? s : "000") + "." +
                (GetNumber(second, 2) is string s2 ? s2 : "00") + "." +
                (GetNumber(third, 2) is string s3 ? s3 : "00") + "." +
                (GetNumber(fourth, 1) is string s4 ? s4 : "0");
            Engine.Commands.Log("Result: " + result);
            return result;
        }
        [Command("testnum", "")]
        public static string GetNumber(string from, int digits)
        {
            if (from.Length >= digits)
            {
                from = from[..digits];
            }
            if (int.TryParse(from, out int result))
            {
                string format = "";
                for (int i = 0; i < digits; i++)
                {
                    format += "0";
                }
                string r = result.ToString(format);
                Engine.Commands.Log(r);
                return r;
            }
            Engine.Commands.Log("null");
            return null;
        }
    }
}