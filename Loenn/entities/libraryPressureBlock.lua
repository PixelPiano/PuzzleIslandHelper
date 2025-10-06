local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local libraryPressureBlock = {}

libraryPressureBlock.justification = { 0, 0 }

libraryPressureBlock.name = "PuzzleIslandHelper/LibraryPressureBlock"
libraryPressureBlock.minimumSize = {16,16}
libraryPressureBlock.depth = -8501
libraryPressureBlock.placements =
{
    name = "Library Pressure Block",
    data = 
    {
        tiletype = "9",
        requiredFlags = "",
        key = "0",
        width = 8,
        height = 8
    }
}

libraryPressureBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
libraryPressureBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return libraryPressureBlock