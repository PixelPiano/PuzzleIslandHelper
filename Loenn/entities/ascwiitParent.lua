local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ascwiit= {}
ascwiit.justification = { 0, 0 }

ascwiit.name = "PuzzleIslandHelper/AscwiitParent"

ascwiit.depth = 1

ascwiit.texture = "objects/PuzzleIslandHelper/ascwiit/lonn"

ascwiit.placements =
{
    {
        name = "Ascwiit Parent",
        data = 
        {
            flag = "",
            eatedFirfil = false,
        }
    }
}


return ascwiit