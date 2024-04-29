local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ascwiit= {}
ascwiit.justification = { 0, 0 }

ascwiit.name = "PuzzleIslandHelper/Ascwiit"

ascwiit.depth = 1

ascwiit.texture = "objects/PuzzleIslandHelper/ascwiit/lonn"

ascwiit.placements =
{
    {
        name = "Ascwiit",
        data = 
        {
            scale = 1
        }
    }
}
function ascwiit.scale(room, entity)
     return {1.0 * (entity.scale or 1), 1.0 * (entity.scale or 1)}

end


return ascwiit