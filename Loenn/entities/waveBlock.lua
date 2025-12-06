local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local waveBlock = {}

waveBlock.justification = { 0, 0 }

waveBlock.name = "PuzzleIslandHelper/WaveBlock"
waveBlock.minimumSize = {8,8}
waveBlock.nodeLimits = {1, 1}
waveBlock.nodeLineRenderType = "line"
waveBlock.depth = -13000
waveBlock.placements =
{
    name = "Wave Block",
    data = 
    {
        tiletype = "3",
        flag = "",
        fromAbove = false,
        blendIn = true,
        overshoot = 8,
        width = 8,
        height = 8,
        delay = 0,
        flagOnEnd = "",
        duration = 1
    }
}
function waveBlock.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 

    return utils.rectangle(x, y, entity.width, entity.height), {utils.rectangle(nodeX, nodeY, entity.width, entity.height)}
end
waveBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        colorA = {
            fieldType = "color"
        },
        colorB = {
            fieldType = "color"
        }
    }
end
waveBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return waveBlock