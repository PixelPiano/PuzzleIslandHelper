local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local customFallingBlock = {}

customFallingBlock.justification = { 0, 0 }

customFallingBlock.name = "PuzzleIslandHelper/CustomFallingBlock"
customFallingBlock.minimumSize = {8,8}
customFallingBlock.depth = -8501
customFallingBlock.placements =
{
    name = "Custom Falling Block",
    data = 
    {
        flagOnImpact = "",
        flagOnTriggered = "",
        tangibleFlag = "",
        snapToFallenFlag = "",
        tiletype = "3",
        width = 8,
        height = 8,
        finalBoss = false,
        behind = false,
        climbFall = true
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