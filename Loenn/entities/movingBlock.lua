local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local movingBlock = {}

movingBlock.justification = { 0, 0 }

movingBlock.name = "PuzzleIslandHelper/MovingBlock"
movingBlock.minimumSize = {8,8}
movingBlock.depth = 1
local activationTypes = {"Always","Player Riding","Player Climbing","Player Riding or Climbing","Actor Riding","Flag","Dash Collide","Player Die","Player Respawn","Awake"}
movingBlock.placements =
{
    name = "Moving Block",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8,
        activationType = "Flag",
        flag = "",
        inverted = false,
        shakes = false,
        permanent = false,
        collidable = true,
        moveLimit = -1
    }
}

movingBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        activationType = {
            options = activationTypes,
            editable = false
        },
    }
end
movingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return movingBlock