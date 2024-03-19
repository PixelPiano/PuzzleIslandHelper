local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local fishLine = {}

fishLine.name = "PuzzleIslandHelper/FishLine"
fishLine.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
fishLine.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
fishLine.placements = {
    name = "Fish Line",
    data = {
        width = 8,
        height = 8,
    }
}
fishLine.depth = 0

return fishLine