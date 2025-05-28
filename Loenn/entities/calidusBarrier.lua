local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local _q = {}

_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/CalidusBarrier"

_q.depth = -10001
_q.texture = "objects/PuzzleIslandHelper/securityLaser/emitter00"
_q.nodeLimits = {1,1}
_q.nodeLineRenderType = "line"
_q.nodeVisibility = "always"
_q.canResize = {false, false}
function _q.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 

    return utils.rectangle(x, y, 8, 8), {utils.rectangle(nodeX, nodeY, 8, 8)}
end
_q.placements =
{
    {
        name = "Calidus Barrier",
        data = {
        flag = "",
        color = "Magenta",
        inverted = false
        }
    }
}
_q.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return _q