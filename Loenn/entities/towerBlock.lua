local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local towerBlock = {}

towerBlock.justification = { 0, 0 }

towerBlock.name = "PuzzleIslandHelper/TowerBlock"
towerBlock.minimumSize = {8,8}
towerBlock.depth = -8501
towerBlock.placements =
{
    name = "Tower Block",
    data = 
    {
        tiletype = "3",
        inside = true,
        width = 8,
        height = 8
    }
}

towerBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        }
    }
end
towerBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return towerBlock