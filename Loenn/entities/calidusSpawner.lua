local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local calidus= {}

calidus.name = "PuzzleIslandHelper/CalidusSpawner"
calidus.depth = 0

calidus.texture = "characters/PuzzleIslandHelper/Calidus/lonn"

function calidus.sprite(room,entity)
   local shapeSprite
   local path = "characters/PuzzleIslandHelper/Calidus/"
   return drawableSprite.fromTexture(path .. "lonn", entity)
end
calidus.placements =
{
    {
        name = "Calidus Spawner",
        data = 
        {
            eyeFlag = "",
            headFlag = "",
            leftArmFlag = "",
            rightArmFlag = ""
        }
    }
}
return calidus