local drawableSprite = require("structs.drawable_sprite")
local frequencyCode = {}


frequencyCode.name = "PuzzleIslandHelper/FrequencyCode"
frequencyCode.justification = {0,0}

frequencyCode.placements =
{
    name = "Frequency Code",
    data =
    {
        rateA = 0,
        rateB = 0,
        rateC = 0,
        rateD = 0,
        flashTexturePath = "",
        spriteOnPath = "1-forsakencity/flag",
        spriteOffPath = "1-forsakencity/flag",
        codeID = "",
        flagOnComplete = "",
        useFlagInsteadOfID = false,
        depth = -9000
    }
}

function frequencyCode.depth(room, entity)
    return entity.depth or 0
end
function frequencyCode.sprite(room, entity)
    local path = "1-forsakencity/flag"
    if drawableSprite.fromTexture(entity.spriteOffPath .. "00",entity) ~= nil then
        path = entity.spriteOffPath .. "00"
    elseif drawableSprite.fromTexture(entity.spriteOffPath,entity) ~= nil then
        path = entity.spriteOffPath
    elseif drawableSprite.fromTexture("decals/" .. entity.spriteOffPath .. "00",entity) ~= nil then
        path = "decals/" .. entity.spriteOffPath .. "00"
    elseif drawableSprite.fromTexture("decals/" .. entity.spriteOffPath,entity) ~= nil then
        path = "decals/" .. entity.spriteOffPath
    end
    local sprite = drawableSprite.fromTexture(path, entity)
    sprite:setJustification(0, 0)
    return sprite
end

return frequencyCode