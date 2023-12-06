local xnaColors = require("consts.xna_colors")
local green = xnaColors.Green

local lcdArea = {}

lcdArea.name = "PuzzleIslandHelper/LCDArea"
lcdArea.fillColor = {green[1] * 0.3, green[2] * 0.3, green[3] * 0.3, 0.6}
lcdArea.borderColor = {green[1] * 0.8, green[2] * 0.8, green[3] * 0.8, 0.8}
lcdArea.placements = {
    name = "LCD Area",
    data = {
        width = 8,
        height = 8,
    }
}
lcdArea.depth = 0

return lcdArea