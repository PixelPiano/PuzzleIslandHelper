local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local labFloatyBlock = {}

labFloatyBlock.justification = { 0, 0 }

labFloatyBlock.name = "PuzzleIslandHelper/CustomFloatingBlock"
labFloatyBlock.minimumSize = {8,8}
labFloatyBlock.depth = 0
labFloatyBlock.placements =
{
    name = "Lab Floaty Block",
    data = 
    {
        flag = "lab_floaty_block",
        tiletype = '3',
        width = 8,
        height = 8,
        blockIndex = 0,
        useRandomPreset = true,
        tiles = ""
    }
}

labFloatyBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        }
    }
end
labFloatyBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return labFloatyBlock