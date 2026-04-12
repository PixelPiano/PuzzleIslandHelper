local xnaColors = require("consts.xna_colors")
local black = xnaColors.Black
local white = xnaColors.White
local red = xnaColors.Red
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")

local dashCodeGate = {}
dashCodeGate.depth = -10001
dashCodeGate.name = "PuzzleIslandHelper/DashCodeGate"
dashCodeGate.minimumSize = {16,16}
dashCodeGate.fillColor = {black[1] * 0.3,black[2] * 0.3,black[3] * 0.3, 0.6}
dashCodeGate.borderColor = {white[1] * 0.8, white[2] * 0.8, white[3] * 0.8, 0.8}
dashCodeGate.placements = {
    name = "Dash Code Gate",
    data = {
        width = 16,
        height = 16,
        required = 0,
    }
}

local function getBorderedRectangle(x, y, width, height, fillColor, borderColor)
    local rectangle = utils.rectangle(x,y, width, height)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle,fillColor,borderColor)
    return rect
end
local function getRectangle(x, y, width, height, color)
    local rectangle = utils.rectangle(x,y, width, height)
    local rect = drawableRectangle.fromRectangle("fill", rectangle,color,color)
    return rect
end
local function getColors(fillAmount, borderAmount, fillColor, borderColor)
    local colors = {}
    local mults = {fillAmount, borderAmount}
    local fill = {fillColor[1] * mults[1], fillColor[2] *mults[1], fillColor[3] * mults[1], 0.6}
    local border ={borderColor[1] * mults[2], borderColor[2] * mults[2], borderColor[3] * mults[2], 0.8}
    table.insert(colors, fill)
    table.insert(colors, border)
    return colors
end
function dashCodeGate.sprite(room, entity)
    local sprites = {}
    local colors = getColors(0.3,0.8,black,white)
    local rect = getBorderedRectangle(entity.x + 1,entity.y + 1,entity.width - 2,entity.height - 2,colors[1],colors[2])
    table.insert(sprites,rect)

    local space = 4;
    local size = math.min(entity.width,entity.height)
    local maxPerRow = (size - 8) / space
    local rows = math.ceil(entity.required / maxPerRow)
    for row = 1, rows, 1 do
        local y = entity.y + entity.height / 2 - (rows * space) / 2 + (row - 1) * space
        local nodesInRow
        if row * maxPerRow < entity.required then
            nodesInRow = maxPerRow
        else
            nodesInRow = entity.required - (row - 1) * maxPerRow
        end
        local x = (entity.x + entity.width * 0.5) + (-nodesInRow / 2 + 0.5) * space
        for col = 1, nodesInRow, 1 do
            local r = getRectangle(x + (col - 1) * space,y,1,1,red)
            table.insert(sprites,r)
        end
    end
    
    return sprites
end

return dashCodeGate