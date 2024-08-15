local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")

local bitRailNode = {}
local controls = {"Default","None","Full"}
local controlToColor =
{
    ["Default"] =  {255/255,255/255,255/255},
    ["None"] = {255 / 255, 0, 0},
    ["Full"] = {0, 255/255, 0},
}
local bouncesToColor = {
    {0, 255 / 255, 255 / 255},
    {255 / 255, 0, 0},
    {252 / 255, 0, 255 / 255},
}
bitRailNode.name = "PuzzleIslandHelper/BitrailNode"
bitRailNode.minimumSize = {8, 8}
bitRailNode.depth = -11000

bitRailNode.placements = {
    name = "Bitrail Node",
    data = {
        color = "FFFFFF",
        bounces = 0,
        isExit = false,
        groupId = "",
        control = "Default",
        timeLimit = -1
    }
}
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.groupId == target.groupId
    end
end
local function getNodeSprites(entity, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local function t(value)
        return value and 1 or 0
    end
    local hasUp = t(hasAdjacent(entity, 0, -8, rectangles))
    local hasDown = t(hasAdjacent(entity, 0, 8, rectangles))
    local hasLeft = t(hasAdjacent(entity, -8, 0, rectangles))
    local hasRight = t(hasAdjacent(entity, 8, 0, rectangles))
    local name = ""..hasUp..hasDown..hasLeft..hasRight
    
    if hasUp + hasDown + hasLeft + hasRight == 1 and entity.isExit then
        name = name .. "e"
    end


    local sprites = {}
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/bitRail/"..name, entity)
    sprite:setColor(controlToColor[entity.control])
    table.insert(sprites, sprite)
    if entity.bounces > 0 then
        local square = drawableSprite.fromTexture("objects/PuzzleIslandHelper/bitRail/smallSquare", entity)
        square:setColor(bouncesToColor[entity.bounces])
        table.insert(sprites, square)
    end

    return sprites
end

function bitRailNode.sprite(room, entity)
    local relevantNodes = utils.filter(getSearchPredicate(entity), room.entities)
    connectedEntities.appendIfMissing(relevantNodes, entity)
    local rectangles = getEntityRectangles(relevantNodes)
    return getNodeSprites(entity, rectangles)
end
function getEntityRectangles(entities)
    local rectangles = {}
    for _, entity in ipairs(entities) do
        table.insert(rectangles, utils.rectangle(entity.x, entity.y, 8,8))
    end

    return rectangles
end
bitRailNode.fieldInformation =
{
    color = 
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    control =
    {
        options = controls,
        editable = false
    }

}

return bitRailNode