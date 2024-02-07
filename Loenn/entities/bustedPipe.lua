local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local bustedPipe = {}
local path = "objects/PuzzleIslandHelper/bustedPipe/"
bustedPipe.name = "PuzzleIslandHelper/BustedPipe"
bustedPipe.depth = -1
bustedPipe.minimumSize = {8,8}
bustedPipe.placements = {
    name = "Busted Pipe",
    data = {
        width = 16,
        height = 8,
        vertical = true,
        useForCutscene = true,
        flag = ""
    }
}

-- Manual offsets and justifications of the sprites
function bustedPipe.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 8)
    local height = math.max(entity.height or 0, 8)
    if entity.vertical then
      for i = 0, height-8, 8 do
        local body = drawableSprite.fromTexture(path .. "vertical", entity)
        body:setJustification(0, 0)
        body:setOffset(0, 0)
        body:addPosition(0, i)
        table.insert(sprites, body)
       end

    else
      for i = 0, width-8, 8 do
        local body = drawableSprite.fromTexture(path .. "horizontal", entity)
        body:setJustification(0, 0)
        body:setOffset(0, 0)
        body:addPosition(i, 0)
        table.insert(sprites, body)
       end
    end
    return sprites
end

function bustedPipe.selection(room, entity)
    if entity.vertical then
        return utils.rectangle(entity.x, entity.y,  16, math.max(entity.height, 8))
    else
        return utils.rectangle(entity.x, entity.y,  math.max(entity.width, 8), 16)
    end
end

return bustedPipe