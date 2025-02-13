local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local heartMachine = {}

heartMachine.justification = { 0, 0 }

heartMachine.name = "PuzzleIslandHelper/HeartMachine"
heartMachine.texture = "objects/PuzzleIslandHelper/heart/machine"
heartMachine.depth = 0
heartMachine.placements =
{
    name = "Heart Machine",
    data = 
    {
        flagOnComplete = "",
        flag = "",
        flags = "",
        inverted = false
    }
}
return heartMachine