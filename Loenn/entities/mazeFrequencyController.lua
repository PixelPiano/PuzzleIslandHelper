local mazeFrequencyController= {}
mazeFrequencyController.justification = { 0, 0 }

mazeFrequencyController.name = "PuzzleIslandHelper/MazeFrequencyController"

mazeFrequencyController.depth = -1

mazeFrequencyController.texture = "objects/PuzzleIslandHelper/controllerSmall"

mazeFrequencyController.placements =
{
    {
        name = "Maze Frequency Controller",
        data = 
        {
            frequency = 0,
            warpToMazeClone = false
        }
    }
}

return mazeFrequencyController