local puzzleSpotlight = {}
puzzleSpotlight.justification = { 0, 0 }

puzzleSpotlight.name = "PuzzleIslandHelper/PuzzleSpotlight"

puzzleSpotlight.depth = -1000001

puzzleSpotlight.texture = "objects/PuzzleIslandHelper/graph/spotlightIcon"

puzzleSpotlight.placements =
{
    {
        name = "Puzzle Spotlight",
        data = {
            flag = "spotlightFlag",
            secondDesign = false,
            Colorgrade = "PianoBoy/inverted",
            startingState = false,
            centerRadius = 30,
            beamLength = 320,
            beamWidth = 5,
            beams = 4,
            rotationRate = 0,
            gapLength = 0,
            offsetRate = 0,
            segmentedBeams = true,
            hasOffset = false,
        }
    }
}

return puzzleSpotlight
