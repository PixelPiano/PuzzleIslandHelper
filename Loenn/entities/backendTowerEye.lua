local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local towerEye = {}

towerEye.name = "PuzzleIslandHelper/BackendTowerEye"

towerEye.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
towerEye.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
towerEye.placements = {
    name = "Backend Tower Eye",
    data = {
        width = 8,
        height = 8,
        flag = "",
        textData = ""
    }
}
towerEye.depth = 0

return towerEye