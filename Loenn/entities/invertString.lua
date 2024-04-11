local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")

local invertString = {}

invertString.name = "PuzzleIslandHelper/InvertString"
invertString.nodeLimits = {1, -1}
invertString.depth = -1
invertString.nodeVisibility = "never"

local function spritesFromMiddle(middle, target, color)
    local coords = {target.x, target.y}
    local control = {
        (middle[1] + coords[1]) / 2,
        (middle[2] + coords[2]) / 2 + 4
    }
    local points = drawing.getSimpleCurve(coords, middle, control)

    return drawableLine.fromPoints(points, color, 1)
end

local defaultColor = {255 / 255, 255 / 255, 255 / 255}

function invertString.sprite(room, entity)
    local color = defaultColor


    local nodes = entity.nodes or {}
    local firstNode = nodes[1] or entity

    local start = {entity.x, entity.y}
    local stop = {firstNode.x, firstNode.y}
    local control = {
        (start[1] + stop[1]) / 2,
        (start[2] + stop[2]) / 2 + 4
    }
    local middle = {drawing.getCurvePoint(start, stop, control, 0.5)}
    local sprites = {}

    for _, node in ipairs(nodes) do
        table.insert(sprites, spritesFromMiddle(middle, node, color))
    end

    -- Use entity rather than start since drawFromMiddle uses x and y
    table.insert(sprites, spritesFromMiddle(middle, entity, color))

    return table.flatten(sprites)
end

function invertString.selection(room, entity)
    local main = utils.rectangle(entity.x - 2, entity.y - 2, 5, 5)
    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            nodes[i] = utils.rectangle(node.x - 2, node.y - 2, 5, 5)
        end
    end

    return main, nodes
end

invertString.placements = {
    name = "Invert String",
    placementType = "line",
    data = {
    }
}

return invertString