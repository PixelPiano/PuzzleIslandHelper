local spinnerBurst = {}

local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

spinnerBurst.justification = { 0, 0 }

spinnerBurst.name = "PuzzleIslandHelper/SpinnerBurst"

spinnerBurst.depth = -8500
spinnerBurst.texture = "objects/PuzzleIslandHelper/spinnerBurst"

spinnerBurst.placements =
{
    {
        name = "Spinner Burst",
        data = 
        {
            color = "0000FF",
            flag = "",
            inverted = false,
            persistent = false,
            delay = 0
        }
    }
}
spinnerBurst.fieldInformation = 
{
    color=
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return spinnerBurst