local calidusCutscene = {}

calidusCutscene.name = "PuzzleIslandHelper/CalidusCutsceneTrigger"

local cutscenes = {"FirstIntro","First", "FirstOutro","SecondIntro","Second"}
calidusCutscene.placements =
{
    {
        name = "Calidus Cutscene",
        data = {
            cutscene = "FirstIntro",
            flag = "",
            activateOnTransition = false,
        }
    },
}
calidusCutscene.fieldInformation = {
    cutscene = {
        options = cutscenes,
        editable = false
    }
}

return calidusCutscene