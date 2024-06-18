local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local xnaColors = require("consts.xna_colors")

local forkAmpBattery= {}

forkAmpBattery.name = "PuzzleIslandHelper/ForkAmpBattery"

forkAmpBattery.depth = 0

forkAmpBattery.texture = "objects/PuzzleIslandHelper/forkAmp/idle00"

forkAmpBattery.placements =
{
    { 
        name = "Fork Amp Battery",
        data =
        {
            flagOnFinish = "",
            isLeader = true,
            subId = "",
            continuityId = ""
        }
    }
}
return forkAmpBattery