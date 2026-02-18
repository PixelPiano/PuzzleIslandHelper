local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local remoteDashBlock = {}

remoteDashBlock.justification = { 0, 0 }

remoteDashBlock.name = "PuzzleIslandHelper/RemoteDashBlock"
remoteDashBlock.minimumSize = {8,8}
remoteDashBlock.placements =
{
    name = "Remote Dash Block",
    data = 
    {
        useCutscene = false,
        cutsceneOnTransition = false,
        shakeBeforeBreak = false, 
        tiletype = "3",
        centerSprite = "",
        allowAnimatedTiles = true,
        permanent = false,
        blendIn = true,
        width = 8,
        height = 8,
        shakeTime = -1,
        flagOnBreak = "",
        canDashFlag = "",
        canBoosterFlag = "",
        forceShakeFlag = "",
        forceBreakFlag = "",
        spriteVisibleFlag = "",
        spriteActiveFlag = "",
        flag = "",
        flagAffectActive = true,
        flagAffectVisible = true,
        flagAffectCollision = true,
        disableLightsInside = true
    }
}

remoteDashBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
remoteDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return remoteDashBlock