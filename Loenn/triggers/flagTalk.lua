local flagEvent = {}

flagEvent.name = "PuzzleIslandHelper/FlagTalk"

flagEvent.placements =
{
    {
        name = "Flag Talk",
        data = {
            width = 16,
            height = 16,
            requiredFlags = "!yourFlag, yourFlag2, !yourFlag3",
            flagsOnBegin = "setThisFlagToTrue, !setThisFlagToFalse",
            flagsToInvert = "",
            talkBuffer = 0.7,
            oncePerLevel = false,
            oncePerSession = false
        }
    },
}
return flagEvent