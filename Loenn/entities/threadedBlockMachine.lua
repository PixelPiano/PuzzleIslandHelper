local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local threadedBlock = {}

threadedBlock.justification = { 0, 0 }

threadedBlock.name = "PuzzleIslandHelper/ThreadedBlockMachine"
threadedBlock.depth = -10000
threadedBlock.texture = "objects/PuzzleIslandHelper/threadedBlock/machine"
threadedBlock.nodeTexture = "objects/PuzzleIslandHelper/threadedBlock/lonnScreen"
threadedBlock.nodeLimits = {1,1}
threadedBlock.nodeLineRenderType = "line"
threadedBlock.nodeVisibility = "always"
threadedBlock.nodeJustification = {0,0}
threadedBlock.placements =
{
    name = "Threaded Block Machine",
    data = 
    {
        counterNameA = "",
        counterNameB = "",
        counterNameC = ""
    }
}

return threadedBlock