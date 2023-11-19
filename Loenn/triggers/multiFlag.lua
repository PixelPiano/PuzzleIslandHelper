local multiFlag = {}

multiFlag.name = "PuzzleIslandHelper/MultiFlag"

local modes = {"OnEnter","OnLeave","OnStay","OnLevelStart"}
multiFlag.placements =
{
    {
        name = "Multiple Flag Trigger",
        data = {
            turnOn = "flag1,flag2",
            turnOff = "flag3,flag4",
            flag = "canChangeFlags",
            mode = "OnEnter"
        }
    },
}
multiFlag.fieldInformation = 
{
    mode = 
    {
        options = modes,
        editable = false
    }
}

return multiFlag