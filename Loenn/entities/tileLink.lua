local customTransition= {}
customTransition.name = "PuzzleIslandHelper/CustomTransition"

customTransition.depth = -8500

customTransition.texture = "objects/PuzzleIslandHelper/stool/stool00"
local sides = {"Up","Down","Left","Right"}
customTransition.placements =
{
    {
        name = "Custom Transition",
        data = 
        {
            flag = "",
            inverted = false,
            roomName = "",
            side = "Right"
        }
    }
}
customTransition.fieldInformation =
{
    side =
    {
        options = sides,
        editable = false
    }
}

return customTransition