local frequencyOrb = {}

frequencyOrb.justification = { 0, 0 }
frequencyOrb.name = "PuzzleIslandHelper/FrequencyOrb"
frequencyOrb.texture = "objects/PuzzleIslandHelper/forkAmp/orb"
frequencyOrb.nodeLimits = {1,1}
frequencyOrb.nodeVisibility = "always"
frequencyOrb.nodeLineRenderType = "line"
frequencyOrb.placements =
{
    name = "Frequency Orb",
    data = 
    {
        useRate0 = false,
        useRate1 = false,
        useRate2 = false,
        useRate3 = false,
        color = "FFFFFF",
        edge = "0000FF",
        scale = 1,
        alwaysActive = true,
        fadeWhenNotPlaying = false
    }
}

frequencyOrb.fieldInformation =
{
    color = {
        fieldType = "color",
        allowXNAColors = true,
    },
    edge = {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return frequencyOrb