local xnaColors = require("consts.xna_colors")

local magenta = xnaColors.Magenta
local towerElevator = {}

towerElevator.name = "PuzzleIslandHelper/TowerElevator"
towerElevator.nodeLimits = {1,1}
towerElevator.nodeVisibility = "always"
towerElevator.nodeLineRenderType = "line"
towerElevator.fillColor = {magenta[1] * 0.3, magenta[2] * 0.3, magenta[3] * 0.3, 0.6}
towerElevator.borderColor = {magenta[1] * 0.8, magenta[2] * 0.8, magenta[3] * 0.8, 0.8}
towerElevator.placements = {
    name = "Tower Elevator",
    data = {
        width = 8,
        height = 8,
        flag = "ColumnPuzzleSolved"
    }
}

return towerElevator