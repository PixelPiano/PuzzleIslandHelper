local xnaColors = require("consts.xna_colors")
local red = xnaColors.Red
local corruption = {}

corruption.name = "PuzzleIslandHelper/Corruption"

corruption.fillColor = {red[1] * 0.3, red[2] * 0.3, red[3] * 0.3, 0.6}
corruption.borderColor = {red[1] * 0.8, red[2] * 0.8, red[3] * 0.8, 0.8}
corruption.placements = {
    name = "Corruption",
    data = {
        width = 8,
        height = 8,
        flag = "",
        maxChunkSize = 1,
        visualUpdateFrameDelay = 1
    }
}
corruption.depth = 1

return corruption