local fakeTilesHelper = require("helpers.fake_tiles")
local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")
local utils = require("utils")
local tileSetPuzzle = {}

tileSetPuzzle.justification = { 0, 0 }
tileSetPuzzle.associatedMods = {"FancyTileEntitiesLoenn"}
tileSetPuzzle.name = "PuzzleIslandHelper/TileSetPuzzle"
tileSetPuzzle.minimumSize = {8,8}
tileSetPuzzle.depth = -8501
tileSetPuzzle.placements =
{
    name = "Tile Set Puzzle",
    data = 
    {
        width = 8,
        height = 8,
        tiledata = "3",
    }
}

tileSetPuzzle.fieldInformation = function() 
    return {
        tiledata = {
            fieldType = "FancyTileEntities.buttonStringField"
        },
    }
end
tileSetPuzzle.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return tileSetPuzzle