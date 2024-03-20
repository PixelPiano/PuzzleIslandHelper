local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")

local shiftArea = {}
shiftArea.depth = -10001
shiftArea.name = "PuzzleIslandHelper/ShiftArea"
shiftArea.nodeLimits = {2,-1}
shiftArea.lineRenderType = "line"
shiftArea.nodeVisibility = "always"
shiftArea.canResize = {false, false}
shiftArea.minimumSize = {8,8}
shiftArea.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
shiftArea.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
shiftArea.placements = {
    name = "Shift Area",
    data = {
        width = 8,
        height = 8,
        bgFrom = "",
        bgTo = "",
        fgFrom = "",
        fgTo = "",
        indices = ""
    }
}
function shiftArea.sprite(room, entity)
    local sprites = {}

    local font = love.graphics.getFont()
    local text = "M"
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, entity.x, entity.y, entity.width or 8, entity.height or 8, font, 1)
                end)
            end)
    local fill = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
    local border ={lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
    local fillColor = utils.callIfFunction(fill, room, entity)
    local borderColor = utils.callIfFunction(border, room, entity)
    local sprite = drawableRectangle.fromRectangle("bordered", entity.rectangle, fillColor, borderColor):getDrawableSprite()
    table.insert(sprites,result)
    table.insert(sprites,sprite)
    return sprites
end

function shiftArea.nodeSprite(room, entity, node, nodeIndex, viewport)
    local sprites = {}

    local font = love.graphics.getFont()
    local text = tostring(node)
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, node.x, node.y, entity.width or 8, entity.height or 8, font, 1)
                end)
            end)
    local fill = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
    local border ={lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
    local fillColor = utils.callIfFunction(fill, room, entity)
    local borderColor = utils.callIfFunction(border, room, entity)
    local sprite = drawableRectangle.fromRectangle("bordered",node.x, node.y, entity.width or 8, entity.height or 8, fillColor, borderColor):getDrawableSprite()
    table.insert(sprites,result)
    table.insert(sprites,sprite)
    return sprites
end
function shiftArea.fieldInformation()
    return {
        bgFrom = {
           options = fakeTilesHelper.getTilesOptions("tilesBg"),
           editable = false
        },
        bgTo = {
           options = fakeTilesHelper.getTilesOptions("tilesBg"),
           editable = false
        },
        fgFrom = {
           options = fakeTilesHelper.getTilesOptions("tilesFg"),
           editable = false
        },
        fgTo = {
            options = fakeTilesHelper.getTilesOptions("tilesFg"),
            editable = false
        }
    }
end


return shiftArea