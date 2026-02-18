local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local musicHint = {}

musicHint.name = "PuzzleIslandHelper/MusicHint"

musicHint.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
musicHint.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
musicHint.placements = {
    name = "Music Hint",
    data = {
        width = 8,
        height = 8,
        unravelled = true,
        rate1 = -1,
        rate2 = -1,
        rate3 = -1,
        rate4 = -1,
        ravelledAngle = 180,
        visibleIndents = false,
        visibleOrbs = false,
        finishDuration = 1.5,
        flagOnFinish = ""

    }
}
musicHint.depth = 10

return musicHint