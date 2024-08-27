local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local digitalFolliage = {}

digitalFolliage.name = "PuzzleIslandHelper/DigitalFolliage"
digitalFolliage.depth = -10010
digitalFolliage.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
digitalFolliage.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
digitalFolliage.placements = {
    name = "Digital Folliage",
    data = {
        width = 8,
        height = 8,
        depth = -1,
        flag = "",
        foreground = false,
        color = "00FF00"
    }
}
digitalFolliage.fieldInformation = 
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true
    }
}

return digitalFolliage