local xnaColors = require("consts.xna_colors")
local red = xnaColors.Red
local glitchDamage = {}

glitchDamage.name = "PuzzleIslandHelper/GlitchDamage"

glitchDamage.fillColor = {red[1] * 0.3, red[2] * 0.3, red[3] * 0.3, 0.6}
glitchDamage.borderColor = {red[1] * 0.8, red[2] * 0.8, red[3] * 0.8, 0.8}
glitchDamage.placements = {
    name = "Glitch Damage",
    data = {
        width = 8,
        height = 8,
        minOffset = 0,
        maxOffset = 0,
        minInterval = 0.1,
        maxInterval = 0.5,
        minSize = 8,
        maxSize = 16,
        offsetChance = 0.5,
        offsetMultX = 1,
        offsetMultY = 1,
        colors = "FF0000FF, 00FF00FF, 0000FFFF"
        flag = ""
    }
}
glitchDamage.fieldInformation = 
{
    colors = {
        fieldType = "list",
        elementSeparator = ",",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true
        },
    }
}
glitchDamage.depth = -5

return glitchDamage