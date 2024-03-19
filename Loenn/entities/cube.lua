local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local cube= {}

cube.canResize={true,true}
cube.name = "PuzzleIslandHelper/Cube"

cube.depth = -8500

cube.placements =
{
    {
        name = "Cube",
        data = 
        {
        }
    }
}
local texture = "objects/PuzzleIslandHelper/shape/lonn"


function cube.sprite(room, entity)
   local shapeSprite = drawableSprite.fromTexture(texture, entity)
   shapeSprite:setScale(entity.width/64,entity.width/64)
   shapeSprite:setOffset(0,0)
   return shapeSprite
end

return cube