local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ruinsJumpThru = {}
local path = "objects/PuzzleIslandHelper/ruinsJumpThru/texture"
ruinsJumpThru.name = "PuzzleIslandHelper/RuinsJumpThru"
ruinsJumpThru.depth = -60
ruinsJumpThru.minimumSize = {8, 4}
ruinsJumpThru.canResize = {true, false}
ruinsJumpThru.placements = {
    name = "Ruins Jump Thru",
    data = {
        width = 8,
        collidable = true
    }
}

-- Manual offsets and justifications of the sprites
function ruinsJumpThru.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 4)

    local leftSprite = drawableSprite.fromTexture(path, entity)

    leftSprite:useRelativeQuad(0, 0, 8, 4)

    table.insert(sprites, leftSprite)

    for i = 8, width-8, 8 do
        local middleSprite = drawableSprite.fromTexture(path, entity)
        middleSprite:addPosition(i, 0)
        middleSprite:useRelativeQuad(8, 0, 8, 4)

        table.insert(sprites, middleSprite)
    end

    local endSprite = drawableSprite.fromTexture(path, entity)
    endSprite:addPosition(width, 0)
    endSprite:useRelativeQuad(16, 0, 2, 4)

    table.insert(sprites, endSprite)

    return sprites
end

function ruinsJumpThru.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, math.max(entity.width, 8),4)
end

return ruinsJumpThru