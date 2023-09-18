local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local labFallingBlock = {}

labFallingBlock.justification = { 0, 0 }

labFallingBlock.name = "PuzzleIslandHelper/LabFallingBlock"
labFallingBlock.minimumSize = {8,8}
labFallingBlock.depth = 0
labFallingBlock.placements =
{
    name = "Lab Falling Block",
    data = 
    {
        flag = "lab_falling_block",
        tiletype = '3',
        width = 8,
        height = 8,
        useRandomPreset = true,
        tiles = ""
    }
}

labFallingBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        }
    }
end
labFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return labFallingBlock