
local warpCapsule = {}

warpCapsule.justification = { 0, 0 }
warpCapsule.name = "PuzzleIslandHelper/WarpCapsule"

warpCapsule.depth = 1
warpCapsule.nodeLimits = {1, 1}
warpCapsule.nodeJustification = {0,0}
warpCapsule.nodeTexture = "objects/PuzzleIslandHelper/digiWarpReceiver/terminal"
warpCapsule.nodeVisibility = "always"
warpCapsule.placements =
{
    {
        name = "Warp Capsule",
        data = {
            warpID = "",
            warpDelay = 0,
            disableMachineFlag = "",
            invertMachineFlag = false,
            disableFlag = "",
            invertFlag = false,
            rune = "",
            isDefaultRune = false,
            isPartOfFirstSet = false
        }
    }
}
warpCapsule.texture = "objects/PuzzleIslandHelper/digiWarpReceiver/lonn"
return warpCapsule