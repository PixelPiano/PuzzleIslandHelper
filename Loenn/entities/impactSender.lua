local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local impactsender = {}

impactsender.justification = { 0, 0 }

impactsender.name = "PuzzleIslandHelper/ImpactSender"
impactsender.minimumSize = {8,8}
impactsender.depth = -13000
impactsender.nodeLimits = {1,1}
impactsender.lineRenderType = "line"
impactsender.nodeVisibility = "always"
impactsender.nodeTexture = "objects/PuzzleIslandHelper/hitmarker"
impactsender.placements =
{
    name = "Impact Sender",
    data = 
    {
        tiletype = "3",
        key = 'c',
        blendin = true,
        pulseColor = "00000000",
        pulseDuration = 0.8,
        shakes = false,
        width = 8,
        height = 8
    }
}

impactsender.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        pulseColor =
        {
            fieldType = "color",
            useAlpha = true,
            allowXNAColors = true,
        },
    }
end
impactsender.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return impactsender