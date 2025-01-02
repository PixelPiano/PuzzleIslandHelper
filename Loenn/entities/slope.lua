local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local slope = {}

slope.justification = { 0, 0 }

slope.name = "PuzzleIslandHelper/Slope"

slope.depth = -10001
slope.texture = "objects/PuzzleIslandHelper/securityLaser/emitter00"
slope.nodeLimits = {1,1}
slope.nodeLineRenderType = "line"
slope.nodeVisibility = "always"
slope.canResize = {false, false}
function slope.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 

    return utils.rectangle(x, y, 8, 8), {utils.rectangle(nodeX, nodeY, 8, 8)}
end
slope.placements =
{
    {
        name = "Slope",
        data = {
        }
    }

}

return slope