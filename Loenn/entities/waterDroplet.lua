local waterDroplet = {}

local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

waterDroplet.justification = { 0, 0 }

waterDroplet.name = "PuzzleIslandHelper/WaterDroplet"

waterDroplet.depth = -8500
waterDroplet.texture = "objects/PuzzleIslandHelper/droplet"
local dirs = {"Right","Up","Left","Down"}

waterDroplet.placements =
{
    {
        name = "Water Droplet",
        data = 
        {
            interval = 0.5,
            delay = 0,
            randomWaitTime = true,
            baseColor = "0000ff",
            direction = "Down"
        }
    }
}
waterDroplet.fieldInformation = 
{
    baseColor=
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    direction =
    {
        options = dirs,
        editable = false
    }
}

return waterDroplet