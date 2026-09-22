local drawableSprite = require("structs.drawable_sprite")
local frequencyDecal = {}
frequencyDecal.name = "PuzzleIslandHelper/FrequencyDecal"
frequencyDecal.justification = {0,0}

frequencyDecal.placements = 
{
    name = "Frequency Decal",
    data = {
        outline = false,
        visibilityFlag = "",
        frequencyFlag = "",
        depth = 2,
        decalPath = "1-forsakencity/flag",
        color = "FFFFFF",
        scaleX = 1,
        scaleY = 1,
        rotation = 0,
        rotationRate = 0,
        rotationRateInterval = -1,
        frequencyA = -1,
        frequencyB = -1,
        frequencyC = -1,
        frequencyD = -1,
        startRadius = 0,
        endRadius = 10
    }
}
frequencyDecal.fieldInformation =
{
    color = 
    {
        fieldType = "color",
        allowXNAColors = true
    }
}
function frequencyDecal.depth(room, entity)
    return entity.depth or 0
end
function frequencyDecal.sprite(room, entity)
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        path = "decals/" .. entity.decalPath .. "00"
    else
        path = "decals/" .. entity.decalPath
    end
    local sprite = drawableSprite.fromTexture(path, entity)
        sprite:setScale(entity.scaleX,entity.scaleY)
        sprite:setJustification(0.5, 0.5)
        sprite:addPosition(sprite.meta.width / 2, sprite.meta.height / 2)
        sprite.rotation = math.rad(entity.rotation)
        sprite:setColor(entity.color)
    return sprite
end

return frequencyDecal
