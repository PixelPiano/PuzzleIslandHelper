local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local memoryPhone = {}
memoryPhone.name = "PuzzleIslandHelper/MemoryPhone"
memoryPhone.depth = 2000
memoryPhone.placements = {
    name = "Memory Phone (Forest)",
    data = {
        path = "objects/PuzzleIslandHelper/phones/forest/lonn"
    }
}
memoryPhone.ignoredFields = { 
    "_id",
    "_name",
    "path"
}
-- Manual offsets and justifications of the sprites
function memoryPhone.sprite(room, entity)
    return drawableSprite.fromTexture(entity.path, entity)
end

return memoryPhone