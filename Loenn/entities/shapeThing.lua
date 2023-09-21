local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local shapeThing= {}

shapeThing.canResize={true,true}
shapeThing.name = "PuzzleIslandHelper/ShapeThing"

shapeThing.depth = -8500

shapes = {"Cube", "Tetrahedron","Octahedron","RandomStatic","RandomMorphing"}
glitches = {"None","Gradual","Flashy"}
shapeThing.placements =
{
    {
        name = "Shape Thing",
        data = 
        {
            shape = "Cube",
            glitch = "None",
            randomPoints = 6,
            color = "00FF00",
            lineThickness = 4,
            xSpeed = 1,
            ySpeed = 1,
            zSpeed = 1,
            rotateX = true,
            rotateY = true,
            rotateZ = true,
            randomizePointLimit = false,
            connectAllPoints = false,
        }
    }
}
local texture = "objects/PuzzleIslandHelper/shape/lonn"


function shapeThing.sprite(room, entity)
   local shapeSprite = drawableSprite.fromTexture(texture, entity)
   shapeSprite:setScale(entity.width/64,entity.width/64)
   shapeSprite:setOffset(0,0)
   return shapeSprite
end


shapeThing.fieldInformation = {
      shape =
    {
        options = shapes,
        editable = false,
    },
        glitch  =
    {
        options = glitches,
        editable = false,
    },
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return shapeThing