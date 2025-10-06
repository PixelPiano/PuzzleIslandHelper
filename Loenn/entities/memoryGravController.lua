local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local memoryGravController = {}

memoryGravController.name = "PuzzleIslandHelper/MemoryGravController"

memoryGravController.depth = 1
memoryGravController.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
memoryGravController.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
local bubbles = {"Maddy","Calidus","Will"}
memoryGravController.placements = {
    name = "Memory Grav Controller",
    data = {
        width = 8,
        height = 8,
        flag = "",
        key = ""
    }
}

return memoryGravController