local eyeLock= {}
eyeLock.justification = { 0, 0 }
eyeLock.name = "PuzzleIslandHelper/EyeLock"
eyeLock.depth = 3
eyeLock.texture = "objects/PuzzleIslandHelper/lock/sprite"
local modes = {"Key","Flag"}
eyeLock.placements =
{
    {
        name = "Eye Lock",
        data = 
        {
            lockID = "",
            requiredFlags = "",
            flagsToSet = "",
            flag = "",
            mode = "Key"
        }
    }
}
eyeLock.fieldInformation =
{
    mode =
    {
        options = modes,
        editable = false
    }
}

return eyeLock