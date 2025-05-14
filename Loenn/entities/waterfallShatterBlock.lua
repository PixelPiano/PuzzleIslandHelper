local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local waterfallShatterBlock = {}

waterfallShatterBlock.justification = { 0, 0 }

waterfallShatterBlock.name = "PuzzleIslandHelper/WaterfallShatterBlock"
waterfallShatterBlock.minimumSize = {8,8}
waterfallShatterBlock.depth = -8501
waterfallShatterBlock.placements =
{
    name = "Waterfall Shatter Block",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8,
        visibleFlag = "",
        invertVisibleFlag = false,
        flagOnShatter = "",
        invertFlagOnShatter = false,
        blendIn = true,
        canDash = true,
        permenant = true
    }
}

waterfallShatterBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
waterfallShatterBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return waterfallShatterBlock