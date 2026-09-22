local drawableSprite = require("structs.drawable_sprite")
local talkingDecal = {}


talkingDecal.name = "PuzzleIslandHelper/BinaryImage"
talkingDecal.justification = {0,0}
talkingDecal.placements = 
{
    name = "Binary Image",
    data = 
    {
        flag = "",
        depth = 2,
        texturePath = "",
        onColor = "00FF00",
        offColor = "FF0000"
    }
}

talkingDecal.fieldInformation =
{
    onColor = 
    {
        fieldType = "color",
        allowXNAColors = true
    },
    offColor = 
    {
        fieldType = "color",
        allowXNAColors = true
    }
}
function talkingDecal.depth(room, entity)
    return entity.depth or 0
end
function talkingDecal.sprite(room, entity)
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.texturePath .. "00") ~= nil then
        path = "decals/" .. entity.texturePath .. "00"
    elseif drawableSprite.fromTexture("decals/" .. entity.texturePath) ~= nil
        path = "decals/" .. entity.texturePath
    else
        path = entity.texturePath
    end
    local sprite = drawableSprite.fromTexture(path, entity)
        sprite:setScale(1,1)
        sprite:setJustification(0, 0)
        sprite.rotation = math.rad(0)
    return sprite
end

return talkingDecal
