local triField = {}

triField.name = "PuzzleIslandHelper/TriangleField"

local directions = {"Left","Right","Up","Down"}
local triangleTypes = {"Equilateral","Isosceles","Random","Duel"}
triField.defaultData = 
{
    colors = "FFFFFF,FF0000, 00FF00",
    texturePath = "objects/PuzzleIslandHelper/cubeField/basic",
    minSpeed = 8,
    maxSpeed = 50,
    minRotateRate = 0.1,
    maxRotateRate = 0.6,
    maxSize = 16,
    minSize = 8,
    maxAlpha = 1,
    minAlpha = 0.5,
    direction = "Left",
    triangleType = "Equilateral"
}
triField.fieldInformation =
{
    direction =
    {
        options = directions,
        editable = false
    },
    triangleType =
    {
        options = triangleTypes,
        editable = false
    }
}

return triField