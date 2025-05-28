local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local movingBlock = {}

movingBlock.justification = { 0, 0 }

movingBlock.name = "PuzzleIslandHelper/MovingBlock"
movingBlock.minimumSize = {8,8}
movingBlock.depth = 1
movingBlock.nodeLimits = {1, 1}
movingBlock.nodeLineRenderType = "line"
local activationTypes = {"Never","Always","PlayerRiding","PlayerClimbing","PlayerRidingOrClimbing","ActorRiding","Flag","DashCollide","PlayerDie","PlayerRespawn","Awake"}
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