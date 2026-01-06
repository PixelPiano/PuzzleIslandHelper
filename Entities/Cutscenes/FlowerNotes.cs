using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{

    [CustomEvent("PuzzleIslandHelper/FlowerNotes")]
    public class FlowerNotes : CutsceneEntity
    {
        private static readonly string[] choices = ["FlowerNotesChoiceA", "FlowerNotesChoiceB", "FlowerNotesChoiceC", "FlowerNotesChoiceD", "FlowerNotesExit"];
        public override void OnBegin(Level level)
        {
            level.DisableMovement();
            Add(new Coroutine(routine()));
        }

        public override void OnEnd(Level level)
        {
            level.EnableMovement();
        }
        private IEnumerator routine()
        {
            yield return Textbox.Say("FlowerNotesHub");
            while (true)
            {
                yield return ChoicePrompt.Prompt(choices);
                if (ChoicePrompt.Choice < 4)
                {
                    yield return Textbox.Say(choices[ChoicePrompt.Choice].Replace("Choice", ""));
                }
                else
                {
                    break;
                }
            }
            EndCutscene(Level);
        }
    }
}
