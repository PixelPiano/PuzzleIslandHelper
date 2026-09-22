local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local xnaColors = require("consts.xna_colors")
local black = xnaColors.Black
local white = xnaColors.White
local checker= {}
checker.justification = { 0, 0 }
checker.minimumSize = {16,16}
checker.name = "PuzzleIslandHelper/MiniHeartChecker"

checker.depth = -8500
checker.placements =
{
    {
        name = "Mini Heart Checker",
        data = 
        {
            width = 16,
            height = 16,
            spritePath = "",
            flags = "",
            colors = "FFFFFFFF",
            xDetect = -1,
            yDetect = -1,
            flagsOnComplete = "thisFlagWillBeTrue, !thisFlagWillBeFalse",
            wallColor = "000000FF",
            padY = 4
        }
    }
}
local function heartsWidth(heartSpriteWidth, hearts, pad)
    return hearts * (heartSpriteWidth + pad) - pad
end
local function heartsHeight(heartSpriteHeight, hearts, pad)
    return hearts * (heartSpriteHeight + pad) - pad
end
local function heartsPossible(edgeSpriteWidth, heartSpriteWidth, width, required, pad)
    local rowWidth = width - 2 * edgeSpriteWidth

    for i = 0, required do
        if heartsWidth(heartSpriteWidth, i, pad) > rowWidth then
            return i - 1
        end
    end

    return required
end
local function hex2rgba(hex)
    hex = hex:gsub("#","")
    return {tonumber("0x"..hex:sub(1,2)), tonumber("0x"..hex:sub(3,4)), tonumber("0x"..hex:sub(5,6)), tonumber("0x"..hex:sub(7,8))}
end
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



local function mysplit(inputstr, sep)
  if sep == nil then
    sep = "%s"
  end
  local t = {}
  for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
    table.insert(t, str)
  end
  return t
end
function checker.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = entity.width or 8
    local height = entity.height or 8
    local roomWidth, roomHeight = room.width, room.height
    local hearts = #mysplit(entity.flags,',')
    
    local path = entity.spritePath
    if path == nil or path == '' then
        path = "objects/PuzzleIslandHelper/miniHeartChecker/"
    end
    local heartSpriteSample = drawableSprite.fromTexture(path.."outline", entity)
    local heartWidth, heartHeight = heartSpriteSample.meta.width, heartSpriteSample.meta.height
    local colors = getColors(0.3,0.8,black,white)
    local rect = getBorderedRectangle(entity.x + 1,entity.y + 1,entity.width - 2,entity.height - 2,hex2rgba(entity.wallColor) or "000000FF",colors[1])
    local sprites = {rect}
    local position = {x = x, y = y}
    local padY = entity.padY or 4
    local heartColors = mysplit(entity.colors,',')
    local totalColors = #heartColors
    if hearts > 0 then
        local fits = heartsPossible(2, heartWidth, width, hearts, 4)
        local rows = math.ceil(hearts / fits)
        local heartColorIndex = 1
        for row = 1, rows do
            local displayedHearts = heartsPossible(2, heartWidth, width, hearts, 4)
            local drawWidth = heartsWidth(heartWidth, displayedHearts,4)
            local drawHeight = heartsHeight(heartHeight, displayedHearts,padY)
            local startX = x + utils.round((width - drawWidth) / 2)
            local startY = y - utils.round(rows / 2 * (heartHeight + padY)) - padY + (padY - 4)
            for col = 1, displayedHearts do
                local drawX = startX + (col - 1) * (heartWidth + 4)
                local drawY = startY + row * (heartHeight + padY) - padY + height / 2 - (padY / 2) + (padY - 4)
                local sprite = drawableSprite.fromTexture(path.."outline", {x = drawX,y = drawY})
                sprite:setJustification(0.0, 0.0)
                if heartColors ~= nil and heartColorIndex <= #heartColors then
                    sprite:setColor(heartColors[heartColorIndex])
                    heartColorIndex = math.max(1, (heartColorIndex + 1) % (#heartColors + 1))
                end
                table.insert(sprites, sprite)
            end
            lastDisplayedHearts = displayedHearts

            hearts -= displayedHearts
        end
    end

    return sprites
end

checker.fieldInformation =
{
    wallColor = {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true
    },
    flags = {
        fieldType = "list",
        elementSeparator = ",",
        elementOptions = {
            fieldType = "list",
        },
    },
    colors = {
        fieldType = "list",
        elementSeparator = ",",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = true,
            useAlpha = true
        },
    }
}

return checker