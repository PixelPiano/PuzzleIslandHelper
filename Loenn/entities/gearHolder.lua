local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local gearMachine = {}
gearMachine.justification = {0,0}
gearMachine.name = "PuzzleIslandHelper/GearMachine"
gearMachine.depth = 0
function gearMachine.sprite(room, entity)
    local sprites = {}
    local path = "objects/PuzzleIslandHelper/gear/"

    local insides = drawableSprite.fromTexture(path .. "lonnInside", entity)
    table.insert(sprites,insides)

    local center = drawableSprite.fromTexture(path .. "center", entity)
    table.insert(sprites, center)
    return sprites
end
gearMachine.placements = {
    name = "Gear Holder",
    data = {
        onlyOnce = false,
        holderId = -1,
        flagOnFinish=""
    }
}

return gearMachine