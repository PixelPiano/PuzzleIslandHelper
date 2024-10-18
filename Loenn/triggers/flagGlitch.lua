local sceneSwitch = {}

sceneSwitch.name = "PuzzleIslandHelper/FlagGlitch"

local sides = {"Left","Right","Top","Bottom"}
sceneSwitch.placements =
{
    {
        name = "Scene Switch",
        data = {
            flagA = "",
            flagB = "",
            flagAState = false,
            flagBState = false,
            sideA = "Left",
            sideB = "Right",
        }
    },
}
sceneSwitch.fieldInformation = 
{
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