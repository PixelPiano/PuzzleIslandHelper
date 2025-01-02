local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local towerBarrier = {}

towerBarrier.name = "PuzzleIslandHelper/TowerBarrier"
towerBarrier.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
towerBarrier.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
towerBarrier.placements = {
    name = "Tower Barrier",
    data = {
        width = 8,
        height = 8
    }
}
towerBarrier.depth = 0

return towerBarrier