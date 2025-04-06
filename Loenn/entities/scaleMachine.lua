local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local scaleMachine = {}
local doorSizes = {"1","2","3","4"}
scaleMachine.name = "PuzzleIslandHelper/ScaleMachine"

scaleMachine.placements = {
    name = "Scale Machine",
    data = {
        scale = 1,
        leftDoorOffset= 0,
        rightDoorOffset = 0,
        leftDoorSize = "1",
        rightDoorSize = "1",
        platformLength = 16
    }
}
scaleMachine.fieldInformation = 
{
    leftDoorSize = 
    {
        options = doorSizes,
        editable = false
    },
    rightDoorSize =
    {
        options = doorSizes,
        editable = false
    }
}
function scaleMachine.sprite(room,entity)
    local sprites = {}
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/scaleMachine/top", entity)
    sprite:setScale(entity.scale or 1, entity.scale or 1)
    sprite:setJustification(0,0)
    table.insert(sprites, sprite)

    local sprite2 = drawableSprite.fromTexture("objects/PuzzleIslandHelper/scaleMachine/inside", entity)
    sprite2:setScale(entity.scale or 1, entity.scale or 1)
    sprite2:setJustification(0,0)
    sprite2:addPosition(0, sprite.meta.height * entity.scale)
    table.insert(sprites, sprite2)
    return sprites
end
scaleMachine.fieldOrder = {
    "x","y",
    "leftDoorOffset",
    "rightDoorOffset",
    "leftDoorSize",
    "rightDoorSize",
    "platformLength", "scale"
}
scaleMachine.depth = 0

return scaleMachine