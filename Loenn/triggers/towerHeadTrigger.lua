local flagArea = {}

flagArea.name = "PuzzleIslandHelper/TowerHeadTrigger"
flagArea.nodeLimits = {1, 1}
flagArea.placements =
{
    {
        name = "Tower Head Trigger",
        data = {
            width = 16,
            height = 16,
            isTalk = false,
            setState = true
        }
    },
}
return flagArea