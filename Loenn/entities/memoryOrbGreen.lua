local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local greenMemoryOrb = {}
greenMemoryOrb.depth = -10001
greenMemoryOrb.name = "PuzzleIslandHelper/GreenMemoryOrb"
greenMemoryOrb.placements = {
    name = "Green Memory Orb",
    data = {
        idlePathID = "",
        maxPathSpeed = 0
    }
}
function greenMemoryOrb.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/memoryOrb", entity)
    sprite:addPosition(-sprite.meta.width / 2, -sprite.meta.height / 2)
    sprite:setColor(utils.getColor("00FF00FF"))
    return sprite
end

return greenMemoryOrb