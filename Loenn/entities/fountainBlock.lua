local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local fountainBlock = {}

fountainBlock.justification = { 0, 0 }

fountainBlock.name = "PuzzleIslandHelper/FountainBlock"
fountainBlock.minimumSize = {8,8}
fountainBlock.depth = 1
fountainBlock.placements =
{
    name = "Fountain Block",
    data = 
    {
        tiletype = "w",
        width = 8,
        height = 8,
    }
}

fountainBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
fountainBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return fountainBlock