local altCalidus = {}
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

altCalidus.justification = { 0, 0 }

local facings = {"Right", "Left"}
altCalidus.name = "PuzzleIslandHelper/AltCalidus"

altCalidus.depth = 1

altCalidus.placements =
{
    {
        name = "Calidus (Alt)",
        data = {
            facing = "Right"
        }
    }
}
altCalidus.fieldInformation =
{
    facing = 
    {
        options = facings,
        editable = false
    }
}
function altCalidus.sprite(room,entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/gameshow/host/machine00", entity)
    if entity.facing == "Left" then
        sprite.scaleX = -1
    end
    sprite:setJustification(0,0)
    return sprite
end
return altCalidus