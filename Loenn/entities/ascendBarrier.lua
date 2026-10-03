local xnaColors = require("consts.xna_colors")
local easeHelper = require("mods").requireFromPlugin("libraries.easeHelper")
local lightBlue = xnaColors.LightBlue
local ascendBarrier = {}

ascendBarrier.name = "PuzzleIslandHelper/AscendBarrier"

ascendBarrier.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
ascendBarrier.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
ascendBarrier.placements = {
    name = "Ascend Barrier",
    data = {
        width = 8,
        height = 8,
        pitchFrom = 0,
        pitchTo = 0,
        rotationTime = 0,
        rotationEase = "Linear",
        bulgeIn = "Linear",
        bulgeOut = "Linear",
        bulgeTime = 0,
        bulgeIncrement = 0
    }
}
ascendBarrier.fieldInformation = {
    rotationEase = {
        options = easeHelper.getEaseOptions(),
        editable = false
    },
    bulgeIn = {
        options = easeHelper.getEaseOptions(),
        editable = false
    },
    bulgeOut = {
        options = easeHelper.getEaseOptions(),
        editable = false
    }
}
ascendBarrier.depth = 0

return ascendBarrier