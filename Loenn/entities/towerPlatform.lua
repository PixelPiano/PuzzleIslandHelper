local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local towerPlatform = {}
local path = "objects/PuzzleIslandHelper/segment"
towerPlatform.name = "PuzzleIslandHelper/TowerPlatform"
towerPlatform.depth = 0
towerPlatform.minimumSize = {8,8}
function towerPlatform.maximumSize(room, entity)
    return {entity.width + 8, 8}
end
towerPlatform.placements = {
    name = "Tower Platform",
    data = {
        width = 16,
        height = 8,
    }
}
local function getSprite(path,entity, x, y, quadX,quadY)
    local sprite = drawableSprite.fromTexture(path, entity)
    sprite:setJustification(0, 0)
    sprite:setOffset(0, 0)
    sprite:addPosition(x,y)
    sprite:useRelativeQuad(quadX, quadY, 8, 4)
    return sprite
end
-- Manual offsets and justifications of the sprites
function towerPlatform.sprite(room, entity)
    local sprites = {}
    local length = entity.width or 16
    table.insert(sprites, getSprite(path,entity,0,0,0,0))
    for i = 8, length - 16, 8 do
        table.insert(sprites, getSprite(path,entity,i,0,0,0))
    end
    table.insert(sprites, getSprite(path,entity,length-8,0,0,0))
    return sprites
end

function towerPlatform.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, math.max(entity.width, 16),8)
end
return towerPlatform