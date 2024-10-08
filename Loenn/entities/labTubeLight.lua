local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local labTubeLight = {}
local path = "objects/PuzzleIslandHelper/machines/gizmos/tubeLight"
labTubeLight.name = "PuzzleIslandHelper/LabTubeLight"
labTubeLight.depth = 2000
labTubeLight.minimumSize = {16, 8}
labTubeLight.canResize = {true, false}
labTubeLight.placements = {
    name = "Lab Tube Light",
    data = {
        width = 16,
        digital = false,
        broken = false,
        flipY = false
    }
}

-- Manual offsets and justifications of the sprites
function labTubeLight.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 8)

    local p = path;
    if entity.digital then
        p = path .. "Digi"
    end
    if entity.broken then
        p = path .. "BrokenLonn"
    end
    local leftSprite = drawableSprite.fromTexture(p, entity)

    leftSprite:setJustification(0, 0)
    leftSprite:setOffset(0, 0)
    leftSprite:useRelativeQuad(0, 0, 8, 8)

    table.insert(sprites, leftSprite)

    for i = 8, width - 16, 8 do
        local middleSprite = drawableSprite.fromTexture(p, entity)

        middleSprite:setJustification(0, 0)
        middleSprite:setOffset(0, 0)
        middleSprite:addPosition(i, 0)
        middleSprite:useRelativeQuad(8, 0, 8, 8)

        table.insert(sprites, middleSprite)
    end

    local rightSprite = drawableSprite.fromTexture(p, entity)

    rightSprite:setJustification(0, 0)
    rightSprite:setOffset(0, 0)
    rightSprite:addPosition(width-8, 0)
    rightSprite:useRelativeQuad(16, 0, 8, 8)

    table.insert(sprites, rightSprite)

    return sprites
end

function labTubeLight.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, math.max(entity.width, 16),8)
end

return labTubeLight