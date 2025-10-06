local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local libraryMazeSwitchBlock = {}

libraryMazeSwitchBlock.justification = { 0, 0 }

libraryMazeSwitchBlock.name = "PuzzleIslandHelper/LibraryMazeSwitchBlock"
libraryMazeSwitchBlock.minimumSize = {16,16}
libraryMazeSwitchBlock.depth = -8501
libraryMazeSwitchBlock.placements =
{
    name = "Library Maze Switch Block",
    data = 
    {
        tiletype = "9",
        requiredFlags = "",
        flagsToSet = "",
        width = 8,
        height = 8
    }
}

libraryMazeSwitchBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
libraryMazeSwitchBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return libraryMazeSwitchBlock