local towerColumn = {}
local xnaColors = require("consts.xna_colors")
local gray = xnaColors.Gray

towerColumn.justification = { 0, 0 }

towerColumn.name = "PuzzleIslandHelper/TowerColumn"

towerColumn.fillColor = {gray[1] * 0.3, gray[2] * 0.3, gray[3] * 0.3, 0.6}
towerColumn.borderColor = {gray[1] * 0.8, gray[2] * 0.8, gray[3] * 0.8, 0.8}
towerColumn.depth = 1
towerColumn.minimumSize = {32, 32}
towerColumn.placements =
{
    {
        name = "Tower Column",
        data = {
            width = 32,
            height = 32,
            stairsRadius = 32,
            floors = 3,
            revolutionsPerFloor = 3,
            flags = ""
        }
    }
}
return towerColumn