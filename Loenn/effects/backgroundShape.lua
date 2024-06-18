local backgroundShape = {}

backgroundShape.name = "PuzzleIslandHelper/BackgroundShape"

backgroundShape.defaultData = 
{
    texturePath = "objects/PuzzleIslandHelper/cubeField/basic",
    size = 8,
    color = "FFFFFF",
    alpha = 1
}
backgroundShape.fieldInformation = 
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return backgroundShape