local utils = require("utils")
local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local riftLine = {}

riftLine.justification = { 0, 0 }

riftLine.name = "PuzzleIslandHelper/RiftLine"

riftLine.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
riftLine.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
riftLine.depth = -10001
riftLine.nodeLimits = {1,1}
riftLine.nodeLineRenderType = "line"
riftLine.nodeVisibility = "always"
riftLine.canResize = {false, false}
function riftLine.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 

    return utils.rectangle(x, y, 8, 8), {utils.rectangle(nodeX, nodeY, 8, 8)}
end
riftLine.placements =
{
    {
        name = "Rift Line",
        data = {
        }
    }

}

return riftLine