local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local xnaColors = require("consts.xna_colors")

local forkAmp= {}

forkAmp.name = "PuzzleIslandHelper/ForkAmp"

forkAmp.depth = 0

forkAmp.texture = "objects/PuzzleIslandHelper/forkAmp/texture"

forkAmp.placements =
{
    { 
        name = "Fork Amp",
        data =
        {
            firstFlag = "",
            secondFlag = "",
            thirdFlag = "",
            fourthFlag= ""
        }
    }
}
return forkAmp