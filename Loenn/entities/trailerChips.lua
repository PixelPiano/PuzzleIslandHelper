local wipEntity = {}
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")
wipEntity.justification = { 0, 0 }

wipEntity.name = "PuzzleIslandHelper/TrailerChips"
wipEntity.texture = "objects/PuzzleIslandHelper/wip/texture"
wipEntity.depth = 1

wipEntity.placements =
{
    {
        name = "Trailer Chips",
        data = {
            phrases = "ChipPhrases"
        }
    }
}


return wipEntity