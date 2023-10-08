local monitorDecal= {}
monitorDecal.justification = { 0, 0 }

monitorDecal.name = "PuzzleIslandHelper/MonitorDecal"

monitorDecal.depth = 9001

monitorDecal.texture = "objects/PuzzleIslandHelper/machines/gizmos/lonn00"

monitorDecal.placements =
{
    {
        name = "Monitor Decal",
        data = 
        {
            inScreenDecal="",
            lightDelay = 1,
            lightStartOffset = 0,
            onLight = true,
            hasRays = true,
            buttons = true,
            scaleDecal = false,
            screenColor = "000000",
            groupId = ""

        }
    }
}
monitorDecal.fieldInformation=
{
    screenColor =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return monitorDecal