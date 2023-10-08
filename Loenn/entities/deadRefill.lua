local drawableSprite = require("structs.drawable_sprite")
local deadRefill = {}
deadRefill.justification = { 0, 0 }

deadRefill.name = "PuzzleIslandHelper/DeadRefill"

deadRefill.depth = 0

choices = {"A","B","C"}

deadRefill.texture = "objects/PuzzleIslandHelper/deadRefill/idleA"


deadRefill.placements =
{
    {
        name = "Dead Refill",
        data = {
            type = "A",
            explosive = false,
            colorful = false,

        }
    }
}
function deadRefill.sprite(room,entity)
   local path = "objects/PuzzleIslandHelper/deadRefill/idle"
   
   return drawableSprite.fromTexture(path .. entity.type, entity)

end
deadRefill.fieldInformation = {
    type = {
        editable = false,
        options = choices
    }
}

return deadRefill