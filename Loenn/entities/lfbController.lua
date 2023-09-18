local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local lfbController = {}


lfbController.name = "PuzzleIslandHelper/LFBController"
lfbController.minimumSize = {8,8}
lfbController.depth = 0
lfbController.texture = "objects/PuzzleIslandHelper/access/artifactHolder00"

lfbController.placements =
{
    name = "Lab Falling Block Controller",
    data = 
    {
        flag = "lab_falling_block_controller",
        tiletype = '3',
        invertFlag = false
    }
}

lfbController.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        }
    }
end

return lfbController