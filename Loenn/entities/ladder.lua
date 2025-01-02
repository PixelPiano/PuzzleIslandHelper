local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ladder = {}
ladder.name = "PuzzleIslandHelper/Ladder"
ladder.depth = 2000
ladder.minimumSize = {16, 8}
ladder.canResize = {false, true}
ladder.placements = {
    name = "Ladder",
    data = {
        height = 8,
        width = 16,
        collidableFlag = "",
        depth = 0,
        visible = true,
        texture = "objects/PuzzleIslandHelper/ladder"
    }
}

function ladder.sprite(room, entity)
    local sprites = {}
    local height = math.max(entity.height or 0, 8)

    local p = entity.texture;

    for i = 0, height - 8, 8 do
        local middleSprite = drawableSprite.fromTexture(p, entity)

        middleSprite:setJustification(0, 0)
        middleSprite:setOffset(0, 0)
        middleSprite:addPosition(0, i)
        table.insert(sprites, middleSprite)
    end

    return sprites
end

function ladder.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 16, math.max(entity.height, 8))
end

return ladder