local xnaColors = require("consts.xna_colors")
local white = xnaColors.White

local wipTeleport = {}

wipTeleport.name = "PuzzleIslandHelper/WipTeleport"
wipTeleport.fillColor = {white[1] * 0.3, white[2] * 0.3, white[3] * 0.3, 0.6}
wipTeleport.borderColor = {white[1] * 0.8, white[2] * 0.8, white[3] * 0.8, 0.8}
wipTeleport.placements = {
    name = "Wip Teleport",
    data = {
        width = 8,
        height = 8,
        room = "",
        markerID = "",
        flag = "",
        flagOnUse = ""
    }
}
wipTeleport.depth = -1000000

return wipTeleport