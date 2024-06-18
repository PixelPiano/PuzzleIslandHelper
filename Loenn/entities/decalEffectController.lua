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
        color = "000000",
        color2 = "00FF00",
        flashColor = "ffffff",
        flashOnCollide = true,
        gfxEffect = "None",
        usesAudio = true,
        colorBlendAmount = 1,
        colorFadeInOut = false,
        colorFadeDuration = 1,
        outlineSprites = false,
        forceBurst = true
    }
}
decalEffectController.fieldOrder = {
   "color",
   "colorBlendAmount",
   "color2",
   "colorFadeDuration",
   "flashColor",
   "flashLimit",
   "targetGroupID",
   "event",
   "gfxEffect",
   "glitch",
   "flashOnCollide",
   "usesAudio",
   "colorFadeInOut",
   "outlineSprites",
   "forceBurst"
}
decalEffectController.ignoredFields = { 
    "width",
    "height",
    "_id",
    "_name",
    "x",
    "y"
}
decalEffectController.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
      color2 =
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
