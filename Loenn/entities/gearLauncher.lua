local gearLauncher = {}
gearLauncher.name = "PuzzleIslandHelper/GearLauncher"
gearLauncher.texture = "objects/PuzzleIslandHelper/gear/holderStart"
gearLauncher.depth = 0
local dir = {"Up","Down","Left","Right","UpLeft","UpRight","DownLeft","DownRight"}
gearLauncher.placements = {
    name = "Gear Launcher",
    data = {
        onlyOnce = false,
        force = 10,
        direction = "Right",
        chargeTime = 1
    }
}
gearLauncher.fieldInformation =
{
    direction =
    {
        options = dir,
        editable = false,
    }
}

return gearLauncher