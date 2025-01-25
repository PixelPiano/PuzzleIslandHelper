local drawableSprite = require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local utils = require("utils")
local forcefield= {}

forcefield.canResize={true,false}
forcefield.name = "PuzzleIslandHelper/CalidusForcefield"

local texture = "objects/PuzzleIslandHelper/calidusSymbol"

forcefield.depth = -10001
forcefield.minimumSize = {16,16}
forcefield.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
forcefield.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
forcefield.placements = {
    name = "Calidus Forcefield",
    data = {
        width = 8,
        height = 8,
        flagPrefix = ""
    }
}
function forcefield.sprite(room, entity)
    local shapeSprite = drawableSprite.fromTexture(texture, entity)
    shapeSprite:setScale(entity.width/64,entity.width/64)
    shapeSprite:setOffset(0,0)
    return shapeSprite
end
function forcefield.selection(room, entity)
    local offset = 0
    local size = entity.width or 16
    entity.height = size
    local main = utils.rectangle(entity.x + offset, entity.y + offset, size, size)
    return main
end


return forcefield