using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
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
        public IEnumerator Routine(string id, Gameshow gameshow)
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
