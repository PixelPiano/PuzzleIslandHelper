local osc= {}
osc.justification = { 0, 0 }

osc.name = "PuzzleIslandHelper/Osc"

osc.depth = -100000

osc.texture = "objects/PuzzleIslandHelper/access/artifactHolder00"

local windowTypes = {"Rect","Triangle","Hamming","Hanning","Blackman","Blackmanharris"}
local drawTypes = {"Surface","Full","Dots","Bars"}
local windowSizes = {128,256,512,1024,2048,4096,8192}
osc.placements =
{
    {
        name = "Osc",
        data = 
        {
            width = 40,
            height = 16,
            scaleX = 1,
            scaleY = 1,
            flag = "",
            event = "event:/...",
            dangerous = false,
            twoSided = false,
            thickness = 1,
            baseColor = "FFFFFF",
            peakColor = "FF0000",
            depth = -100000,
            fftWindowSize = 2048,
            fftWindowType = "Triangle",
            drawType = "Surface"
        }
    }
}
osc.fieldInformation =
{
    fftWindowType =
    {
        options = windowTypes,
        editable = false
    },
    fftWindowSize =
    {
        options = windowSizes,
        editable = false
    },
    drawType =
    {
        options = drawTypes,
        editable = false
    },
    thickness =
    {
        fieldType = "integer",
        minimumValue = 1
    },
    depth = 
    {
        fieldType = "integer",
    },
    baseColor =
    {
        fieldType = "color",
        allowXNAColors = true
    },
    peakColor =
    {
        fieldType = "color",
        allowXNAColors = true
    }
}

return osc