local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local tutorial = {}

tutorial.name = "PuzzleIslandHelper/FrequencyBTutorial"

tutorial.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
tutorial.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
tutorial.placements = {
    name = "Frequency B Tutorial",
    data = {
        width = 8,
        height = 8,
        ids = "",
        rateA = 0,
        rateB = 0,
        rateC = 0,
        rateD = 0,
        angleDegrees = 0,
    }
}

return tutorial