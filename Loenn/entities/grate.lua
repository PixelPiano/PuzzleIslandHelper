local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local grate = {}
local defaultpath = "objects/PuzzleIslandHelper/grate/darker"
grate.name = "PuzzleIslandHelper/Grate"
grate.depth = -60
grate.minimumSize = {16, 8}
grate.canResize = {true, false}
grate.placements = {
    name = "Grate",
    data = {
        width = 16,
        path = "objects/PuzzleIslandHelper/grate/darker"
    }
}

-- Manual offsets and justifications of the sprites
function grate.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 16)
    local p = entity.path or defaultpath
    local leftSprite = drawableSprite.fromTexture(p, entity)

    leftSprite:useRelativeQuad(0, 0, 8, 8)

    table.insert(sprites, leftSprite)

    for i = 8, width-16, 8 do
        local middleSprite = drawableSprite.fromTexture(p, entity)
        middleSprite:addPosition(i, 0)
        middleSprite:useRelativeQuad(8, 0, 8, 8)

        table.insert(sprites, middleSprite)
    end

    local endSprite = drawableSprite.fromTexture(p, entity)
    endSprite:addPosition(width - 8, 0)
    endSprite:useRelativeQuad(16, 0, 8, 8)

    table.insert(sprites, endSprite)

    return sprites
end

function grate.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, math.max(entity.width, 16),8)
end

return grate