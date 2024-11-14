local drawableSprite = require("structs.drawable_sprite")

local chair = {}

chair.justification = { 0, 0 }

chair.name = "PuzzleIslandHelper/Chair"
chair.placements = 
{
    {
        name = "Chair",
        data = {  
            path = "digiA",
            flag = "",
            sitX = 0,
            sitY = 0,
            canFace = "Both",
            depth = 1
        }
    }
}
local facings = {"Both","LeftOnly","RightOnly"}
chair.fieldInformation = {
    canFace =
    {
        options = facings,
        editable = false
    }
}
function chair.depth(room, entity)
    return entity.depth or 0
end
function chair.sprite(room, entity)
    local path = "objects/PuzzleIslandHelper/chairs/"..entity.path
    local sprite = drawableSprite.fromTexture(path, entity)
    sprite:setJustification(0, 0)
    return sprite
end
return chair
