local entityStateTrigger = {}

entityStateTrigger.name = "PuzzleIslandHelper/EntityStateTrigger"
entityStateTrigger.nodeLimits = {0, 1}
local entities = {"DashBlock","ExitBlock","Bumper","Refill","Feather","Booster"}

entityStateTrigger.placements =
{
    {
        name = "Entity State Trigger",
        data = {
            flag = "",
            inverted = false,
            target = "DashBlock",
            tiedToTarget = false,
            startState = false,
            persistent = false,
            setFlagOnTransition = false,
            invertFlagOnContact = false,
            onlyOnContact = false
        }
    },
}
entityStateTrigger.fieldInformation = {
    target = {
        options = entities,
        editable = false
    }
}

return entityStateTrigger