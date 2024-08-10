local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local bitRailNode = {}

bitRailNode.name = "PuzzleIslandHelper/BitrailNode"
bitRailNode.minimumSize = {8, 8}
bitRailNode.depth = -11000
bitRailNode.texture = "objects/PuzzleIslandHelper/bitRail/lonn"

bitRailNode.placements = {
    name = "Bitrail Node",
    data = {
        color = "FFFFFF"
    }
}
bitRailNode.fieldInformation =
{
    color = 
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return bitRailNode