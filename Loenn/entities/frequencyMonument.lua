local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local frequencyMonument= {}
frequencyMonument.justification = { 0, 0 }

frequencyMonument.name = "PuzzleIslandHelper/FrequencyMonument"

frequencyMonument.depth = 1

frequencyMonument.placements =
{
    {
        name = "Frequency Monument",
        data = 
        {
            flagOnComplete = "",
            rateA = 0,
            rateB = 0,
            rateC = 0,
            rateD = 0,
            centerTexture = "objects/PuzzleIslandHelper/monument/defaultCenter",
            componentID = ""
        }
    }
}
function frequencyMonument.sprite(room, entity)
    local centerPath = entity.centerTexture
    if drawableSprite.fromTexture(centerPath) == nil then
        centerPath = "objects/PuzzleIslandHelper/monument/defaultCenter"
    end
    local sprites = {}
    local center = drawableSprite.fromTexture(centerPath, entity)
    local base = drawableSprite.fromTexture("objects/PuzzleIslandHelper/monument/base",entity)
    base:setJustification(0,0)
    local r =  base:getRectangle()
    local spriteWidth = r.width
    local spriteHeight = r.height
    center:setJustification(0.5, 0.5)
    center:addPosition(spriteWidth / 2, spriteHeight / 2)
    table.insert(sprites,base)
    table.insert(sprites,center)
    return sprites
end
return frequencyMonument