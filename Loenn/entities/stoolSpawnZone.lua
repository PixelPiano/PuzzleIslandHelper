local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local stoolRespawn = {}
local path = "objects/PuzzleIslandHelper/stool/spawnTex"
stoolRespawn.name = "PuzzleIslandHelper/StoolRespawn"
stoolRespawn.depth = -100000
stoolRespawn.minimumSize = {24, 8}
stoolRespawn.canResize = {true, false}
stoolRespawn.placements = {
    name = "Stool Respawn",
    data = {
        width = 24,
        height = 8
    }
}
local function getSprite(path,entity, x, y, quadX)
    local sprite = drawableSprite.fromTexture(path, entity)
    sprite:setJustification(0, 0)
    sprite:setOffset(0, 0)
    sprite:addPosition(x,y)
    sprite:useRelativeQuad(quadX, 0, 8, 8)
    return sprite
end
-- Manual offsets and justifications of the sprites
function stoolRespawn.sprite(room, entity)
    local sprites = {}
    local p = path;
    local length = math.max(entity.width or 24, 24)

    table.insert(sprites, getSprite(p,entity,0,0,0))
    for i = 8, length - 16, 8 do
        table.insert(sprites, getSprite(p,entity,i,0,8))
    end
    table.insert(sprites, getSprite(p,entity,length - 8,0,16))
    return sprites
end

return stoolRespawn