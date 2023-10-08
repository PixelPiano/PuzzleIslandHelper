local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local labHangingLamp = {}
local path = "objects/hanginglamp"
labHangingLamp.name = "PuzzleIslandHelper/LabHangingLamp"
labHangingLamp.depth = 2000
labHangingLamp.minimumSize = {0, 16}
labHangingLamp.canResize = {false, true}
labHangingLamp.placements = {
    name = "Lab Hanging Lamp",
    data = {
        height = 16,
        opacity = 1,
        staticOpacity = false
    }
}

-- Manual offsets and justifications of the sprites
function labHangingLamp.sprite(room, entity)
    local sprites = {}
    local height = math.max(entity.height or 0, 16)

    local topSprite = drawableSprite.fromTexture(path, entity)

    topSprite:setJustification(0, 0)
    topSprite:setOffset(0, 0)
    topSprite:useRelativeQuad(0, 0, 8, 8)

    table.insert(sprites, topSprite)

    for i = 0, height - 16, 8 do
        local middleSprite = drawableSprite.fromTexture(path, entity)

        middleSprite:setJustification(0, 0)
        middleSprite:setOffset(0, 0)
        middleSprite:addPosition(0, i)
        middleSprite:useRelativeQuad(0, 8, 8, 8)

        table.insert(sprites, middleSprite)
    end

    local bottomSprite = drawableSprite.fromTexture(path, entity)

    bottomSprite:setJustification(0, 0)
    bottomSprite:setOffset(0, 0)
    bottomSprite:addPosition(0, height - 8)
    bottomSprite:useRelativeQuad(0, 16, 8, 8)

    table.insert(sprites, bottomSprite)

    return sprites
end

function labHangingLamp.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, math.max(entity.height, 16))
end

return labHangingLamp