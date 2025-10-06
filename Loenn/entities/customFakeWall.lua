local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local customFakeWall = {}

customFakeWall.justification = { 0, 0 }

customFakeWall.name = "PuzzleIslandHelper/CustomFakeWall"
customFakeWall.minimumSize = {8,8}
function customFakeWall.depth(room, entity)
    return entity.depth
end
customFakeWall.placements =
{
    name = "Custom Fake Wall",
    data = 
    {
        tiletype = "3",
        allowAnimatedTiles = true,
        canWalkThroughFlag = "",
        canFadeFlag = "",
        requirePlayerAndFadeFlag = false,
        fadeFrom = 1,
        fadeTo = 0,
        depth = -13000,
        permanent = false,
        flag = "",
        audioEventOnEnter = "event:/game/general/secret_revealed",
        audioEventOnLeave = "",
        blendIn = true,
        width = 8,
        height = 8,
        onlyCheckFlagOnAwake = false
    }
}

customFakeWall.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
customFakeWall.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return customFakeWall