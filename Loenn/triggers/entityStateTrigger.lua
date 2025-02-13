local entityStateTrigger = {}

entityStateTrigger.name = "PuzzleIslandHelper/EntityStateTrigger"
entityStateTrigger.nodeLimits = {0, 1}
local entities = {"DashBlock","ExitBlock","Bumper","Refill","Feather","Booster"}
local flagModes = {"OnChange","EveryFrame","OnlyOnce"}

entityStateTrigger.placements =
{
    {
        name = "Entity State Trigger",
        data = {
            flag = "",
            inverted = false,
            flagMode = "OnChange",
            target = "DashBlock",
            tiedToTarget = false
        }
    },
}
entityStateTrigger.fieldInformation = {
    flagMode = {
        options = flagModes,
        editable = false
    },
    target = {
        options = entities,
        editable = false
    }
}

return entityStateTrigger