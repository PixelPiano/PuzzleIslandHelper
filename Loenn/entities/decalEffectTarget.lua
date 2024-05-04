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
        scaleY = 1
    }
}
function decalEffectTarget.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(getTex(entity), entity)
        sprite:setScale(entity.scaleX or 1, entity.scaleY or 1)
        sprite:setJustification(0.5, 0.5)
    return sprite
end
function getTex(entity)
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        return "decals/" .. entity.decalPath .. "00"
    else
        return "decals/" .. entity.decalPath
    end
end

return decalEffectTarget
