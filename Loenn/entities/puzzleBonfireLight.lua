local puzzleBonfireLight= {}
puzzleBonfireLight.justification = { 0, 0 }

puzzleBonfireLight.name = "PuzzleIslandHelper/PuzzleBonfireLight"

puzzleBonfireLight.depth = -100000

puzzleBonfireLight.texture = "objects/PuzzleIslandHelper/puzzleBonfireLight/idle00"

puzzleBonfireLight.placements =
{
    {
        name = "Lab Floor Light",
        data = 
        {
            color = "FFFFFF",
            lightFadeStart = 32,
            lightFadeEnd = 64,
            bloomRadius = 32,
            baseBrightness = 0.5,
            brightnessVariance = 0.5,
            flashFrequency = 0.25,
            wigglerDuration = 4,
            wigglerFrequency = 0.2,
        }
    }
}
puzzleBonfireLight.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return puzzleBonfireLight