local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local black = xnaColors.Black

local blackBox= {}

blackBox.fillColor = {black[1] * 0.3, black[2] * 0.3, black[3] * 0.3, 0.4}
blackBox.borderColor = {black[1] * 0.8, black[2] * 0.8, black[3] * 0.8, 0.8}
blackBox.canResize={true,true}
blackBox.name = "PuzzleIslandHelper/BlackBox"
function blackBox.depth(room, entity)
    return entity.depth or 1
end
blackBox.placements =
{
    {
        name = "Black Box",
        data = 
        {
            width = 8,
            height = 8,
            fadeTime = 1,
            flag = "",
            inverted = false,
            depth = -10000
        }
    }
}

return blackBox