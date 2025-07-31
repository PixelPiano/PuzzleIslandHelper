local capsuleIDTrigger = {}

capsuleIDTrigger.name = "PuzzleIslandHelper/CapsuleIDTrigger"

capsuleIDTrigger.nodeLimits = {1, -1}
capsuleIDTrigger.nodeLineRenderType = "fan"
capsuleIDTrigger.placements =
{
    {
        name = "Capsule ID Trigger",
        data = {
            width = 16,
            height = 16,
            flags = "",
            warpID = "",
            alwaysActive = false
        }
    },
}

return capsuleIDTrigger