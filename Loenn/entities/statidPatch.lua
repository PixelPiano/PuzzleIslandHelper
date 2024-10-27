local xnaColors = require("consts.xna_colors")
local lime = xnaColors.Lime

local statidPatch = {}

statidPatch.name = "PuzzleIslandHelper/StatidPatch"
statidPatch.fillColor = {lime[1] * 0.3, lime[2] * 0.3, lime[3] * 0.3, 0.6}
statidPatch.borderColor = {lime[1] * 0.8, lime[2] * 0.8, lime[3] * 0.8, 0.8}
statidPatch.placements = {
    name = "Statid Patch",
    data = {
        width = 8,
        height = 8,
        digital = false,
        spacing = 8,
        halfStepChance = 0.1,
        petals = 4,
        petalRange = 0
    }
}
statidPatch.depth = 0

return statidPatch