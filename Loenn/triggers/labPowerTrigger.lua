local labPowerTrigger = {}

labPowerTrigger.name = "PuzzleIslandHelper/LabPowerTrigger"

local modes = {"OnEnter","OnStay","OnLeave","OnLevelStart","OnUpdate","OnRemoved"}
local states = {"Backup","Barely","Restored"}
labPowerTrigger.placements =
{
    {
        name = "Lab Power Trigger",
        data = {
            flag = "",
            mode = "OnEnter",
            state = "Backup"
        }
    },
}
labPowerTrigger.fieldInformation = 
{
    mode = 
    {
        options = modes,
        editable = false
    },
    state = 
    {
        options = states,
        editable = false
    }
}

return labPowerTrigger