local drawableSprite = require("structs.drawable_sprite")

local entity3D = {}

entity3D.justification = { 0, 0 }

entity3D.name = "PuzzleIslandHelper/Entity3D"
function entity3D.depth(room, entity)
    return entity.depth or 0
end
entity3D.placements = {
    name = "Entity 3D",
    data = {
        fps = 12.0,
        spritePath = "objects/PuzzleIslandHelper/missingTexture",
        depth = 1,
        scaleX = 1,
        scaleY = 1,
        modelPath = "",
        roll = 0,
        yaw = 0,
        pitch = 0,
        rollRate = 0,
        yawRate = 0,
        pitchRate = 0,
        flag = ""

    }
}
function entity3D.texture(room, entity)
    if drawableSprite.fromTexture("decals/" .. entity.spritePath .. "00") ~= nil then
        return "decals/" .. entity.spritePath .. "00"
    elseif drawableSprite.fromTexture("decals/" .. entity.spritePath) ~= nil then
        return "decals/" .. entity.spritePath
    elseif drawableSprite.fromTexture(entity.spritePath.."00") ~=nil then
        return entity.spritePath.."00"
    else
        return entity.spritePath
    end
end

return entity3D
