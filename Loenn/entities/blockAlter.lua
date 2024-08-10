local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local blockAlter = {}

blockAlter.justification = { 0, 0 }

blockAlter.name = "PuzzleIslandHelper/BlockAlter"
blockAlter.minimumSize = {8,8}
blockAlter.depth = 1
blockAlter.placements =
{
    name = "Block Alter",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8,
    }
}

blockAlter.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
blockAlter.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return blockAlter