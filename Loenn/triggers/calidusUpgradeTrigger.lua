local calidusUpgradeTrigger = {}

calidusUpgradeTrigger.name = "PuzzleIslandHelper/CalidusUpgradeTrigger"

local upgrades = {"Nothing","NothingGrounded","Eye","Head","LeftArm","RightArm","Blip"}
calidusUpgradeTrigger.placements =
{
    {
        name = "Calidus Upgrade Trigger",
        data = {
            upgrade = "Nothing",
            oncePerSession = false,
            skipCutscene = false
        }
    },
}
calidusUpgradeTrigger.fieldInformation = {
    upgrade = {
        options = upgrades,
        editable = false
    }
}

return calidusUpgradeTrigger