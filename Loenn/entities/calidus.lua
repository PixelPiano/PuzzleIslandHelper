local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local calidus= {}

calidus.name = "PuzzleIslandHelper/Calidus"
calidus.depth = 0

calidus.texture = "characters/PuzzleIslandHelper/Calidus/lonn"
local directions = {"Center","Left","Right","Up","Down","UpLeft","DownLeft","DownRight","UpRight","Player"}
function calidus.sprite(room,entity)
   local shapeSprite
   local path = "characters/PuzzleIslandHelper/Calidus/"
   if entity.broken then
        shapeSprite = drawableSprite.fromTexture(path .. "formation00", entity)
   else
        shapeSprite = drawableSprite.fromTexture(path .. "lonn", entity)
   end
   
   return shapeSprite

end
calidus.placements =
{
    {
        name = "Calidus",
        data = 
        {
            broken = false,
            looking = "Center",
            startFloating = true
        }
    }
}

calidus.fieldInformation = 
{
    looking = {
        editable = false,
        options = directions
    }
}
return calidus