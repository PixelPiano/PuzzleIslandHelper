local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local floatyAlterBlock = {}

floatyAlterBlock.justification = { 0, 0 }

floatyAlterBlock.name = "PuzzleIslandHelper/FloatyAlterBlock"
floatyAlterBlock.minimumSize = {8,8}
floatyAlterBlock.depth = 0
floatyAlterBlock.placements =
{
    name = "Floaty Alter Block",
    data = 
    {
        tiletype = '3',
        width = 8,
        height = 8,
        tileData = "",
        disableSpawnOffset = false,
        group = 0,
        randomSeed = -1
    }
}

floatyAlterBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        }
    }
end
floatyAlterBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return floatyAlterBlock