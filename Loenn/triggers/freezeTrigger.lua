local freezeTrigger = {}

freezeTrigger.name = "PuzzleIslandHelper/FreezeTrigger"

local flagConditions = {"FlagActive","FlagInactive","FlagActivated","FlagDeactivated","FlagChanged"}
local playerConditions = {"None","Die","Spawn","Respawn","Dash","Jump","MoveX","MoveY","Move"}
local triggerConditions = {"None","OnEnter","OnLeave","OnStayInside","OnStayOutside"}
freezeTrigger.placements =
{
        name = "Freeze Trigger",
        data = 
        {
            flag = "",
            duration = 0,
            repeatDelay = -1,
            startAudioEvent = "event:/PianoBoy/invertGlitch2",
            endAudioEvent = "event:/PianoBoy/invertGlitch2",
            flagOnActivate = "",
            flagOnEnd = "",
            flagCondition = "FlagActivated",
            playerCondition = "None",
            triggerCondition = "None",
            loops = false,
            pauseMovementWhenFrozen = true,
            onlyOnce = false,
            onlyOncePerSession = false,
            allowFrameOne = false
        }
}
freezeTrigger.fieldOrder = 
{"x","y","width","height","startAudioEvent","endAudioEvent",
"flagCondition","triggerCondition","playerCondition",
"flag","flagOnActivate","flagOnEnd",
"duration","repeatDelay","loops","onlyOnce","onlyOncePerSession","pauseMovementWhenFrozen"}
freezeTrigger.fieldInformation = {
    flagCondition = {
        options = flagConditions,
        editable = false
    },
    playerCondition = {
        options = playerConditions,
        editable = false
    },
    triggerCondition = {
        options = triggerConditions,
        editable = false
    }
}

return freezeTrigger