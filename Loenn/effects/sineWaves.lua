local sineWaves = {}

sineWaves.name = "PuzzleIslandHelper/SineWaves"

sineWaves.defaultData = {
    verticalChance = 0.5,
    minCurve = 4,
    maxCurve = 8,
    minWaveLength = 16,
    maxWaveLength = 32,
    maxLines = 12,
    color = "FFFFFF"
}
sineWaves.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return sineWaves