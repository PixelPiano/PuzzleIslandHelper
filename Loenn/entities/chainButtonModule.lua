local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local chainButtonModule = {}

chainButtonModule.name = "PuzzleIslandHelper/ChainButtonModule"
chainButtonModule.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
chainButtonModule.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
chainButtonModule.placements = {
    name = "Chain Button Module",
    data = {
        width = 8,
        height = 8,
        buttonFlagPrefix = ""
    }
}
chainButtonModule.depth = 1

return chainButtonModule