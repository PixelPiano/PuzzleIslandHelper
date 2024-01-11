local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local observer= {}
observer.justification = { 0, 0 }

observer.name = "PuzzleIslandHelper/Observer"

observer.depth = 0

observer.texture = "objects/PuzzleIslandHelper/observer/observer00"

local modes = {"Off","Scanning","Detected","Alert"}
function observer.sprite(room,entity)
   local sprite
   local path = "objects/PuzzleIslandHelper/observer/observer"
   if entity.digital then
        sprite = drawableSprite.fromTexture(path .. "Digital00", entity)
   else
        sprite = drawableSprite.fromTexture(path .. "00", entity)
   end
   return sprite
end
observer.placements =
{
    {
        name = "Observer",
        data = 
        {
            signalWidth = 32,
            signalHeight = 16,
            signalTime = 2,
            mode = "Scanning",
            digital = false
        }
    }
}
observer.fieldInformation =
{
    mode = 
    {
        options = modes,
        editable = false
    }
}

return observer