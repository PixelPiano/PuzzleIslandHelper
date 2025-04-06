local boxTeleport = {}

boxTeleport.name = "PuzzleIslandHelper/BoxTeleport"

local modes = {"LeftToRight","TopToBottom","None"}
local operators = {"GreaterThan","LessThan"}
boxTeleport.placements =
{
    {
        name = "Box Teleport",
        data = {
            room = "",
            flag = "",
            inverted = false,
            targetMarkerId = "",
            mode = "LeftToRight",
            operator = "GreaterThan",
            threshold = 0.0,
            snapToGround = true,
            activateIfEqual = false
        }
    },
}
boxTeleport.fieldInformation = 
{
    mode = 
    {
        options = modes,
        editable = false
    },
    operator = 
    {
        options = operators,
        editable = false
    }
}

return boxTeleport