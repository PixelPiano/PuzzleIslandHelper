local piCutscene = {}

piCutscene.name = "PuzzleIslandHelper/PICutscene"

local cutscenes = {"Prologue","WarpToCalidus","Calidus1","GetInvert","GrassShift","Gameshow","Fountain","End","TEST"}
piCutscene.placements =
{
    {
        name = "PICutscene",
        data = {
        cutscene = "Prologue",
        part = 1,
        flag = "",
        oncePerRoom = false,
        oncePerPlaythrough = false,
        activateOnTransition = false,
        assignableInt = 0
        }
    },
}
piCutscene.fieldInformation = {
    cutscene = {
        options = cutscenes,
        editable = false
    }
}

return piCutscene