
local digiWarpReceiver = {}

digiWarpReceiver.justification = { 0, 0 }
digiWarpReceiver.name = "PuzzleIslandHelper/DigiWarpReceiver"

digiWarpReceiver.depth = 1
digiWarpReceiver.nodeLimits = {1, 1}
digiWarpReceiver.nodeJustification = {0,0}
digiWarpReceiver.nodeTexture = "objects/PuzzleIslandHelper/digiWarpReceiver/terminal"
digiWarpReceiver.nodeVisibility = "always"
digiWarpReceiver.placements =
{
    {
        name = "Digi Warp Receiver",
        data = {
            warpID = "",
            warpDelay = 0,
            password = "",
            isPrimary = false,
            disableMachineFlag = "",
            invertMachineFlag = false,
            disableFlag = "",
            invertFlag = false
        }
    }
}
digiWarpReceiver.texture = "objects/PuzzleIslandHelper/digiWarpReceiver/lonn"
return digiWarpReceiver