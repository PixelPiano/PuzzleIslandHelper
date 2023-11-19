local sceneSwitch = {}

sceneSwitch.name = "PuzzleIslandHelper/SceneSwitch"

local transitions = {"Forest","Resort","Home"}
sceneSwitch.placements =
{
    {
        name = "Scene Switch",
        data = {
            transition = "Forest",
            templeOnLeft = true
        }
    },
}
sceneSwitch.fieldInformation = 
{
    transition = 
    {
        options = transitions,
        editable = false
    }
}

return sceneSwitch