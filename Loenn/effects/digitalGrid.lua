local digital_grid = {}

digital_grid.name = "PuzzleIslandHelper/DigitalGrid"
digital_grid.fieldInformation = {
}

digital_grid.defaultData = {
    horizontalLineHeight = 2,
    verticalLineWidth = 4;
    rateX = 4,
    rateY=4,
    xSpacing = 24,
    ySpacing = 24,
    moving = false,
    color = "00ff00",
    verticalLineAngle = 10,
    horizontalLineAngle = 10,
    opacity = 1,
    verticalLines = true,
    horizontalLines = true,
    blur = true,
    glitch = false,
}
digital_grid.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return digital_grid