local xnaColors = require("consts.xna_colors")
local green = xnaColors.Green

local itemArea = {}

itemArea.name = "PuzzleIslandHelper/EscapeEntities/ItemArea"
itemArea.fillColor = {green[1] * 0.3, green[2] * 0.3, green[3] * 0.3, 0.6}
itemArea.borderColor = {green[1] * 0.8, green[2] * 0.8, green[3] * 0.8, 0.8}
itemArea.placements = {
    name = "Item Area",
    data = {
        width = 8,
        height = 8,
    }
}
itemArea.depth = 0

return itemArea