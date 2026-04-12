local codeDoor = {}
local drawableSprite = require("structs.drawable_sprite")

codeDoor.name = "PuzzleIslandHelper/Decontamination"

codeDoor.depth = -1

codeDoor.minimumSize = {16, 64}
codeDoor.maximumSize = {-1, 64}
codeDoor.canResize = {true, false}
codeDoor.placements =
{
    name = "Decontamination",
    data = 
    {
        height = 64,
        width = 16,
        panCameraFlag = "",
        openDoorsFlag = "",
        steamFlag = "",
        cutsceneOnlyOnce = true,
    }
}
function codeDoor.sprite(room, entity)
    local path ="objects/PuzzleIslandHelper/machineDoor/codeIdle00"
    local doorLeft = drawableSprite.fromTexture(path, entity)
    local doorRight = drawableSprite.fromTexture(path, entity)
    doorLeft:addPosition(0,8)
    doorRight:addPosition(entity.width - 8, 8)
    local sprites = {}
    table.insert(sprites,doorLeft)
    table.insert(sprites,doorRight)
    return sprites
end

return codeDoor