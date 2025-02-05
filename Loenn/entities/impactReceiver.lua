local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local impactReceiver = {}

impactReceiver.justification = { 0, 0 }

impactReceiver.name = "PuzzleIslandHelper/ImpactReceiver"
impactReceiver.minimumSize = {8,8}
impactReceiver.depth = -13000
impactReceiver.placements =
{
    name = "Impact Receiver",
    data = 
    {
        tiletype = "3",
        code = "",
        width = 8,
        height = 8
    }
}

impactReceiver.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
impactReceiver.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return impactReceiver