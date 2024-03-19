local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local fakeTilesHelper = require("helpers.fake_tiles")

local shiftArea = {}
shiftArea.depth = -10001
shiftArea.name = "PuzzleIslandHelper/ShiftArea"
shiftArea.nodeLimits = {2,2}
shiftArea.lineRenderType = "line"
shiftArea.nodeVisibility = "always"
shiftArea.canResize = {false, false}
shiftArea.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
shiftArea.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
shiftArea.placements = {
    name = "Shift Area",
    data = {
        width = 8,
        height = 8,
        bgFrom = "",
        bgTo = "",
        fgFrom = "",
        fgTo = ""
    }
}
function shiftArea.fieldInformation()
    return {
        bgFrom = {
           options = fakeTilesHelper.getTilesOptions("tilesBg"),
           editable = false
        },
        bgTo = {
           options = fakeTilesHelper.getTilesOptions("tilesBg"),
           editable = false
        },
        fgFrom = {
           options = fakeTilesHelper.getTilesOptions("tilesFg"),
           editable = false
        },
        fgTo = {
            options = fakeTilesHelper.getTilesOptions("tilesFg"),
            editable = false
        }
    }
end


return shiftArea