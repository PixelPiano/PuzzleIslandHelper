local flagArea = {}

flagArea.name = "PuzzleIslandHelper/FlagArea"

flagArea.placements =
{
    {
        name = "Flag Area",
        data = {
            width = 16,
            height = 16,
            activeFlag = "",
            flag = "",
            inverted = false,
            activeFlagInverted = false,
            checkEveryFrame = false
        }
    },
}
return flagArea