local xnaColors = require("consts.xna_colors")

local white = xnaColors.White
local transitTower = {}

transitTower.name = "PuzzleIslandHelper/TransitTower"

transitTower.fillColor = {white[1] * 0.3, white[2] * 0.3, white[3] * 0.3, 0.6}
transitTower.borderColor = {white[1] * 0.8, white[2] * 0.8, white[3] * 0.8, 0.8}
transitTower.placements = {
    name = "Transit Tower",
    data = {
        width = 8,
        height = 8
    }
}
transitTower.depth = 1

return transitTower