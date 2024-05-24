local drawableFunction = require("structs.drawable_function")
local utils = require("utils")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local coral= {}
coral.justification = { 0, 0 }
coral.canResize = {false, false}
coral.minimumSize = {8,8}
coral.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
coral.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
coral.name = "PuzzleIslandHelper/Coral"

coral.depth = -100001
coral.placements =
{
    {
        name = "Coral",
        data = 
        {
            maxLimbs = 7,
            maxSegments = 30,
            color = "ffffff",
            color2 = "0000ff",
            angle = -90,
            segmentLength = 2,
        }
    }
}
function coral.sprite(room, entity)
    local font = love.graphics.getFont()
    local text = "c"
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, entity.x - 0.5, entity.y - 1, 8, 8, font, 1)
                end)
            end)
    local sprites = {}
     local white = {1,1,1,1}
    local rectangle = utils.rectangle(entity.x, entity.y, 8, 8)

    local mults = {0.3, 0.8}
    local fill = {lightBlue[1] * mults[1], lightBlue[2] * mults[1], lightBlue[3] * mults[1], 0.6}
    local border ={lightBlue[1] * mults[2], lightBlue[2] * mults[2], lightBlue[3] * mults[2], 0.8}
    local rect = drawableRectangle.fromRectangle("bordered",rectangle,fill, border)
    table.insert(sprites,rect)
    table.insert(sprites,result)
    return sprites
end
function coral.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    return utils.rectangle(x,y, 8,8)
end

coral.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    color2 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}


return coral