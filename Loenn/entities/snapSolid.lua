local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local snapSolid = {}

snapSolid.name = "PuzzleIslandHelper/SnapSolid"
snapSolid.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
snapSolid.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
snapSolid.placements = {
    name = "Snap Solid",
    data = {
        width = 8,
        height = 8,
    }
}

snapSolid.depth = -1

return snapSolid