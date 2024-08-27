local memoryScreen = {}

memoryScreen.name = "PuzzleIslandHelper/MemoryScreen"
memoryScreen.depth = 1
memoryScreen.texture = "objects/PuzzleIslandHelper/memoryScreen/lonn"
memoryScreen.justification = {0.5,0.5}
memoryScreen.nodeTexture = "objects/PuzzleIslandHelper/memoryScreen/lonn"
memoryScreen.nodeLimits = {0,1}
memoryScreen.nodeLineRenderType = "line"
memoryScreen.nodeVisibility = "always"

memoryScreen.placements = {
    name = "Memory Screen",
    data = {
        flag = "",
        color = "00FF00"
    }
}
memoryScreen.fieldInformation = 
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true
    }
}

return memoryScreen