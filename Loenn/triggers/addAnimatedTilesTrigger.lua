local fakeTilesHelper = require("helpers.fake_tiles")
local addAnimatedTilesTrigger = {}

addAnimatedTilesTrigger.name = "PuzzleIslandHelper/AddAnimatedTilesTrigger"
addAnimatedTilesTrigger.nodeLimits = {1, -1}
addAnimatedTilesTrigger.nodeJustification = {0,0}
addAnimatedTilesTrigger.nodeLineRenderType = "fan"
addAnimatedTilesTrigger.nodeVisibility = "always"
addAnimatedTilesTrigger.placements =
{
    {
        name = "Add Animated Tiles Trigger",
        data = {
            width = 16,
            height = 16,
            newTileType = '3',
            newBlendIn = true,
            linkVisible = true,
            linkPositions = true,
            extendX = 0,
            extendY = 0,
            offsetX = 0,
            offsetY = 0,
            addTileInterceptorIfAbsent = true
        }
    },
}
addAnimatedTilesTrigger.fieldInformation = function() 
    return {
        newTileType = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
return addAnimatedTilesTrigger