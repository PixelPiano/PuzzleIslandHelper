local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local animatedDashBlock = {}

animatedDashBlock.justification = { 0, 0 }

animatedDashBlock.name = "PuzzleIslandHelper/FlagDashBlock"
animatedDashBlock.minimumSize = {8,8}
animatedDashBlock.placements =
{
    name = "Flag Dash Block",
    data = 
    {
        tiletype = "3",
        centerSprite = "",
        allowAnimatedTiles = true,
        permanent = false,
        blendIn = true,
        width = 8,
        height = 8,
        flagOnBreak = "",
        canDashFlag = "",
        canBoosterFlag = "",
        spriteVisibleFlag = "",
        spriteActiveFlag = "",
        flag = "",
        flagAffectActive = true,
        flagAffectVisible = true,
        flagAffectCollision = true,
        disableLightsInside = true

    }
}

animatedDashBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
animatedDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return animatedDashBlock