local drawableSprite = require("structs.drawable_sprite")

local templeMonolith = {}

templeMonolith.name = "PuzzleIslandHelper/TempleMonolith"
local types = {"0","1","2","3"}
templeMonolith.depth = 0
templeMonolith.placements = {
    name = "Temple Monolith",
    data = {
        type = "0",
        falls = false,
        reflection = false
    }
}

function templeMonolith.sprite(room, entity, viewport)
    local sprite =  drawableSprite.fromTexture("objects/PuzzleIslandHelper/templeMonolith/bigMonolith0" .. entity.type, entity)
    sprite.justificationX = 0
    sprite.justificationY = 0
    return sprite
end
templeMonolith.fieldInformation = 
{
    type =
    {
        options = types,
        editable = false
    }
}
return templeMonolith
