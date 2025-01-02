local flagEvent = {}

flagEvent.name = "PuzzleIslandHelper/FlagEventTrigger"

flagEvent.placements =
{
    {
        name = "Flag Event",
        data = {
            event = "",
            onSpawn = false,
            width = 16,
            height = 16,
            requiredFlags = "!yourFlag, yourFlag2, !yourFlag3",
            flagsOnBegin = "setThisFlagToTrue, !setThisFlagToFalse",
            flagsToInvert = "",
            oncePerLevel = true,
            oncePerSession = true,
        }
    },
}
return flagEvent