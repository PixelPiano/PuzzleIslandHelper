local sceneSwitch = {}

sceneSwitch.name = "PuzzleIslandHelper/SceneSwitch"

local areas = {"Forest","Resort","Backend","Pipes","Golden","Void","None"}
local sides = {"Left","Right","Top","Bottom"}
sceneSwitch.placements =
{
    {
        name = "Scene Switch",
        data = {
            areaA = "None",
            areaB = "None",
            sideA = "Left",
            sideB = "Right",
        }
    },
}
sceneSwitch.fieldInformation = 
{
    areaA = 
    {
        options = areas,
        editable = false
    },
        areaB = 
    {
        options = areas,
        editable = false
    },
        sideA = 
    {
        options = sides,
        editable = false
    },
        sideB = 
    {
        options = sides,
        editable = false
    },
}

return sceneSwitch