local decalEffectController = {}

decalEffectController.justification = { 0, 0 }

decalEffectController.name = "PuzzleIslandHelper/DecalEffectController"
decalEffectController.texture = "objects/PuzzleIslandHelper/decalEffectController/decalEffectControllerIcon"
local gfxEffects = {"None", "Blur", "Distort", "Glitch"}
decalEffectController.placements = {
    name = "Decal Effect Controller",
    data = {
        flashLimit = 0.5,
        targetGroupID = "decalEffectGroup_1",
        event = "event:/new_content/game/10_farewell/glitch_short",
        glitch = true,
        color = "00FF00",
        flashColor = "ffffff",
        flashOnCollide = true,
        gfxEffect = "None",
        usesAudio = true,
    }
}

decalEffectController.fieldInformation = {
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

return decalEffectController
