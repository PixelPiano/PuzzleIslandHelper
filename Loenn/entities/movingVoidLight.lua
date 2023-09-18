local utils = require("utils")
local movingVoidLight = {}

movingVoidLight.justification = { 0, 0 }

movingVoidLight.name = "PuzzleIslandHelper/MovingVoidLight"
movingVoidLight.nodeLimits = {1,1}
movingVoidLight.nodeLineRenderType = "line"
movingVoidLight.minimumSize = {23,23}
movingVoidLight.depth = -8501
movingVoidLight.placements =
{
    name = "Moving Void Light",
    data = 
    {
        color = "ffff00",
        radius = 30,
        alpha = 1
    }
}
movingVoidLight.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}
movingVoidLight.texture = "objects/PuzzleIslandHelper/voidLight/lonn"

return movingVoidLight