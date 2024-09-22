local voidLight = {}
voidLight.justification = { 0, 0 }

voidLight.name = "PuzzleIslandHelper/VoidLight"

voidLight.depth = -8500
voidLight.fillColor = {0.4, 0.4, 1.0, 0.4}
voidLight.borderColor = {0.4, 0.4, 1.0, 1.0}
voidLight.nodeLimits = {1, 1}
voidLight.nodeJustification = {0,0}
voidLight.nodeTexture = "objects/PuzzleIslandHelper/voidLight/lonn"
voidLight.nodeVisibility = "always"
voidLight.placements =
{
    {
        name = "Void Light",
        data = {
        width = 16,
        height = 16,
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