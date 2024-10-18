local xnaColors = require("consts.xna_colors")
local black = xnaColors.Black

local voidCritterWall = {}

voidCritterWall.name = "PuzzleIslandHelper/VoidCritterWall"
voidCritterWall.fillColor = {black[1] * 0.3, black[2] * 0.3, black[3] * 0.3, 0.6}
voidCritterWall.borderColor = {black[1] * 0.8, black[2] * 0.8, black[3] * 0.8, 0.8}
voidCritterWall.placements = {
    name = "Void Critter Wall",
    data = {
        width = 8,
        height = 8,
        flag = "",
        inverted = false,
        depth = 0
    }
}
function voidCritterWall.depth(room, entity)
    return entity.depth
end

return voidCritterWall