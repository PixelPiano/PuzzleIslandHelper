local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local graveLine = {}

graveLine.name = "PuzzleIslandHelper/GraveLine"
graveLine.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
graveLine.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
graveLine.placements = {
    name = "Grave Line",
    data = {
        width = 8,
        height = 8,
       
    }
}
graveLine.depth = 0

return graveLine