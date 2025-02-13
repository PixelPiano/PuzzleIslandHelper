local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local memoryBlockade = {}

memoryBlockade.justification = { 0, 0 }

memoryBlockade.name = "PuzzleIslandHelper/MemoryBlockade"
memoryBlockade.minimumSize = {8,8}
memoryBlockade.nodeLimits = {1,1}
memoryBlockade.depth = -8501
memoryBlockade.nodeLineRenderType = "line"
memoryBlockade.nodeVisibility = "always"
memoryBlockade.placements =
{
    name = "Memory Blockade",
    data = 
    {
        tiletype = "3",
        path = "",
        colors = "",
        flags = "",
        requiredFlag = "",
        width = 8,
        height = 8
    }
}

memoryBlockade.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
memoryBlockade.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)
function memoryBlockade.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 
    local width, height = entity.width or 8, entity.height or 8

    return utils.rectangle(x, y, width, height), {utils.rectangle(nodeX, nodeY, width, height)}
end
return memoryBlockade