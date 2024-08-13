local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local bitRailNode = {}
local controls = {"Default","None","Full"}
bitRailNode.name = "PuzzleIslandHelper/BitrailNode"
bitRailNode.minimumSize = {8, 8}
bitRailNode.depth = -11000
bitRailNode.texture = "objects/PuzzleIslandHelper/bitRail/lonn"

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