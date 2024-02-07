local cogLauncher = {}
cogLauncher.name = "PuzzleIslandHelper/CogLauncher"
cogLauncher.texture = "objects/PuzzleIslandHelper/cog/holderStart"
cogLauncher.depth = 0
local dir = {"Up","Down","Left","Right","UpLeft","UpRight","DownLeft","DownRight"}
cogLauncher.placements = {
    name = "Cog Launcher",
    data = {
        onlyOnce = false,
        force = 10,
        direction = "Right",
        chargeTime = 1
    }
}
cogLauncher.fieldInformation =
{
    direction =
    {
        options = dir,
        editable = false,
    }
}

return cogLauncher