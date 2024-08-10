local codeDoor = {}
local drawableSprite = require("structs.drawable_sprite")

codeDoor.name = "PuzzleIslandHelper/LabPoweredDoor"

codeDoor.depth = -1

codeDoor.texture = "objects/PuzzleIslandHelper/machineDoor/codeIdle00"

codeDoor.placements =
{
    name = "Lab Powered Door",
    data = 
    {
        sideways = false
    }
}
function codeDoor.sprite(room, entity)
    local path ="objects/PuzzleIslandHelper/machineDoor/codeIdle00"
    local sprite = drawableSprite.fromTexture(path, entity)
    local rotation = 0
    if entity.sideways then
        rotation = 90
    end
    sprite.rotation = math.rad(rotation)
    if rotation == 90 then
        sprite:setJustification(0,1)
    else
        sprite:setJustification(0,0)
    end
    return sprite
end

return codeDoor