local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local stabilizer = {}
local path = "objects/PuzzleIslandHelper/stabilizer/"
stabilizer.name = "PuzzleIslandHelper/Stabilizer"
stabilizer.depth = 1
stabilizer.placements = {
    name = "Stabilizer",
    data = {
        startOn = false,
        locked = false,
        persistant = true
    }
}

function stabilizer.sprite(room, entity)
    local sprite
    if entity.startOn then
        sprite = drawableSprite.fromTexture(path .. "on",entity)
    else
        sprite = drawableSprite.fromTexture(path .. "off",entity)
    end
    sprite:setJustification(0,0)
    return sprite
end

return stabilizer