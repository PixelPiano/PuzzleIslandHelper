local flagIntervalSwitch = {}

flagIntervalSwitch.name = "PuzzleIslandHelper/FlagIntervalSwitch"

flagIntervalSwitch.placements =
{
    {
        name = "Flag Interval Switch",
        data = {
            flag = "",
            intervalFlags = "flag1,flag2,flag3,flag4",
            interval = 0.3,
            endWaitTime = 0.5,
            repeatOnEnd = false,
            invertOnRepeat = false,
            intervalFlagState = true,
            oneAtATime = true,
            activationMethod = "OnLevelStart"
        }
    },
}
local types = {"OnLevelStart","OnEnter","OnLeave"}
flagIntervalSwitch.fieldInformation =
{
    activationMethod =
    {
        options = types,
        editable = false
    }
}

return flagIntervalSwitch