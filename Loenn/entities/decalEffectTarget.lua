local drawableSprite = require("structs.drawable_sprite")

local decalEffectTarget = {}

--decalEffectTarget.justification = { 0, 0 }

decalEffectTarget.name = "PuzzleIslandHelper/DecalEffectTarget"
function decalEffectTarget.depth(room, entity)
    return entity.depth or 0
end
decalEffectTarget.placements = {
    name = "Decal Effect Target",
    data = {
        fps = 12.0,
        decalPath = "1-forsakencity/flag",
        depth = 1,
        groupId = "decalEffectGroup_1",
        scaleX = 1,
        scaleY = 1,
        rotation = 0
    }
}
function decalEffectTarget.sprite(room, entity)
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        path = "decals/" .. entity.decalPath .. "00"
    else
        path = "decals/" .. entity.decalPath
    end
    local sprite = drawableSprite.fromTexture(path, entity)
        sprite:setScale(entity.scaleX or 1, entity.scaleY or 1)
        sprite:setJustification(0.5, 0.5)
        sprite.rotation = math.rad(entity.rotation or 0)
    return sprite
end

return decalEffectTarget
