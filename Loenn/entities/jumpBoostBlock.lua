local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local jumpBoostBlock = {}

jumpBoostBlock.justification = { 0, 0 }

jumpBoostBlock.name = "PuzzleIslandHelper/JumpBoostBlock"
jumpBoostBlock.minimumSize = {8,8}
jumpBoostBlock.depth = -8501
jumpBoostBlock.placements =
{
    name = "Jump Boost Block",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8
    }
}

jumpBoostBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
jumpBoostBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return jumpBoostBlock