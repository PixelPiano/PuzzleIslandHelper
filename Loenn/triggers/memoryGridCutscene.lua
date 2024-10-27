local memoryGridCutscene = {}

memoryGridCutscene.name = "PuzzleIslandHelper/MemoryGridCutscene"

local cutscenes = {"Blocks1","Settle","Lookout","ShakeAgain","Blocks2"}
memoryGridCutscene.placements =
{
    {
        name = "Memory Grid Cutscene",
        data = {
            cutscene = "Blocks1",
            requiredFlag = "",
            flagOnEnd = ""
        }
    },
}
memoryGridCutscene.fieldInformation = {
    cutscene = {
        options = cutscenes,
        editable = false
    }
}

return memoryGridCutscene