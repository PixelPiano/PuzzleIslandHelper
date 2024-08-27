local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local digitalTumor = {}

digitalTumor.name = "PuzzleIslandHelper/DigitalTumor"
digitalTumor.depth = -10010
digitalTumor.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
digitalTumor.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
digitalTumor.placements = {
    name = "Digital Tumor",
    data = {
        width = 8,
        height = 8,
        flag = "",
        foreground = false,
        color = "00FF00"
    }
}
digitalTumor.fieldInformation = 
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true
    }
}

return digitalTumor