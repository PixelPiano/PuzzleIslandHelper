local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local directionalDashBlock = {}
local directions = {"Left","Right","Up","Down"}

directionalDashBlock.justification = { 0, 0 }

directionalDashBlock.name = "PuzzleIslandHelper/DirectionalDashBlock"
directionalDashBlock.minimumSize = {8,8}
directionalDashBlock.depth = -8501
directionalDashBlock.placements =
{
    name = "Directional Dash Block",
    data = {
        direction = "Left",
        tiletype = "3",
        width = 8,
        height = 8,
        blendin = true,
        permanent = false,
        canDash = true
    }
}
directionalDashBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions("tilesFg"),
            editable = false
        },
        direction = {
            options = directions,
            editable = false
        },
    }
end

directionalDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

function directionalDashBlock.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height)
end

return directionalDashBlock