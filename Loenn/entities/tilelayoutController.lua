
local tileLayoutController = {}

tileLayoutController.justification = { 0, 0 }

tileLayoutController.name = "PuzzleIslandHelper/TileLayoutController"
tileLayoutController.texture = "objects/PuzzleIslandHelper/tileLayoutControllerLonn"
tileLayoutController.depth = 1

tileLayoutController.placements =
{
    {
        name = "Tile Layout Controller",
        data = {
            copyFromRoom = ""
        }
    }
}
return tileLayoutController