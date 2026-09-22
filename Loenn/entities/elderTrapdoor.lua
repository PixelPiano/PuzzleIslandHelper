local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local elderTrapdoor = {}
elderTrapdoor.name = "PuzzleIslandHelper/ElderTrapdoor"

elderTrapdoor.depth = -100000
elderTrapdoor.minimumSize = {8,8}

elderTrapdoor.placements =
{
    {
        name = "Elder Trapdoor",
        data = {
            tiletype = '',
            tilesFadeFlag = "",
            completedFlag = "",
            width = 8,
            height = 8,
            trapdoorX = 8,
            trapdoorWidth = 32,

        }
    }
}
elderTrapdoor.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
elderTrapdoor.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)
return elderTrapdoor