local drawableSprite = require("structs.drawable_sprite")

local decalEffectTarget = {}

decalEffectTarget.justification = { 0, 0 }

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
    }
}
function decalEffectTarget.texture(room, entity)
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        return "decals/" .. entity.decalPath .. "00"
    else
        return "decals/" .. entity.decalPath
    end
end


return decalEffectTarget
