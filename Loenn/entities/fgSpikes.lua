local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local entities = require("entities")
local spikeHelper = require("helpers.spikes")
local variants = {
    "default",
    "outline",
    "cliffside",
    "reflection",
}
local directionNames = {
        up = "PuzzleIslandHelper/FGSpikesUp",
        down =  "PuzzleIslandHelper/FGSpikesDown",
        left =  "PuzzleIslandHelper/FGSpikesLeft",
        right =  "PuzzleIslandHelper/FGSpikesRight",
 }

function createEntityHandler(name, direction, variants)

    variants = variants or spikeHelper.spikeVariants
    local handler = {}

    handler.name = name
    local handlerDirectionNames = directionNames
    handler.placements = getSpikePlacements(direction, variants)
    
    handler.canResize = spikeHelper.getCanResize(direction)
    handler.fieldInformation = getFieldInformations(variants)

    function handler.sprite(room, entity)
        return getSpikeSprites(entity, direction, entity.type or "default")
    end

    function handler.selection(room, entity)
        local sprites =  getSpikeSprites(entity, direction, entity.type or "default")
        return entities.getDrawableRectangle(sprites)
    end
    if handlerDirectionNames then
        function handler.flip(room, entity, horizontal, vertical)
            return flip(entity, direction, handlerDirectionNames, horizontal, vertical)
        end

        function handler.rotate(room, entity, rotationDirection)
            return rotate(entity, direction, handlerDirectionNames, rotationDirection)
        end
    end
    return handler
end
function getFieldInformations(variants)
    return {
        type = {
            options = variants
        }
    }
end
function rotate(entity, direction, handlerDirectionNames, rotationDirection)
    local sideIndexLookup = {
        "up",
        "right",
        "down",
        "left",

        up = 1,
        right = 2,
        down = 3,
        left = 4
    }
    local sideIndex = sideIndexLookup[direction]
    local targetIndex = utils.mod1(sideIndex + rotationDirection, 4)

    if sideIndex ~= targetIndex then
        local targetDirection = sideIndexLookup[targetIndex]
        local newHandlerName = directionNames[targetDirection]

        entity._name = newHandlerName

        -- Swap width and height if rotation goes from horizontal <-> vertical
        if sideIndex % 2 ~= targetIndex % 2 then
            entity.width, entity.height = entity.height, entity.width
        end
    end

    return sideIndex ~= targetIndex
end

function flip(entity, direction, handlerDirectionNames, horizontal, vertical)
    local result = false

    if vertical and (direction == "up" or direction == "down") then
        result = rotate(entity, direction, handlerDirectionNames, 2)
    end

    if horizontal and (direction == "left" or direction == "right") then
        result = rotate(entity, direction, handlerDirectionNames, 2)
    end

    return result
end
function getSpikePlacements(direction, variants)
    local placements = {}
    local horizontal = direction == "left" or direction == "right"
    local lengthKey = horizontal and "height" or "width"
 for i, variant in ipairs(variants) do
        placements[i] = {
            name = string.format("FG Spikes (%s)", firstToUpper(direction)..", "..firstToUpper(variant)),
            data = {
                type = variant
            }
        }
        placements[i].data[lengthKey] = 8
    end
    return placements
end
function firstToUpper(str)
    return (str:gsub("^%l", string.upper))
end
local fgSpikeUp = createEntityHandler("PuzzleIslandHelper/FGSpikesUp", "up", variants)
local fgSpikeDown = createEntityHandler("PuzzleIslandHelper/FGSpikesDown", "down", variants)
local fgSpikeLeft = createEntityHandler("PuzzleIslandHelper/FGSpikesLeft", "left", variants)
local fgSpikeRight = createEntityHandler("PuzzleIslandHelper/FGSpikesRight", "right", variants)
local spikeDepth = -10001
local spikeTexture = "danger/spikes/%s_%s00"



local spikeOffsets = {
    up = {4, 1},
    down = {4, -1},
    right = {-1, 4},
    left = {1, 4}
}

local spikeJustifications = {
    up = {0.5, 1.0},
    down = {0.5, 0.0},
    right = {0.0, 0.5},
    left = {1.0, 0.5}
}

local tentacleRotations = {
    up = 0,
    down = math.pi,
    right = math.pi / 2,
    left = math.pi * 3 / 2
}
local function getOffset(direction, variant)
    if variant == "tentacles" then
        return 0, 0
    else
        return unpack(spikeOffsets[direction] or {0, 0})
    end
end


local function getJustification(direction, variant)
    if variant == "tentacles" then
        if direction == "up" or direction == "right" then
            return 0.0, 0.5
        elseif direction == "down" or direction == "left" then
            return 1.0, 0.5
        end
    else
        return unpack(spikeJustifications[direction] or {0, 0})
    end
end

local function getRotation(direction, variant)
    if variant == "tentacles" then
        return tentacleRotations[direction] or 0

    else
        return 0
    end
end

local function getSpikeSpritesFromTexture(entity, direction, variant, texture, step)
    step = step or 8

    local horizontal = direction == "left" or direction == "right"
    local justificationX, justificationY = getJustification(direction, variant)
    local offsetX, offsetY = getOffset(direction, variant)
    local rotation = getRotation(direction, variant)
    local length = horizontal and (entity.height or step) or (entity.width or step)
    local positionOffsetKey = horizontal and "y" or "x"

    local position = {
        x = entity.x,
        y = entity.y
    }

    local sprites = {}

    for i = 0, length - 1, step do
        -- Tentacles overlap instead of "overflowing"
        if i == length - step / 2 then
            position[positionOffsetKey] -= step / 2
        end

        local sprite = drawableSprite.fromTexture(texture, position)

        sprite.depth = spikeDepth
        sprite.rotation = rotation
        sprite:setJustification(justificationX, justificationY)
        sprite:addPosition(offsetX, offsetY)

        table.insert(sprites, sprite)

        position[positionOffsetKey] += step
    end

    return sprites
end

-- Spikes with side images
local function getNormalSpikeSprites(entity, direction, variant, step)
    local texture = string.format(spikeTexture, variant, direction)

    return getSpikeSpritesFromTexture(entity, direction, variant, texture, step or 8)
end

-- Spikes with rotated sprites
local function getTentacleSprites(entity, direction, variant, step)
    local texture = "danger/tentacles00"

    return getSpikeSpritesFromTexture(entity, direction, variant, nil, texture, step or 16)
end

--getTentacleSprites, getNormalSpikeSprites
function getSpikeSprites(entity, direction, variant)
    -- Use first key if table
    if variant == "tentacles" then
        return getTentacleSprites(entity, direction, variant, 16)

    else
        return getNormalSpikeSprites(entity, direction, variant, 8)
    end
end

local function setPlacementAttributeIfMissing(data, attribute, value)
    if type(attribute) == "table" then
        for _, attr in ipairs(attribute) do
            if not data[attr] then
                data[attr] = value
            end
        end

    else
        if not data[attribute] then
            data[attribute] = value
        end
    end
end

-- Check if all variant keys have default data
local function hasDataForVariantKey(data, variantKey)
    if type(variantKey) == "table" then
        for _, attr in ipairs(variantKey) do
            if not data[attr] then
                return false
            end
        end

    else
        if not data[variantKey] then
            return false
        end
    end

    return true
end


function getCanResize(direction)
    if direction == "left" or direction == "right" then
        return {false, true}
    end

    return {true, false}
end
return {
    fgSpikeUp,
    fgSpikeDown,
    fgSpikeLeft,
    fgSpikeRight
}
