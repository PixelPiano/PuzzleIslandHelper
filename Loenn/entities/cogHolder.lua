local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local cogMachine = {}
cogMachine.justification = {0,0}
cogMachine.name = "PuzzleIslandHelper/CogMachine"
cogMachine.depth = 0
function cogMachine.sprite(room, entity)
    local sprites = {}
    local path = "objects/PuzzleIslandHelper/cog/"

    local insides = drawableSprite.fromTexture(path .. "lonnInside", entity)
    table.insert(sprites,insides)

    local center = drawableSprite.fromTexture(path .. "center", entity)
    table.insert(sprites, center)
    return sprites
end
cogMachine.placements = {
    name = "Cog Holder",
    data = {
        onlyOnce = false,
        holderId = -1,
        flagOnFinish=""
    }
}

return cogMachine