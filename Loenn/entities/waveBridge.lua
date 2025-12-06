local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local waveBridge = {}

waveBridge.justification = { 0, 0 }

waveBridge.name = "PuzzleIslandHelper/WaveBridge"
waveBridge.minimumSize = {8,8}
waveBridge.depth = -13000
local modes = {"All","Forwards","Backwards","FromEnds","FromMiddle"}
waveBridge.placements =
{
    name = "Wave Bridge",
    data = 
    {
        tiletype = "3",
        flag = "",
        flagOnEnd = "",
        fromAbove = false,
        blendIn = true,
        width = 8,
        height = 8,
        delay = 0,
        chunkWidth = 8,
        duration = 1,
        mode = "All",
        sequenceFlag = ""
    }
}

waveBridge.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        mode = {
            options = modes,
            editable = false
        }
    }
end
waveBridge.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return waveBridge