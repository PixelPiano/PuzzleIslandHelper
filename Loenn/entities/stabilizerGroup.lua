local drawableSprite = require "structs.drawable_sprite"
local utils = require("utils")
local stabilizerGroup = {}

stabilizerGroup.name = "PuzzleIslandHelper/StabilizerGroup"
stabilizerGroup.placements =
{
    {
        name = "Stabilizer Group",
        data = {
            groupID = "",
            combo = "0101",
            combos = "",
            persistent = false,
            isMachine = true
        }
    },
}
function stabilizerGroup.sprite(room, entity)

    local sprites = {}
    local x = 0
    for c in entity.combo:gmatch"." do
        local sprite
        local path
        if c == '1' then
            if entity.isMachine then
                path = "objects/PuzzleIslandHelper/stabilizer/cubbyOn"
            else
                path = "objects/PuzzleIslandHelper/stabilizer/on"
            end
            sprite = drawableSprite.fromTexture(path, entity)
        elseif c == '0' then
            if entity.isMachine then
                path = "objects/PuzzleIslandHelper/stabilizer/cubbyOff"
            else
                path = "objects/PuzzleIslandHelper/stabilizer/off"
            end
            sprite = drawableSprite.fromTexture(path, entity)
        else
            return sprites
        end
        sprite:setJustification(0, 0)
        sprite:addPosition(x, 0)
        x = x + 16
        table.insert(sprites, sprite)
    end
    return sprites
end
function stabilizerGroup.selection(room, entity)
    local width = 0
    for c in entity.combo:gmatch"." do
        if c == '1' or c == '0' then
            width = width + 16
        end
    end
    if width < 16 then
        return utils.rectangle(entity.x, entity.y, 8, 18)
    else
        return utils.rectangle(entity.x, entity.y, width, 18)
    end
end


return stabilizerGroup