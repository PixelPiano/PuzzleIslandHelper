local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local popBlock = {}

popBlock.name = "PuzzleIslandHelper/PopBlock"
popBlock.minimumSize = {24, 24}
popBlock.depth = -12999


popBlock.placements = {
    name = "Pop Block",
    data = {
        width = 24,
        height = 24
    }
}

local function addSprite(sprites, texture, x, y, offsetX, offsetY, entity)
    local sprite = drawableSprite.fromTexture(texture,entity)
    sprite:useRelativeQuad(x,y,8,8)
    sprite:addPosition(offsetX,offsetY)
    table.insert(sprites, sprite)
end

local function getSprites(entity)
    local sprites = {}
    local texture = "objects/PuzzleIslandHelper/popBlock/sprite"
    local width = entity.width or 24
    local height = entity.height or 24

    addSprite(sprites, texture, 0, 0, 0, 0,entity)
    addSprite(sprites, texture, 16, 0, width - 8, 0,entity)
    addSprite(sprites, texture, 0, 16, 0, height - 8,entity)
    addSprite(sprites, texture, 16, 16, width - 8, height - 8,entity)

    for x = 8, width - 16, 8 do
        addSprite(sprites, texture, 8, 0, x, 0,entity)
        addSprite(sprites, texture, 8, 16, x, height - 8,entity)
    end
    for y = 8, height - 16, 8 do
        addSprite(sprites, texture, 0, 8, 0, y,entity)
        addSprite(sprites, texture, 16, 8, width - 8, y,entity)
    end

    for x2 = 8, width - 16, 8 do
        for y2 = 8, height - 16, 8 do
            addSprite(sprites, texture, 8, 8, x2, y2,entity)
        end
    end

    return sprites
end

function popBlock.sprite(room, entity)
    return getSprites(entity)
end

return popBlock