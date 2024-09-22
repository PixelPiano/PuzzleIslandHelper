local digitalField = {}

digitalField.name = "PuzzleIslandHelper/DigitalField"

digitalField.defaultData = {
    
    backColor = "00ff00",
    frontColor = "00ff00",
    xSpacing = 16,
    ySpacing = 16,
    layers = 4
}
digitalField.fieldInformation =
{
    backColor =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    frontColor =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return digitalField