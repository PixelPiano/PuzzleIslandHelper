local calidusCutscene = {}

calidusCutscene.name = "PuzzleIslandHelper/CalidusCutsceneTrigger"

calidusCutscene.placements =
{
    {
        name = "Calidus Cutscene",
        data = {
            cutscene = "FirstIntro",
            flag = "",
            activateOnTransition = false,
            startArgs = "",
            endArgs = "",
            oncePerInstance = false,
            oncePerSession = true
        }
    },
}

return calidusCutscene