local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local water = {}

water.name = "PuzzleIslandHelper/ChargedWater"

water.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
water.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
local bubbles = {"Straight","FullControl","FloatDown"}
water.placements = {
    name = "charged water",
    data = {
        width = 8,
        height = 8,
        bubbleType = "Straight",
        bubbleUp = true,
        bubbleDown = false,
        bubbleRight = false,
        bubbleLeft = false,
        flag = "",
        usedInCutscene = false
    }
}
water.fieldInformation = 
{
    bubbleType = {
        options = bubbles,
        editable = false
    }
}
water.depth = 0

return water