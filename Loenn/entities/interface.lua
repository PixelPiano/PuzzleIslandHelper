local drawableSprite = require("structs.drawable_sprite")
local interface= {}

interface.name = "PuzzleIslandHelper/Interface"
interface.depth = 1

interface.texture = "objects/PuzzleIslandHelper/interface/keyboard"
local versions = {"Lab","Pipes"}
interface.placements =
{
    {
        name = "Computer Interface",
        data = 
        {
            type = "Lab",
            flipX = false
        }
    }
}
function interface.sprite(room, entity, viewport)

    local sprite
    if entity.type == "Lab" then
        sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/interface/keyboard", entity)
    end
    if entity.type == "Pipes" then
        sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/interface/pipes/machine00", entity)
    end
    sprite.justificationX = 0
    sprite.justificationY = 0
    return sprite
end
interface.fieldInformation =
{
    type =
    {
        options = versions,
        editable = false
    }
}

return interface