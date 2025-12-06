local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local slope = {}

slope.justification = { 0, 0 }

slope.name = "PuzzleIslandHelper/SlopeBeta"

slope.depth = -10001
slope.texture = "objects/PuzzleIslandHelper/securityLaser/emitter00"
slope.nodeLimits = {1,1}
slope.nodeLineRenderType = "line"
slope.nodeVisibility = "always"
slope.canResize = {false, false}
function slope.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 

    return utils.rectangle(x - 8, y, 8, 8), {utils.rectangle(nodeX, nodeY, 8, 8)}
end
local eases = {"Linear","SineIn","SineOut","SineInOut","CircleIn","CircleOut"}
local inputs = {"None","Up","Down","Left","Right","Any"}
slope.placements =
{
    {
        name = "Slope (Beta)",
        data = {
            collidableFlag = "",
            requireUpInput = false,
            flagWhenOnSlope = "",
            flagState = true,
            xExtend = 8,
            platformWidth = 16,
            invertFlagWhenOffSlope = false,
            ease = "Linear",
            leftSpeedThresh = 0,
            rightSpeedThresh = 0,
            speedChangeMult = 0,
            facingOffset = false,
            depth = 0,
            fallInputA = "None",
            fallInputB = "None",
            fallInputMiddle = "None"
        }
    }
}
slope.fieldInformation = 
{
    ease = 
    {
        options = eases,
        editable = false
    },
    fallInputA = 
    {
        options = inputs,
        editable = false
    },
    fallInputB =
    {
        options = inputs,
        editable = false
    },
    fallInputMiddle =
    {
        options = inputs,
        editable = false
    }
}
function slope.sprite(room, entity)
    local line =
        drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({255, 0, 0})
            love.graphics.line({entity.x ,entity.y, entity.nodes[1].x,entity.nodes[1].y})
            end)
        end)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/securityLaser/emitter00", entity)
    sprite:addPosition(-4, 4)
    return {line, sprite}
end
function slope.nodeSprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/securityLaser/emitter00", entity)
    local nodeX = entity.nodes[1].x
    local nodeY = entity.nodes[1].y
    sprite:addPosition(nodeX - entity.x + 4, nodeY - entity.y + 4)
    return sprite
end

return slope