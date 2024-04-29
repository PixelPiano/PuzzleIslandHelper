local utils = require("utils")
local drawableSpriteStruct = require("structs.drawable_sprite")
local jautils = require("mods").requireFromPlugin("libraries.jautils")
local fallback = "danger/PuzzleIslandHelper/icecrystal/fg03"
local fallbackbg = "danger/PuzzleIslandHelper/icecrystal/bg"
local spinner = {}

local groups = {
    [1] = "e478ff",
    [2] = "e542ff",
    [3] = "b03eff",
    [4] = "de0adc",
}
spinner.name = "PuzzleIslandHelper/SoundSpinner"
spinner.depth = -8500
spinner.placements = {}
for i = 1, 4 do
    local placement = {
        name = "Sound Spinner (Group "..i..")",
        data = 
        {
            freq1 = -1,
            freq2 = -1,
            freq3 = -1,
            freq4 = -1,
            tint = groups[i],
            destroyColor = tint,
            borderColor = "000000",
            directory = "danger/PuzzleIslandHelper/icecrystal",
            spritPathSuffix = "",
            attachToSolid = false,
            moveWithWind = false,
            dashThrough = false,
            rainbow = false,
            collidable = true,
            drawOutline = true,
            bloomAlpha = 1,
            bloomRadius = 8,
            debrisCount = 8,
            attachGroup = i,
            singleFGImage = false
        }
    }
    table.insert(spinner.placements, placement)
end
spinner.fieldInformation = 
{
    tint =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    destroyColor =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    borderColor =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}
function spinner.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "moveWithWind",
        "dashThrough",
        "collidable",
        "bloomAlpha",
        "bloomRadius",
        "destroyColor",
        "tint",
        "destroyColor",
        "borderColor",
        "directory",
        "spritPathSuffix",
        "attachToSolid",
        "moveWithWind",
        "dashThrough",
        "rainbow",
        "collidable",
        "drawOutline",
        "bloomAlpha",
        "bloomRadius",
        "debrisCount",
        "attachGroup",
        "singleFGImage",
        "freq1",
        "freq2",
        "freq3",
        "freq4"
    }
     local function doNotIgnore(value)
        for i = #ignored, 1, -1 do
            if ignored[i] == value then
                table.remove(ignored, i)
                return
            end
        end
    end

    local group = entity.attachGroup or 1
    local iscomparison = false

    for i = 1, 4 do
        if group >= i then
            doNotIgnore("freq"..i)
        end
    end

    return ignored
end
function distanceSquared(x1, y1, x2, y2)
    local diffX, diffY = x2 - x1, y2 - y1
    return diffX * diffX + diffY * diffY
end

local function createConnectorsForSpinner(room, entity, baseBGSprite)
    local sprites = {}

    local name = entity._name
    local attachGroup = entity.attachGroup
    local attachToSolid = entity.attachToSolid or false
    local x, y = entity.x, entity.y

    if RYSY and RYSY.entitiesWithSIDWithinRangeUntilThis then
        for _, e2 in RYSY.entitiesWithSIDWithinRangeUntilThis(room, name, entity, 24) do
            if e2.attachGroup == attachGroup and e2.attachToSolid == attachToSolid then
                local e2x, e2y = e2.x, e2.y

                local connector = copyTexture(baseBGSprite,
                    math.floor((x + e2x) / 2),
                    math.floor((y + e2y) / 2),
                false)

                connector.depth = -8499
                table.insert(sprites, connector)
            end
        end

        return sprites
    end

    for _, e2 in ipairs(room.entities) do
        if e2 == entity then break end

        if e2._name == name and e2.attachGroup == attachGroup and e2.attachToSolid == attachToSolid then
            local e2x, e2y = e2.x, e2.y

            if distanceSquared(x, y, e2x, e2y) < 576 then
                local connector = copyTexture(baseBGSprite,
                    math.floor((x + e2x) / 2),
                    math.floor((y + e2y) / 2),
                false)

                connector.depth = -8499
                table.insert(sprites, connector)
            end
        end
    end

    return sprites
end
function copyTexture(baseTexture, x, y, relative)
    local texture = drawableSpriteStruct.fromMeta(baseTexture.meta, baseTexture)
    texture.x = relative and baseTexture.x + x or x
    texture.y = relative and baseTexture.y + y or y

    return texture
end
local pathCache = {}

function spinner.sprite(room, entity)
    local pathSuffix = entity.spritePathSuffix or ""
    local color = utils.getColor(entity.tint or "ffffff")
    local baseSprite = drawableSpriteStruct.fromTexture(fallbackbg, entity)
    baseSprite:setColor(color)
    local sprites = createConnectorsForSpinner(room, entity, baseSprite)

    local fgSprite = drawableSpriteStruct.fromTexture(fallback, entity)
    fgSprite:setColor(color)
    table.insert(sprites, fgSprite)

    return sprites
end

function spinner.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

return spinner