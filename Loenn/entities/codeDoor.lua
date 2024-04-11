local codeDoor = {}
local drawableSprite = require("structs.drawable_sprite")
codeDoor.justification = { 0, 0 }

codeDoor.name = "PuzzleIslandHelper/CodeDoor"

codeDoor.depth = -1

codeDoor.texture = "objects/PuzzleIslandHelper/machineDoor/codeIdle00"

codeDoor.placements =
{
    name = "Code Door",
    data = 
    {
        flag = "",
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
    sprite:setJustification(0.5, 0.5)
    return sprite
end

return codeDoor