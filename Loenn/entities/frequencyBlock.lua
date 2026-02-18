local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local frequencyBlock = {}

frequencyBlock.justification = { 0, 0 }

frequencyBlock.name = "PuzzleIslandHelper/FrequencyBlock"
frequencyBlock.minimumSize = {8,8}
frequencyBlock.placements =
{
    name = "Frequency Block",
    data = 
    {
        tiletype = "3",
        permanent = false,
        blendIn = true,
        rate1 = 0,
        rate2 = 0,
        rate3 = 0,
        rate4 = 0,
        width = 8,
        height = 8,
    }
}

frequencyBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
frequencyBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return frequencyBlock