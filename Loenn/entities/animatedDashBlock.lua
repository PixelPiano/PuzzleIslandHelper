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
        allowAnimatedTiles = true,
        permanent = false,
        blendIn = true,
        width = 8,
        height = 8,
        canDash = true,
        flagOnBreak = "",
        canDashFlag = "",
        canBoosterFlag = "",
        visibleFlag = "",
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