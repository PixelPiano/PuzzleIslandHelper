local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local invertBlock = {}

invertBlock.justification = { 0, 0 }

invertBlock.name = "PuzzleIslandHelper/InvertBlock"
invertBlock.minimumSize = {8,8}
invertBlock.depth = -8501
invertBlock.placements =
{
    name = "Invert Block",
    data = {

        flag = "invert",
        startState = false,
        invertFlag = false,
        tiletype = "3",
        width = 8,
        height = 8,
        solidWhenNormal = true,
        solidWhenFlipped = true,
        canSwitch = false
    }
}

invertBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

invertBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

function invertBlock.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height)
end

return invertBlock