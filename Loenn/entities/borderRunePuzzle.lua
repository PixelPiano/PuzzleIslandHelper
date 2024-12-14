local rotator= {}
rotator.justification = { 0, 0 }

rotator.name = "PuzzleIslandHelper/BorderRunePuzzle"

rotator.depth = 1
rotator.minimumSize = {8, 16}
rotator.canResize = {false, false}
rotator.texture = "objects/PuzzleIslandHelper/borderRune/borderRunePuzzle"

rotator.placements =
{
    {
        name = "Border Rune Puzzle",
        data = 
        {
            width = 8,
            height = 16,
            index = 0
        }
    }
}

return rotator