local spotlightFadeTrigger = {}

spotlightFadeTrigger.name = "PuzzleIslandHelper/SpotlightFadeTrigger"

local modes = {"NoEffect","LeftToRight","RightToLeft","TopToBottom","BottomToTop","HorizontalCenter","VerticalCenter"}
spotlightFadeTrigger.placements =
{
    {
        name = "Spotlight Fade Trigger",
        data = {
            colorFrom = "FFFFFF",
            colorTo = "FFFFFF",
            alphaFrom = 1,
            alphaTo = 1,
            startFadeFrom = 32,
            startFadeTo = 32,
            endFadeFrom = 64,
            endFadeTo = 64,
            affectColor = true,
            positionMode = "NoEffect"
        }
    },
}
spotlightFadeTrigger.fieldOrder = {
   "x","y","width","height",
   "colorFrom", "colorTo",
   "alphaFrom","alphaTo",
   "startFadeFrom","startFadeTo",
   "endFadeFrom","endFadeTo",
   "positionMode","affectColor"
}
spotlightFadeTrigger.fieldInformation = {
    positionMode = 
    {
        options = modes,
        editable = false
    },
    colorFrom =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    colorTo =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return spotlightFadeTrigger