local waterfallHelper = require("helpers.waterfalls")
local customWaterfall = {}

customWaterfall.justification = { 0, 0 }
customWaterfall.name = "PuzzleIslandHelper/CustomWaterfall"
customWaterfall.canResize = {false, false}
customWaterfall.depth = 1
customWaterfall.depth = -9999
customWaterfall.placements =
{
    name = "Custom Waterfall",
    data = 
    {
       displacementFlag = "",
       invertDisplacementFlag = false,
       audioFlag = "",
       invertAudioFlag = false,
       renderFlag = "",
       invertRenderFlag = false,
       goesThroughSolids = false
    }
}

function customWaterfall.sprite(room, entity)
    return waterfallHelper.getWaterfallSprites(room, entity)
end
customWaterfall.rectangle = waterfallHelper.getWaterfallRectangle
return customWaterfall