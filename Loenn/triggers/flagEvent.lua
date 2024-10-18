local flagEvent = {}

flagEvent.name = "PuzzleIslandHelper/FlagEventTrigger"

flagEvent.placements =
{
    {
        name = "Flag Event",
        data = {
            width = 16,
            height = 16,
            requiredFlags = "!yourFlag, yourFlag2, !yourFlag3",
            event = "",
            onSpawn = false,
            flagOnStart = "",
            flagOnStartState = true
        }
    },
}
return flagEvent