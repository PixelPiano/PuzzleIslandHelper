local eyeLock= {}
eyeLock.justification = { 0, 0 }
eyeLock.name = "PuzzleIslandHelper/EyeLock"
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
            mode = "Key",
            depth = 3,
            flagDelay = 0.7
        }
    }
}
function eyeLock.depth(room, entity)
    return entity.depth or 3
end
eyeLock.fieldInformation =
{
    mode =
    {
        options = modes,
        editable = false
    }
}

return eyeLock