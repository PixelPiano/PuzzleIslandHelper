local calidusUpgradeTrigger = {}

calidusUpgradeTrigger.name = "PuzzleIslandHelper/CalidusUpgradeTrigger"

local upgrades = {"Nothing","Grounded","Slowed","Weakened","Eye","Head","Arms","Blip"}
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