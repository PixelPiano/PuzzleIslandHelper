local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local redMemoryOrb = {}
local presets = {"MainFollower","Reflection","Dummy"}
redMemoryOrb.depth = -10001
redMemoryOrb.name = "PuzzleIslandHelper/RedMemoryOrb"
redMemoryOrb.placements = {
    name = "Red Memory Orb",
    data = {
        preset = "Dummy",
        flag = ""
    }
}
redMemoryOrb.fieldInformation = 
{
    preset =
    {
        options = presets,
        editable = false
    }
}
function redMemoryOrb.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/memoryOrb", entity)
    sprite:addPosition(-sprite.meta.width / 2, -sprite.meta.height / 2)
    sprite:setColor(utils.getColor("FF0000FF"))
    return sprite
end

return redMemoryOrb