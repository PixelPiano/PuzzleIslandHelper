local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local xnaColors = require("consts.xna_colors")
local black = xnaColors.Black

local towerEntrance= {}

towerEntrance.fillColor = {black[1] * 0.3, black[2] * 0.3, black[3] * 0.3, 0.4}
towerEntrance.borderColor = {black[1] * 0.8, black[2] * 0.8, black[3] * 0.8, 0.8}
towerEntrance.canResize={true, true}
towerEntrance.name = "PuzzleIslandHelper/TowerEntrance"
towerEntrance.placements =
{
    {
        name = "Tower Entrance",
        data = 
        {
            width = 8,
            height = 8,
            lockFlag = "",
            flagsToSet = ""
        }
    }
}

return towerEntrance