local drawableSprite = require("structs.drawable_sprite")
local blowAwayDecal = {}
blowAwayDecal.name = "PuzzleIslandHelper/BlowAwayDecal"
function blowAwayDecal.depth(room, entity)
    return entity.depth or 0
end
blowAwayDecal.placements = {
    name = "Blow Away Decal",
    data = {
        path = "1-forsakencity/flag",
        depth = 1,
        scaleX = 1,
        scaleY = 1,
        rotation = 0,
        sliceSize = 1,
        sliceSinIncrement = 0.1,
        easeDown = false,
        waveSpeed = 4,
        waveAmplitude = 1,
        persistent = false
    }
}
function blowAwayDecal.sprite(room, entity)
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.path .. "00") ~= nil then
        path = "decals/" .. entity.path .. "00"
    else
        path = "decals/" .. entity.path
    end
    local sprite = drawableSprite.fromTexture(path, entity)
        sprite:setScale(entity.scaleX or 1, entity.scaleY or 1)
        sprite:setJustification(0.5, 0.5)
        sprite.rotation = math.rad(entity.rotation or 0)
    return sprite
end

return blowAwayDecal
