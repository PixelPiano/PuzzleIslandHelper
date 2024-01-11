using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Gameshow
{
    public class RandomRoutines
    {
        public static List<string> ValidIDs = new()
        {
            "q36U","q33U","q39U","q38U"
        };
        public RandomRoutines()
        {

        }
        public IEnumerator Routine(string id, Cutscenes.Gameshow gameshow)
        {
            switch (id)
            {
                case "q36U":
                    yield return Textbox.Say(id + "a");
                    yield return 4;
                    yield return gameshow.LoseLife();
                    yield return Textbox.Say(id + "b");
                    break;
                case "q33U":
                    yield return Textbox.Say(id + "a");
                    yield return 2;
                    yield return Textbox.Say(id + "b");
                    break;
                default: yield return Textbox.Say(id); break;
            }
            yield return null;
        }
    }
}
