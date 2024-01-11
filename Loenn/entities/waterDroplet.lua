local waterDroplet = {}

local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

waterDroplet.justification = { 0, 0 }

waterDroplet.name = "PuzzleIslandHelper/WaterDroplet"

waterDroplet.depth = -8500
waterDroplet.nodeLimits = {1,1}
waterDroplet.nodeLineRenderType = "line"
waterDroplet.nodeVisibility = "always"
waterDroplet.canResize = {false, false}
function waterDroplet.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 

    return utils.rectangle(x, y, 8, 8), {utils.rectangle(nodeX, nodeY, 8, 8)}
end
waterDroplet.texture = "objects/PuzzleIslandHelper/waterDroplet/lonn"
local dirs = {"Right","Up","Left","Down"}

waterDroplet.placements =
{
    {
        name = "Water Droplet",
        data = 
        {
            waitTime = 0.5,
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