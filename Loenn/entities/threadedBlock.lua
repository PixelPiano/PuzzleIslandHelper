local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local threadedBlock = {}

threadedBlock.justification = { 0, 0 }

threadedBlock.name = "PuzzleIslandHelper/ThreadedBlock"
threadedBlock.minimumSize = {8,8}
threadedBlock.depth = -10000
threadedBlock.placements =
{
    name = "Threaded Block",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8,
        counterName = "",
        counterSpace = 16
    }
}

threadedBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
threadedBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return threadedBlock