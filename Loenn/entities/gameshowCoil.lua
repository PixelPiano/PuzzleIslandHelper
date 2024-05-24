local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local gameshowCoil = {}
local path = "objects/PuzzleIslandHelper/gameshow/coil"
gameshowCoil.name = "PuzzleIslandHelper/GameshowCoil"
gameshowCoil.depth = 2000
gameshowCoil.minimumSize = {8, 8}
gameshowCoil.canResize = {true, false}
gameshowCoil.placements = {
    name = "Gameshow Coil",
    data = {
        width = 8,
        flashes = 3,
        interval = 0.1,
        flag = "gameshowDigiFlash"
    }
}

-- Manual offsets and justifications of the sprites
function gameshowCoil.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 8)

    for i = 8, width, 8 do
        local sprite = drawableSprite.fromTexture(path, entity)

        sprite:setJustification(0, 0)
        sprite:setOffset(0, 0)
        sprite:addPosition(i-8, 0)
        table.insert(sprites, sprite)
    end

    return sprites
end

function gameshowCoil.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, math.max(entity.width, 8),8)
end

return gameshowCoil