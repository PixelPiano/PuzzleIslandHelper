local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local customNote = {}

customNote.name = "PuzzleIslandHelper/CustomNote"

customNote.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
customNote.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
customNote.placements = {
    name = "Custom Note",
    data = {
        width = 8,
        height = 8,
        rotateRate = 0,
        rotateUpdateInterval = -1,
        lineLengthOffset = 3,
        size = 8,
        alpha = 1,
        fadeDistance = 8,
        onlyOnce = false,
        oncePerSession = false,
        flagsOnFinish = "",
        requiredFlags = "",
        shineOffsetX = "",
        shineOffsetY = "",
        useDialog = true,
        text = "",
        minAngle = 0,
        maxAngle = 360,
        flashes = false,
        flashAttack = 0,
        flashSustain = 0,
        flashRelease = 0,
        flashWait = 0,
        flashDelay = 0,
        flashIntensity = 0,
        fadeX = false,
        fadeY = false,
        scrambleFlag = "",
        scramble = false
    }
}

return customNote