local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")
local logging = require("logging")
local xnaColors = require("consts.xna_colors")
local gray = xnaColors.Gray
local modes = {"Direct","From ID"}
local runeDisplay = {}
runeDisplay.justification = { 0, 0 }
runeDisplay.name = "PuzzleIslandHelper/RuneDisplay"
runeDisplay.fillColor = {gray[1] * 0.3, gray[2] * 0.3, gray[3] * 0.3, 0.6}
runeDisplay.borderColor = {gray[1] * 0.8, gray[2] * 0.8, gray[3] * 0.8, 0.8}
runeDisplay.depth = 1
runeDisplay.placements =
{
    {
        name = "Rune Display",
        data = {
            mode = "Direct",
            runeId = "",
            rune = "",
            flag = "",
            width = 24,
            height = 16,
            depth = 1,
            drawBounds = true,
            drawAllPoints = true,
        }
    }
}
runeDisplay.fieldInformation =
{
    mode = 
    {
        options = modes,
        editable = false
    }
}
local xFactor = 0.16666666666666666
local yFactor = 0.5
local indexOffsets = {{x = 1, y = 0},
                      {x = 3, y = 0},
                      {x = 5, y = 0},
                      {x = 0, y = 1},
                      {x = 2, y = 1},
                      {x = 4, y = 1},
                      {x = 6, y = 1},
                      {x = 1, y = 2},
                      {x = 3, y = 2},
                      {x = 5, y = 2}}

                      
local function getLine(p1, p2)
    local line =
    drawableFunction.fromFunction(function()
        drawing.callKeepOriginalColor(function()
        love.graphics.setColor({255, 0, 0})
        love.graphics.line({p1.x, p1.y, p2.x, p2.y})
        end)
    end)
    return line
end
local function getFallbackLine(entity)
    local fallbackLine =
        drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({255, 0, 0})
            love.graphics.line({entity.x,entity.y, entity.x + 8,entity.y + 8})
            end)
        end)
    return fallbackLine
end
local function getText(text, position, width, height)
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, position.x, position.y, width or 8, height or 8, font, 1)
                end)
            end)
    return result
end
local function getIndexOffset(entity, index, width, height)
    local ox = entity.x
    local oy = entity.y-height
    local pair = indexOffsets[index + 1]
    return {x = ox + (pair.x * xFactor) * width, y = oy + (pair.y * yFactor) * height}
end
local function getIndexPositions(entity, width, height)
    local result = {}
    local list = entity.rune or "None"
    for token in list:gmatch("%S+") do
        if string.len(token) == 2 then
            local c1 = tonumber(token:sub(1,1))
            local c2 = tonumber(token:sub(2,2))
            local s = getIndexOffset(entity, c1, width, height)
            local e = getIndexOffset(entity, c2, width, height)
            table.insert(result, {a = s, b = e})
        end
    end
    return result
end
local function getLineSprites(entity, width, height)
    local lines = {}
    local ind = getIndexPositions(entity, width, height)
    if ind then 
        for i = 1, #ind, 1 do
            local p1 = ind[i].a
            local p2 = ind[i].b
            if p1 and p2 then
                p1.y += height
                p2.y += height
                table.insert(lines,getLine(p1,p2))
            else
                table.insert(lines,getFallbackLine(entity))
                return lines
            end
        end
    end
    return lines
end

function runeDisplay.sprite(room, entity)
    local sprites = getLineSprites(entity, entity.width or 8, entity.height or 8)
    if entity.mode == "FromID" then
        local p = {x = entity.x, y = entity.y}
        table.insert(sprites, getText(entity.runeId,p,entity.width or 8, 8))
    end
    return sprites
end
return runeDisplay