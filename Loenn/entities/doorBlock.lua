local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local customFallingBlock = {}

customFallingBlock.justification = { 0, 0 }

customFallingBlock.name = "PuzzleIslandHelper/DoorBlock"
customFallingBlock.minimumSize = {8,8}
customFallingBlock.depth = -8501
customFallingBlock.nodeLimits = {1, 1}
customFallingBlock.nodeLineRenderType = "line"
customFallingBlock.placements =
{
    name = "Door Block",
    data = 
    {
        tiletype = "3",
        blendIn = true,
        flag = "",
        inverted = false,
        persistent = false,
        width = 8,
        height = 8
    }
}

customFallingBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
customFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return customFallingBlock