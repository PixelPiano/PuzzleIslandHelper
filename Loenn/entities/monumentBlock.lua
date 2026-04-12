local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local monumentBlock = {}

monumentBlock.justification = { 0, 0 }

monumentBlock.name = "PuzzleIslandHelper/MonumentBlock"
monumentBlock.minimumSize = {8,8}
monumentBlock.depth = -8501
monumentBlock.placements =
{
    name = "Monument Block",
    data = 
    {
        shiftID = "",
        tiletype = "3",
        blendIn = true,
        width = 8,
        height = 8
    }
}

monumentBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
monumentBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return monumentBlock