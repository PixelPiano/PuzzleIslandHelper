local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local rail = {}
local path = "objects/PuzzleIslandHelper/minecart/"
rail.name = "PuzzleIslandHelper/Rail"
rail.depth = -1
rail.minimumSize = {16, 4}
rail.canResize = {true, false}
rail.placements = {
    name = "Rail",
    data = {
        width = 16,
        hasBarrier = true
    }
}

-- Manual offsets and justifications of the sprites
function rail.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 16)
    local walls = entity.hasBarrier
    local leftSprite = drawableSprite.fromTexture(path .. "rail", entity)

    leftSprite:setJustification(0, 0)
    leftSprite:setOffset(0, 0)
    leftSprite:useRelativeQuad(0, 0, 8, 8)
    table.insert(sprites, leftSprite)

    if walls then
        local leftWall = drawableSprite.fromTexture(path .. "barrier", entity)
        leftWall:setJustification(0,0)
        leftWall:setOffset(0,0)
        leftWall:addPosition(0,-4)
        table.insert(sprites,leftWall)
    end
    for i = 8, width - 16, 8 do
        local middleSprite = drawableSprite.fromTexture(path .. "rail", entity)

        middleSprite:setJustification(0, 0)
        middleSprite:setOffset(0, 0)
        middleSprite:addPosition(i, 0)
        middleSprite:useRelativeQuad(8, 0, 8, 8)

        table.insert(sprites, middleSprite)
    end

    local rightSprite = drawableSprite.fromTexture(path .. "rail", entity)

    rightSprite:setJustification(0, 0)
    rightSprite:setOffset(0, 0)
    rightSprite:addPosition(width - 8, 0)
    rightSprite:useRelativeQuad(16, 0, 8, 8)

    table.insert(sprites, rightSprite)
    if walls then
        local rightWall = drawableSprite.fromTexture(path .. "barrier", entity)
        rightWall:setJustification(0,0)
        rightWall:setOffset(0,0)
        rightWall:addPosition(width,-4)
        rightWall:setScale(-1,1)
        table.insert(sprites,rightWall)
    end
    return sprites
end

function rail.selection(room, entity)
    return utils.rectangle(entity.x, entity.y + 4,  math.max(entity.width, 16), 4)
end

return rail