using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    //UNFINISHED
    public class HairColorOverride : Backdrop
    {
        private Player player;
        private List<Color?> Colors = new List<Color?>();
        private bool[] bools;
        public HairColorOverride(string Zero, string One, string Two, string Three, string Four, string Five, string Six, bool[] bools)
        {
            Colors.Add(Calc.HexToColor(Zero));
            Colors.Add(Calc.HexToColor(One));
            Colors.Add(Calc.HexToColor(Two));
            Colors.Add(Calc.HexToColor(Three));
            Colors.Add(Calc.HexToColor(Four));
            Colors.Add(Calc.HexToColor(Five));
            Colors.Add(Calc.HexToColor(Six));
            this.bools = bools;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (IsVisible(scene as Level))
            {
                player = (scene as Level).Tracker.GetEntity<Player>();
                if (player is not null)
                {
                    for (int i = 0; i < Colors.Count; i++)
                    {
                        if (Colors[i].HasValue && bools[i])
                        {
                            if (player.Dashes == i)
                            {
                                player.OverrideHairColor = Colors[i];
                            }
                            else
                            {
                                player.OverrideHairColor = null;
                            }
                        }
                    }
                }
            }
        }
    }
}

