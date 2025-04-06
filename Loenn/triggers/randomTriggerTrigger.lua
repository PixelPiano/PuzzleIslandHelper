local randomTriggerTrigger = {}

randomTriggerTrigger.name = "PuzzleIslandHelper/RandomTriggerTrigger"

local modes = {"OnEnter","OnLeave","OnStay"}
local onceModes = {"None","PerRoom","PerSession"}
randomTriggerTrigger.nodeLimits = {0, -1}
randomTriggerTrigger.nodeLineRenderType = "fan"
randomTriggerTrigger.placements =
{
    {
        name = "Random Trigger Trigger",
        data = {
            flag = "",
            inverted = false,
            ignoreFlag = false,
            activateMode = "OnEnter",
            onTransition = false,
            onceMode = "None",
            transitionUpdate = false
        }
    },
}
randomTriggerTrigger.fieldInformation = 
{
    activateMode = 
    {
        options = modes,
        editable = false
    },
    onceMode = 
    {
        options = onceModes,
        editable = false
    }
}

return randomTriggerTrigger