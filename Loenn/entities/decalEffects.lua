local drawableSprite = require("structs.drawable_sprite")

local decalEffects = {}

decalEffects.justification = { 0, 0 }

decalEffects.name = "PuzzleIslandHelper/DecalEffects"
local gfxEffects = {"None", "Blur", "Distort", "Glitch"}
function decalEffects.depth(room, entity)
    return entity.depth or 0
end
decalEffects.placements = {
    name = "Decal Effects",
    data = {
        fps = 12.0,
        flashLimit = 0.5,
        decalPath = "1-forsakencity/flag",
        event = "event:/new_content/game/10_farewell/glitch_short",
        glitch = true,
        color = "00FF00",
        flashColor = "ffffff",
        flashOnCollide = true,
        cameraFade = true,
        depth = 1,
        gfxEffect = "None",
        usesAudio = true,
    }
}
function decalEffects.texture(room, entity)
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        return "decals/" .. entity.decalPath .. "00"
    else
        return "decals/" .. entity.decalPath
    end
end

decalEffects.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
      flashColor =
    {
         fieldType = "color",
         allowXNAColors = true,
    },
      gfxEffect = 
    {
         editable = false,
         options = gfxEffects
    },

}

return decalEffects
