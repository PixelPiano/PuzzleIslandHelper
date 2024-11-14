local xnaColors = require("consts.xna_colors")
local brown = xnaColors.Brown

local festivalFloat = {}

festivalFloat.name = "PuzzleIslandHelper/FestivalFloat"
festivalFloat.fillColor = {brown[1] * 0.3, brown[2] * 0.3, brown[3] * 0.3, 0.6}
festivalFloat.borderColor = {brown[1] * 0.8, brown[2] * 0.8, brown[3] * 0.8, 0.8}
festivalFloat.minimumSize = {8, 8}
festivalFloat.canResize = {true, false}
festivalFloat.placements = {
    name = "Festival Float",
    data = {
        width = 8,
        height = 8,
    }
}
festivalFloat.depth = 2

return festivalFloat