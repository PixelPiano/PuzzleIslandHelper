local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local windEmitter = {}
local directions = {"Up", "Down", "Left", "Right"}
windEmitter.name = "PuzzleIslandHelper/WindEmitter"
windEmitter.depth = -5
windEmitter.minimumSize = {8,8}

windEmitter.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
windEmitter.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}

windEmitter.fieldInformation = {
    direction = {
        editable = false,
        options = directions
    },
}
windEmitter.placements = {
    {
        name = "Wind Emitter",
        data = {
            width = 8,
            height = 8,
            direction = "Right",
            interval = 0.1,
            intervalVariance = 0,
            minSpeed = 8,
            maxSpeed = 20,
            minAcceleration = 0,
            maxAcceleration = 4
        }
    }
}

return windEmitter