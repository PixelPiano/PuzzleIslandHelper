local drawableSprite = require("structs.drawable_sprite")

local window= {}
window.canResize={true,true}
window.name = "PuzzleIslandHelper/WindowDecal"

window.depth = 9001

window.placements =
{
    {
        name = "WindowDecal",
        data = 
        {
            opacity = 0.6,
            color = "FFFFFF",
            fg = false,
            customTag = ""
        }
    }
}
local texture = "objects/PuzzleIslandHelper/window/bigWindowB"


function window.sprite(room, entity)
   local windowSprite = drawableSprite.fromTexture(texture, entity)
   windowSprite:setScale(entity.width/160,entity.height/112)
   windowSprite:setOffset(0,0)
   return windowSprite
end


window.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return window