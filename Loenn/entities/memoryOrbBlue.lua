local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local blueMemoryOrb = {}
blueMemoryOrb.depth = -10001
blueMemoryOrb.name = "PuzzleIslandHelper/BlueMemoryOrb"
blueMemoryOrb.placements = {
    name = "Blue Memory Orb",
    data = {
        idlePathID = "",
        maxPathSpeed = 0
    }
}
function blueMemoryOrb.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/memoryOrb", entity)
    sprite:addPosition(-sprite.meta.width / 2, -sprite.meta.height / 2)
    sprite:setColor(utils.getColor("0000FFFF"))
    return sprite
end

return blueMemoryOrb