local xnaColors = require("consts.xna_colors")
local white = xnaColors.White

local toDoArea = {}

toDoArea.name = "PuzzleIslandHelper/MapToDoArea"
toDoArea.fillColor = {white[1] * 0.3, white[2] * 0.3, white[3] * 0.3, 0.6}
toDoArea.borderColor = {white[1] * 0.8, white[2] * 0.8, white[3] * 0.8, 0.8}
toDoArea.placements = {
    name = "ToDo Area",
    data = {
        width = 8,
        height = 8,
        note = "",
        displayInGame = false
    }
}
toDoArea.depth = -1000000

return toDoArea