local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local xnaColors = require("consts.xna_colors")
local black = xnaColors.LightBlue

local blackBox= {}

blackBox.fillColor = {black[1] * 0.3, black[2] * 0.3, black[3] * 0.3, 0.4}
blackBox.borderColor = {black[1] * 0.8, black[2] * 0.8, black[3] * 0.8, 0.8}
blackBox.name = "PuzzleIslandHelper/DevComment"
blackBox.placements =
{
    {
        name = "Dev Comment",
        data = 
        {
            width = 8,
            height = 8,
            text = ""
        }
    }
}

return blackBox