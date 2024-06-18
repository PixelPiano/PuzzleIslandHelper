local cubeField = {}

cubeField.name = "PuzzleIslandHelper/CubeField"

cubeField.defaultData = 
{
    texturePath = "objects/PuzzleIslandHelper/cubeField/basic",
    layers = 4,
    cubeSize = 8,
    color = "FFFFFF",
    alpha = 1
}
cubeField.fieldInformation = 
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return cubeField