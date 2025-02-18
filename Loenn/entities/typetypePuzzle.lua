local wipEntity = {}
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")
wipEntity.justification = { 0, 0 }
wipEntity.nodeJustification = {0,0}
wipEntity.nodeLimits = {0,-1}
wipEntity.nodeLineRenderType = "fan"
wipEntity.nodeVisibility = "always"
wipEntity.name = "PuzzleIslandHelper/TiletypePuzzle"
wipEntity.texture = "objects/PuzzleIslandHelper/wip/texture"
wipEntity.depth = -10000

wipEntity.placements =
{
    {
        name = "Tiletype Puzzle",
        data = {
            flagOnComplete = "",
            nodeFlagPrefix = "",
            tiletypes = "",
            solution = "",
            nodeWidth = 32,
            nodeHeight = 32
        }
    }
}

return wipEntity