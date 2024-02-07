local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local customWater = {}

customWater.name = "PuzzleIslandHelper/CustomWater"
customWater.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
customWater.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}

customWater.placements = {
    name = "Custom Water",
    data = {
        width = 8,
        height = 8,
        flag = "",
        inverted = false,   
    }
}
customWater.depth = 0

return customWater