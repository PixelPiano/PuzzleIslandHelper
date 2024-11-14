local calidusFollowTrigger = {}

calidusFollowTrigger.name = "PuzzleIslandHelper/CalidusFollowTrigger"

local modes = {"OnEnter","OnStay","OnLeave","OnLevelStart"}

calidusFollowTrigger.placements =
{
    {
        name = "Calidus Follow Trigger",
        data = {
            mode = "OnEnter",
            flag = "",
            inverted = false
        }
    },
}
calidusFollowTrigger.fieldInformation = {
    mode = {
        options = modes,
        editable = false
    }
}

return calidusFollowTrigger