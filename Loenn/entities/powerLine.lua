local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local yellow = xnaColors.Yellow
local white = xnaColors.White
local black = xnaColors.Black
local magenta = xnaColors.Magenta
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")
local powerLine = {}

powerLine.name = "PuzzleIslandHelper/PowerLine"
powerLine.nodeLimits = {1, -1}
powerLine.minimumSize = {8, 8}
powerLine.nodeVisibility = "never"
powerLine.nodeLineRenderType = "line"
powerLine.depth = -11000


powerLine.placements = {
    name = "Power Line",
    data = {
        flags = "",
        depth = 2,
        startFade = 10,
        endFade = 25,
        length = 50,
        speed = 1,
        colorA = "FFFFFF",
        colorB = "FFFFFF",
        altColorA = "FFFFFF",
        altColorB = "FFFFFF",
        alphaA = 1,
        alphaB = 0,
        tweenDuration = 1,
        backwards = false,
        connectEnds = false,
        invertFlag = false,
        offset = 0,
        flagMode = "Hide",
        changeFlags = false,
        displayLengths = false

    }
}
powerLine.fieldOrder =
{
    "x","y","flags","flagMode","startFade","endFade","length","speed","alphaA","alphaB","tweenDuration","offset",
    "colorA","colorB","altColorA","altColorB","depth","backwards","connectEnds","invertFlag","changeFlags","displayLengths"
}
local flagModes = {"Hide","AlternateColor"}
powerLine.fieldInformation = 
{
    colorA =  {
        fieldType = "color",
        allowXNAColors = true
    },
    colorB = {
        fieldType = "color",
        allowXNAColors = true
    },
    altColorA = {
        fieldType = "color",
        allowXNAColors = true
    },
    altColorB = {
        fieldType = "color",
        allowXNAColors = true
    },
    depth = {
        fieldType = "integer"
    },
    flagMode = {
        options = flagModes,
        editable = false
    }
}
local cornerTextureQuads = {
    upLeft = {2,1},
    upRight = {1,1},
    downRight = {1,0},
    downLeft = {2,0}
}

local function getDirection(x1, y1, x2, y2)
    if x2 < x1 and y1 == y2 then
        return "left"

    elseif x2 > x1 and y1 == y2 then
        return "right"

    elseif y2 < y1 and x1 == x2 then
        return "up"

    elseif y2 > y1 and x1 == x2 then
        return "down"
    end

    return "none"
end

local function getCornerType(direction, nextDirection)
    if direction == "right" and nextDirection == "up" or direction == "down" and nextDirection == "left" then
        return "upLeft"

    elseif direction == "down" and nextDirection == "right" or direction == "left" and nextDirection == "up" then
        return "upRight"

    elseif direction == "left" and nextDirection == "down" or direction == "up" and nextDirection == "right" then
        return "downRight"

    elseif direction == "up" and nextDirection == "left" or direction == "right" and nextDirection == "down" then
        return "downLeft"
    end

    return "unknown"
end

local function getLength(x1, y1, x2, y2, width, startNode, endNode)
    local length = math.sqrt((x1 - x2)^2 + (y1 - y2)^2)

    if startNode then
        length += width / 2
    end

    if endNode then
        length -= width / 2
    end

    return math.floor(length)
end

local function getStraightDrawingInfo(x1, y1, x2, y2, width, length, direction, startNode, endNode)
    if direction == "up" then
        return x2 - width / 2, y2 - (endNode and 0 or width / 2), width, length

    elseif direction == "right" then
        return x1 + (startNode and 0 or width / 2), y1 - width / 2, width, length

    elseif direction == "down" then
        return x1 - width / 2, y1 + (startNode and 0 or width / 2), width, length

    elseif direction == "left" then
        return x2 - (endNode and 0 or width / 2), y2 - width / 2, width, length
    end

    return x1, y1, width, length
end

local function getCornerDrawingInfo(cornerType, direction, width, length)
    local verticalWallX, horizontalWallY = -1, -1
    local offsetX, offsetY = 0, 0

    if cornerType == "upLeft" then
        verticalWallX = width - 8
        horizontalWallY = width - 8

    elseif cornerType == "upRight" then
        verticalWallX = 0
        horizontalWallY = width - 8

    elseif cornerType == "downRight" then
        verticalWallX = 0
        horizontalWallY = 0

    elseif cornerType == "downLeft" then
        verticalWallX = width - 8
        horizontalWallY = 0
    end

    if direction == "right" then
        offsetX = length - width

    elseif direction == "down" then
        offsetY = length - width
    end

    return verticalWallX, horizontalWallY, offsetX, offsetY
end

local function addSprite(sprites, texture, x, y, quadX, quadY)
    local sprite = drawableSprite.fromTexture(texture, {x = x, y = y})

    sprite:useRelativeQuad(quadX * 8, quadY * 8, 8, 8)
    table.insert(sprites, sprite)
end

local function addStraightHorizontalSprites(sprites, texture, x, y, width, length, direction, startNode, endNode)
    for ox = 0, length - 1, 8 do
        addSprite(sprites, texture, x + ox, y, 0, 0)
    end
end

local function addStraightVerticalSprites(sprites, texture, x, y, width, length, direction, startNode, endNode)
    for oy = 0, length - 1, 8 do
        addSprite(sprites, texture, x, y + oy, 0, 1)
    end
end

local function addCornerSprites(sprites, texture, x, y, width, aaa, length, direction, cornerType, startNode, endNode)
    if endNode or width < 8 or cornerType == "unknown" then
        return
    end
    local cornerQuad = cornerTextureQuads[cornerType]
    local verticalWallX, horizontalWallY, offsetX, offsetY = getCornerDrawingInfo(cornerType, direction, aaa, length)

    for ox = 0, 8 - 1, 8 do
        for oy = 0, 8 - 1, 8 do
            addSprite(sprites, texture, x + ox + offsetX, y + oy + offsetY, cornerQuad[1], cornerQuad[2])
        end
    end
end

local function addSectionSprites(sprites, entity, texture, px, py, nx, ny, nnx, nny, width, startNode, endNode)
    local direction = getDirection(px, py, nx, ny)
    local nextDirection = getDirection(nx, ny, nnx, nny)
    local cornerType = getCornerType(direction, nextDirection)
    local pipeLength = getLength(px, py, nx, ny, width, startNode, endNode)

    local vertical = direction == "up" or direction == "down"
    local horizontal = direction == "left" or direction == "right"

    local drawX, drawY, drawWidth, drawLength = getStraightDrawingInfo(px, py, nx, ny, width, pipeLength, direction, startNode, endNode)
    local straightLength = not endNode and cornerType ~= "unknown" and drawLength - drawWidth or drawLength
    local drawingOffset = not endNode and (direction == "up" or direction == "left") and drawWidth or 0

    if horizontal then
        addStraightHorizontalSprites(sprites, texture, drawX + drawingOffset, drawY, drawWidth, straightLength, direction, startNode, endNode)
        addCornerSprites(sprites, texture, drawX, drawY, drawWidth, width, pipeLength, direction, cornerType, startNode, endNode)

    elseif vertical then
        addStraightVerticalSprites(sprites, texture, drawX, drawY + drawingOffset, drawWidth, straightLength, direction, startNode, endNode)
        addCornerSprites(sprites, texture, drawX, drawY, drawWidth, width, pipeLength, direction, cornerType, startNode, endNode)
    end
end

local function addSprites(sprites, entity)
    local texture
    if not entity.texturePath or entity.texturePath == '' then
        texture = "objects/PuzzleIslandHelper/powerLineSimple"
    else
        texture = entity.texturePath
    end
    local width = 8

    local px, py = entity.x, entity.y
    local nodes = entity.nodes or {}

    for i, node in ipairs(nodes) do
        local startNode, endNode = i == 1, i == #nodes
        local nx, ny = node.x, node.y
        local nnx = endNode and -1 or nodes[i + 1].x
        local nny = endNode and -1 or nodes[i + 1].y

        addSectionSprites(sprites, entity, texture, px, py, nx, ny, nnx, nny, width, startNode, endNode)

        px, py = nx, ny
    end

    return sprites
end
function powerLine.depth(room,entity)
return entity.depth
end
local function getLines(entity)
    local color = {255,0,0,255}
    local lines = {}
    local n = entity.nodes
    if n then
        local p = {x = entity.x, y = entity.y}
        table.insert(lines, getLine(p,n[1],color))
        for i = 2, #n do
            local p1 = n[i-1]
            local p2 = n[i]
            table.insert(lines,getLine(p1,p2,color))
        end
        if entity.connectEnds then
            table.insert(lines,getLine(n[#n],p,color))
        end
    end
    return lines
end

local function getLine(p1, p2, color)
    local line =
        drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({color.r /255, color.g/255, color.b/255, color.a/255})
            love.graphics.line({p1.x,p1.y, p2.x,p2.y})
            end)
        end)
    return line
end
local function getDistanceText(a, b)
    local length = math.sqrt((a.x-b.x)^2 + (a.y-b.y)^2)
    local mp = {x = a.x + (b.x - a.x) / 2, y = a.y + (b.y - a.y) / 2}
    return getText(length, mp, 8, 1000)
end
local function getColors(color, fillAmount, borderAmount, brighter)
    local colors = {}
    local addition = brighter and 0.2 or 0.0
    local mults = {fillAmount + addition, borderAmount + addition}
    local fill = {color[1] * mults[1], color[2] *mults[1], color[3] * mults[1], 0.6 + addition}
    local border ={color[1] * mults[2], color[2] * mults[2], color[3] * mults[2], 0.8 + addition}
    table.insert(colors, fill)
    table.insert(colors, border)
    return colors
end
local function getRectangle(room, entity, position, brighter, size, color)
    local colors = getColors(color,0.3, 0.8, brighter)
    local rectangle = utils.rectangle(position.x - size / 2, position.y - size / 2, size, size)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle, colors[1], colors[2])
    return rect
end
function powerLine.sprite(room, entity)
    local sprites = getLines(entity)
    local p = {x = entity.x, y = entity.y}
    table.insert(sprites, getRectangle(room,entity,p,true,5,magenta))
    for j = 1, #entity.nodes do
        local n = {x = entity.nodes[j].x,y = entity.nodes[j].y}
        table.insert(sprites, getRectangle(room, entity, n, false, 3, black))
    end
    if entity.displayLengths and #entity.nodes > 0 then
        local p2 = {x = entity.nodes[1].x,y= entity.nodes[1].y}
        table.insert(sprites,getDistanceText(p, p2))
        if #entity.nodes > 1 then
            for i = 2, #entity.nodes do
                p = {x = entity.nodes[i-1].x, y = entity.nodes[i-1].y}
                p2 = {x = entity.nodes[i].x,y = entity.nodes[i].y}
                table.insert(sprites,getDistanceText(p2,p))
            end
        end
    end
    return sprites
end
local function getText(text, position, width, height)
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, position.x, position.y, width, height, font, 1)
                end)
            end)
    return result
end
function powerLine.selection(room, entity)
    local nodes = entity.nodes or {}
    local nodeRectangles = {}
    local mainRectangle = utils.rectangle(entity.x - 2, entity.y - 2, 5, 5)

    for i, node in ipairs(nodes) do
        nodeRectangles[i] = utils.rectangle(node.x - 2, node.y - 2, 5, 5)
    end

    return mainRectangle, nodeRectangles
end

return powerLine