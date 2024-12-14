local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")

local machine= {}
machine.justification = { 0, 0 }

machine.name = "PuzzleIslandHelper/HatchMachineNode"

machine.depth = -8500

machine.texture = "objects/PuzzleIslandHelper/machines/hatchMachine/node"

machine.placements =
{
    {
        name = "Hatch Machine Node",
        data = 
        {
            index = 0,
            tiletype = "3"
        }
    }
}
machine.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
return machine