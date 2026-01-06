local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local black = xnaColors.Black
local white = xnaColors.White
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")
local drawableSprite = require("structs.drawable_sprite")


local runeFirfilShape = {}
runeFirfilShape.depth = -10001
runeFirfilShape.name = "PuzzleIslandHelper/RuneFirfilShape"
runeFirfilShape.nodeLimits = {2,-1}
runeFirfilShape.nodeLineRenderType = "none"
runeFirfilShape.nodeVisibility = "always"
runeFirfilShape.canResize = {false, false}
runeFirfilShape.minimumSize = {3,3}
runeFirfilShape.placements = {
    name = "Rune Firfil Shape",
    data = {
        width = 3,
        height = 3,
        texture = "PianoBoy/infinityBlock",
        glow = "PianoBoy/infinity",
        runeID = "",
        runeWidth = 32,
        runeHeight = 24,
        runeOffsetX = 0,
        runeOffsetY = 0,
        glowOffsetX = 0,
        glowOffsetY = 0,
        centerGlowX = false,
        centerGlowY = false,
        centerRuneX = false,
        centerRuneY = false,
        nodeSize = 3
    }
}
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
local function getNodeRectangle(room, entity, position, brighter, size)
    local colors = getColors(0.3, 0.8, brighter)
    local rectangle = utils.rectangle(position.x - size / 2, position.y - size / 2, size, size)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle, colors[1], colors[2])
    return rect
end
local function getRectangle(position, width, height, color, border)
    local rectangle = utils.rectangle(position.x, position.y, width, height)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle, color, border)
    return rect
end
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

function runeFirfilShape.sprite(room, entity)
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.texture .. "00") ~= nil then
        path = "decals/" .. entity.texture .. "00"
    else
        path = "decals/" .. entity.texture
    end
    local sprite = drawableSprite.fromTexture(path, entity)
    sprite:setScale(1,1)
    sprite:setJustification(0, 0)
    sprite.rotation = math.rad(0)
    local r = sprite:getRectangle()
    local spriteWidth = r.width
    local spriteHeight = r.height
    path = ""
    if drawableSprite.fromTexture("decals/" .. entity.glow .. "00") ~= nil then
        path = "decals/" .. entity.glow .. "00"
    else
        path = "decals/" .. entity.glow
    end
    local glowSprite = drawableSprite.fromTexture(path, entity)
    glowSprite:setScale(1,1)
    glowSprite:setJustification(0, 0)
    glowSprite.rotation = math.rad(0)
    local gr = glowSprite:getRectangle()
    if entity.centerGlowX then
        glowSprite:addPosition(spriteWidth/2 - gr.width/2,0)
    end
    if entity.centerGlowY then
        glowSprite:addPosition(0,spriteHeight/2 - gr.height / 2)
    end
    glowSprite:addPosition(entity.glowOffsetX,entity.glowOffsetY)
    local runeX = entity.x + entity.runeOffsetX
    local runeY = entity.y + entity.runeOffsetY
    if entity.centerRuneX then
        runeX = runeX + spriteWidth / 2 - entity.runeWidth / 2
    end
    if entity.centerRuneY then
        runeY = runeY + spriteHeight / 2 - entity.runeHeight / 2
    end
    local rect = getRectangle({x = runeX - 2,y = runeY - 2},entity.runeWidth + 4,entity.runeHeight + 4,black, white)
    local lines = getLineSprites(entity, entity.runeWidth or 8, entity.runeHeight or 8)
    local p = {x = entity.x, y = entity.y}
    local text = getText(entity.runeID,p,entity.width or 8, 8)
    local sprites = {}
    table.insert(sprites,rect)
    for i,line in ipairs(lines) do
        table.insert(sprites,line)
    end
    table.insert(sprites,sprite)
    table.insert(sprites,glowSprite)
    table.insert(sprites,text)
    return sprites
end

function runeFirfilShape.nodeSprite(room, entity, node, nodeIndex, viewport)
    local sprites = {}
    local pos = {x = node.x, y = node.y}
    local rect = getNodeRectangle(room, entity, pos, false, 3)
    table.insert(sprites,rect)
    return sprites
end
function runeFirfilShape.selection(room, entity)
    local offset = -entity.nodeSize / 2
    local size = entity.nodeSize
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.texture .. "00") ~= nil then
        path = "decals/" .. entity.texture .. "00"
    else
        path = "decals/" .. entity.texture
    end
    local sprite = drawableSprite.fromTexture(path, entity)
    local main = sprite:getRectangle()
    main.x = main.x + main.width / 2
    main.y = main.y + main.height / 2
    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            table.insert(nodes,utils.rectangle(node.x + offset, node.y + offset, size, size))
        end
    end

    return main, nodes
end


return runeFirfilShape