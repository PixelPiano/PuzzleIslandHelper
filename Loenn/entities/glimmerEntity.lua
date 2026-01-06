local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.White
local glimmer = {}

glimmer.name = "PuzzleIslandHelper/Glimmer"
glimmer.minimumSize = {8,8}

glimmer.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
glimmer.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
glimmer.placements = {
    name = "Glimmer",
    data = {
        width = 8,
        height = 8,
        depth = 1,
        flag = "",
        fadeX = false,
        fadeY = false,
        flashes = false,
        flashIntensity = 0.5,
        flashDelay = 0,
        flashAttack = 0,
        flashSustain = 0,
        flashRelease = 0,
        flashWait = 0,
        solidColor = false,
        minLineWidth = 1,
        maxLineWidth = 4,
        maxAngle = 360,
        minAngle = 0,
        lineOffset = 4,
        rotateInterval = -1,
        lines = 8,
        fadeWhenBlocked = true,
        fadeThresh = 0,
        rotateRate = 0.5,
        alpha = 1
    }
}
function glimmer.depth(room, entity)
    return entity.depth
end

return glimmer