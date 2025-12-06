local tower = {}
local xnaColors = require("consts.xna_colors")
local gray = xnaColors.Gray

tower.justification = { 0, 0 }

tower.name = "PuzzleIslandHelper/Tower"

tower.fillColor = {gray[1] * 0.3, gray[2] * 0.3, gray[3] * 0.3, 0.6}
tower.borderColor = {gray[1] * 0.8, gray[2] * 0.8, gray[3] * 0.8, 0.8}
tower.depth = 1
tower.minimumSize = {32, 32}
tower.placements =
{
    {
        name = "Tower",
        data = {
            width = 8,
            height = 8
        }
    }
}
return tower