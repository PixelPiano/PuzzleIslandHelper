local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local xnaColors = require("consts.xna_colors")

local forkAmpBattery= {}

forkAmpBattery.name = "PuzzleIslandHelper/ForkAmpBattery"

forkAmpBattery.depth = 0

forkAmpBattery.texture = "objects/PuzzleIslandHelper/forkAmp/battery"

forkAmpBattery.placements =
{
    { 
        name = "Fork Amp Battery",
        data =
        {
            flagOnFinish = ""
        }
    }
}
return forkAmpBattery