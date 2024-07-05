local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local lameFallingBlock = {}

lameFallingBlock.justification = { 0, 0 }

lameFallingBlock.name = "PuzzleIslandHelper/LameFallingBlock"
lameFallingBlock.minimumSize = {8,8}
lameFallingBlock.depth = -8501
lameFallingBlock.placements =
{
    name = "Lame Falling Block",
    data = 
    {
        flag = "",
        tiletype = "3",
        invertFlag = false,
        width = 8,
        height = 8,
        finalBoss = false,
        behind = false
    }
}

lameFallingBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
lameFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return lameFallingBlock