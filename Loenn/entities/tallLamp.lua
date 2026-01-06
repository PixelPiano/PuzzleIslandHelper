local tallLamp= {}
tallLamp.justification = { 0, 0 }

tallLamp.name = "PuzzleIslandHelper/TallLamp"

tallLamp.depth = 1

tallLamp.texture = "objects/PuzzleIslandHelper/tallLampOff"

tallLamp.placements =
{
    {
        name = "Tall Lamp",
        data = 
        {
            flagSetOnInteract = "",
            canInteractFlag = "",
            lightRadius = 64,
            lightAlpha = 1,
            bloomAlpha = 1,
            playSound = true,
            instant = false,
            color = "FFFFFF",

        }
    }
}
tallLamp.fieldInformation =
{
    color = 
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return tallLamp