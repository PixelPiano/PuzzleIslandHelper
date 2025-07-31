local utils = require("utils")
local tileSetPuzzleController = {}

tileSetPuzzleController.justification = { 0, 0 }
tileSetPuzzleController.name = "PuzzleIslandHelper/TileSetPuzzleController"
tileSetPuzzleController.minimumSize = {8,8}
tileSetPuzzleController.depth = -8501
tileSetPuzzleController.placements =
{
    name = "Tile Set Puzzle Controller",
    data = 
    {
        width = 8,
        height = 8
    }
}
return tileSetPuzzleController