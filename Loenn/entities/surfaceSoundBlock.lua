local xnaColors = require("consts.xna_colors")
local gray = xnaColors.Gray

local surfaceSoundBlock= {}

surfaceSoundBlock.name = "PuzzleIslandHelper/SurfaceSoundBlock"

surfaceSoundBlock.canResize = {true,true}
surfaceSoundBlock.minimumSize = {8,8}
surfaceSoundBlock.depth = -8500
surfaceSoundBlock.fillColor = {gray[1] * 0.3, gray[2] * 0.3, gray[3] * 0.3, 0.6}
surfaceSoundBlock.borderColor = {gray[1] * 0.8, gray[2] * 0.8, gray[3] * 0.8, 0.8}

surfaceSoundBlock.placements =
{
    {
        name = "Invisible Surface Sound Block",
        data = 
        {
            width = 8,
            height = 8,
            surfaceSound = 0
        }
    }
}
surfaceSoundBlock.fieldInformation = {
    surfaceSound = {
        fieldType = "integer",
    }
}
return surfaceSoundBlock