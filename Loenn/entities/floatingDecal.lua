local drawableSprite = require("structs.drawable_sprite")

local floatingDecal = {}

--floatingDecal.justification = { 0, 0 }
floatingDecal.nodeLimits = {1, 1}
floatingDecal.nodeLineRenderType = "line"
floatingDecal.name = "PuzzleIslandHelper/FloatingDecal"
function floatingDecal.depth(room, entity)
    return entity.depth or 0
end

floatingDecal.placements = {
    name = "Floating Decal",
    data = {
        fps = 12.0,
        decalPath = "1-forsakencity/flag",
        flag = "floating_decal",
        invertFlag = false,
        interval = 1,
        depth = 1,
        scaleX = 1,
        scaleY = 1,
        rotation = 0,
        color = "FFFFFF"
    }
}
floatingDecal.fieldInformation =
{
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}
-- Manual offsets and justifications of the sprites
function floatingDecal.sprite(room, entity)

    local path;
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        path = "decals/" .. entity.decalPath .. "00"
    else
        path = "decals/" .. entity.decalPath
    end
    local sprite = drawableSprite.fromTexture(path, entity)

        sprite.rotation = math.rad(entity.rotation or 0)
        sprite:setScale(entity.scaleX or 1, entity.scaleY or 1)
        sprite:setJustification(0.5, 0.5)
    return sprite
end
return floatingDecal
