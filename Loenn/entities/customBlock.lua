local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local customFallingBlock = {}

customFallingBlock.justification = { 0, 0 }

customFallingBlock.name = "PuzzleIslandHelper/CustomBlock"
customFallingBlock.minimumSize = {8,8}
customFallingBlock.depth = -8501
customFallingBlock.placements =
{
    name = "Custom Block",
    data = 
    {
        tiletype = "3",
        allowAnimatedTiles = true,
        fadeWhenInside = true,
        collisionFlags = "",
        fadeFlags = "",
        invertCollision = false,
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