local drawing = require("utils.drawing")
local utils = require("utils")
local drawableLine = require("structs.drawable_line")

local codeWire = {}

codeWire.name = "PuzzleIslandHelper/CodeWire"
codeWire.nodeLimits = {1, 1}
codeWire.nodeVisibility = "never"
codeWire.fieldInformation = {
    color = {
        fieldType = "color",
        allowXNAColors = true
    }
}

function codeWire.depth(room, entity)
    return entity.above and -8500 or 2000
end

local defaultColor = {89 / 255, 88 / 255, 102 / 255}

function codeWire.sprite(room, entity)
    local color = defaultColor

    if entity.color then
        local success, r, g, b = utils.parseHexColor(entity.color)

        if success then
            color = {r, g, b}
        end
    end

    local firstNode = entity.nodes[1]

    local start = {entity.x, entity.y}
    local stop = {firstNode.x, firstNode.y}
    local control = {
        (start[1] + stop[1]) / 2,
        (start[2] + stop[2]) / 2 + 24
    }

    local points = drawing.getSimpleCurve(start, stop, control)

    return drawableLine.fromPoints(points, color, 1)
end

function codeWire.selection(room, entity)
    local main = utils.rectangle(entity.x - 2, entity.y - 2, 5, 5)
    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            nodes[i] = utils.rectangle(node.x - 2, node.y - 2, 5, 5)
        end
    end

    return main, nodes
end

codeWire.placements = {
    name = "Code Wire",
    data = {
        above = false,
        color = "595866",
        rateA = 0,
        rateB = 0,
        rateC = 0,
        rateD = 0,
        percent = 0
    }
}

return codeWire