local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local customFallingBlock = {}

customFallingBlock.justification = { 0, 0 }

customFallingBlock.name = "PuzzleIslandHelper/TileshiftBlock"
customFallingBlock.minimumSize = {8,8}
customFallingBlock.depth = -8501
customFallingBlock.placements =
{
    name = "Tileshift Block",
    data = 
    {
        tiletype = "3",
        allowAnimatedTiles = true,
        blendIn = true,
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