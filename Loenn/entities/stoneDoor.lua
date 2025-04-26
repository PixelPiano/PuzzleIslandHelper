
local stoneDoor = {}
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

stoneDoor.justification = { 0, 0 }

stoneDoor.name = "PuzzleIslandHelper/StoneDoor"
stoneDoor.texture = "objects/PuzzleIslandHelper/stoneDoor/lonn"
stoneDoor.depth = 1

stoneDoor.placements =
{
    {
        name = "Stone Door",
        data = {
            base = "FF0000",
            indent = "00FF00",
            spots = "0000FF",
            spotsOutline = "FFFFFF"
        }
    }
}
stoneDoor.fieldInformation =
{
    base =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    indent =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    spots =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    spotsOutline =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}
return stoneDoor