local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local movingJelly = {}
local directions = {"None","Up", "Down", "Left", "Right"}
movingJelly.name = "PuzzleIslandHelper/MovingJelly"
movingJelly.depth = -5

movingJelly.fieldInformation = {
    directionA = {
        editable = false,
        options = directions
    },
    directionB = {
        editable = false,
        options = directions
    },
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}
movingJelly.placements = {
    {
        name = "Moving Jelly",
        data = {
            tutorial = false,
            bubble = false,
            directionA = "Up",
            directionB = "None",
            toggleDirection = false,
            rate = 10,
            startActive = true,
            flag = "",
            disableGravity = true,
            color = "00FF00",
            dragPlayerAlong = false,
            playerCanInfluence = false,
        }
    }
}


local texture = "objects/glider/idle0"

function movingJelly.sprite(room, entity)
    local bubble = entity.bubble

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(texture, entity)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites

    else
        return drawableSprite.fromTexture(texture, entity)
    end
end

function movingJelly.rectangle(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    return sprite:getRectangle()
end

return movingJelly