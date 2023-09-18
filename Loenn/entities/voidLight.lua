local voidLight = {}
voidLight.justification = { 0, 0 }

voidLight.name = "PuzzleIslandHelper/VoidLight"

voidLight.depth = -8500

voidLight.texture = "objects/PuzzleIslandHelper/voidLight/lonn"

voidLight.placements =
{
    {
        name = "Void Light",
        data = {
        color = "ffff00",
        radius = 30,
        alpha = 1
        }
    }
}
voidLight.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return voidLight