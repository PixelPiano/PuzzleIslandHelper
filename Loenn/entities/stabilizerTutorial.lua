local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local stabilizerTutorial = {}

stabilizerTutorial.name = "PuzzleIslandHelper/StabilizerTutorial"

stabilizerTutorial.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
stabilizerTutorial.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
stabilizerTutorial.placements = {
    name = "Stabilizer Tutorial",
    data = {
        width = 8,
        height = 8,
        combos = "0101,1010"
    }
}
stabilizerTutorial.depth = 0

return stabilizerTutorial