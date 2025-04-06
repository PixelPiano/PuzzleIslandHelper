local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local randomTeleportArea = {}

randomTeleportArea.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
randomTeleportArea.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
randomTeleportArea.name = "PuzzleIslandHelper/RandomTeleportArea"
randomTeleportArea.nodeLimits = {1, -1}
randomTeleportArea.minimumSize = {8, 8}
randomTeleportArea.nodeVisibility = "always"
randomTeleportArea.nodeLineRenderType = "line"
randomTeleportArea.depth = -11000


randomTeleportArea.placements = {
    name = "Random Teleport Area",
    data = {
        width = 8,
        height = 8,
        waitUntilLeave = true,
        randomTeleport = false
    }
}


return randomTeleportArea