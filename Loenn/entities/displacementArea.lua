local xnaColors = require("consts.xna_colors")
local white = xnaColors.White

local displacementArea = {}

displacementArea.name = "PuzzleIslandHelper/DisplacementArea"
displacementArea.fillColor = {white[1] * 0.3, white[2] * 0.3, white[3] * 0.3, 0.6}
displacementArea.borderColor = {white[1] * 0.8, white[2] * 0.8, white[3] * 0.8, 0.8}
displacementArea.placements = {
    name = "Displacement Area",
    data = {
        width = 8,
        height = 8,
        flag = "",
        inverted = false,
        scrollX = 1,
        scrollY = 1,
        fadeInOut = true,
        fadeTime = 1,
        alpha = 1,
        depth = 0,
        path = "PuzzleIslandHelper/displacement/test",
        delay = 0.1,
        color = "FFFFFF",
    }
}
displacementArea.fieldInformation = 
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}
function displacementArea.depth(room, entity)
    return entity.depth or 0
end

return displacementArea