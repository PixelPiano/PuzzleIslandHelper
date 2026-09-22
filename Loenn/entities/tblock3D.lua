local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local tBlock3D = {}

tBlock3D.justification = { 0, 0 }

tBlock3D.name = "PuzzleIslandHelper/TBlock3D"
tBlock3D.minimumSize = {8,8}
tBlock3D.canResize = {false,false}
tBlock3D.depth = -8501
tBlock3D.placements =
{
    name = "T Block 3D",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8,
        tilesets = ""
    }
}

tBlock3D.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
tBlock3D.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return tBlock3D