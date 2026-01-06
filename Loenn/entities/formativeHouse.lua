local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local passengerHouse = {}
passengerHouse.minimumSize = {16,16}
passengerHouse.name = "PuzzleIslandHelper/FormativeHouse"
passengerHouse.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
passengerHouse.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
local facings = {"Left","Right"}
passengerHouse.placements = {
    name = "Formative House",
    data = {
        width = 16,
        height = 16,
        facing = "Right"
    }
}
passengerHouse.fieldInformation =
{
    facing =
    {
        options = facings,
        editable = false
    }
}
passengerHouse.depth = 6

return passengerHouse