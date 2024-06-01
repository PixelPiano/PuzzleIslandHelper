local puzzleBonfireLight= {}
puzzleBonfireLight.justification = { 0, 0 }

puzzleBonfireLight.name = "PuzzleIslandHelper/PuzzleBonfireLight"

puzzleBonfireLight.depth = -100000

puzzleBonfireLight.texture = "objects/PuzzleIslandHelper/puzzleBonfireLight/lightOff00"

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