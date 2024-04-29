local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local test= {}
test.justification = { 0, 0 }

test.name = "PuzzleIslandHelper/TEST"

test.depth = -8500
test.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
test.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}

test.placements =
{
    {
        name = "TEST",
        data = 
        {
            width = 8,
            height = 8,
            numA = 0,
            numB = 0,
            numC = 0,
            numD = 0,
            stringA = "",
            stringB = "",
            boolA = false,
            boolB = false,
            boolC = false,
            color = "FFFFFF"
        }
    }
}
test.fieldInformation =
{
    color = {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return test