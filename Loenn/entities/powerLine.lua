local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

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
        flag = "",
        inverted = false,
        texturePath = "",
        depth = 2,
        lineStartFade = 10,
        lineEndFade = 25,
        lineLength = 50,
        lineSpeed = 1,
        lineColorA = "FFFFFF",
        lineColorB = "FFFFFF",
        stateChangeDuration = 1,
        backwards = false
    }
}
powerLine.fieldInformation = 
{
    lineColorA =  {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true
    },
    lineColorB = {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true
    },
    depth = {
        fieldType = "integer"
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

function powerLine.sprite(room, entity)
    return addSprites({}, entity)
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