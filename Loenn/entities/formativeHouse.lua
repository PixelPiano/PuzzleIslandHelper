local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local water = {}
water.minimumSize = {16,16}
water.name = "PuzzleIslandHelper/FormativeHouse"
water.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
water.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
water.placements = {
    name = "Formative House",
    data = {
        width = 16,
        height = 16
    }
}

water.depth = 6

return water