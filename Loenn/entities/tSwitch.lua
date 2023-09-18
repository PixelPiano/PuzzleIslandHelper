local tSwitch = {}
tSwitch.justification = { 0, 0 }

tSwitch.name = "PuzzleIslandHelper/TSwitch"
tSwitch.minimumSize = {24,16}
tSwitch.depth = -8500

tSwitch.texture = "objects/PuzzleIslandHelper/tswitch/block00"

tSwitch.placements =
{
    {
        name = "T Switch",
        data = {
        tileData = "MMM,0M0",
        randomSeed = 0,
        blendEdges = true,
        }
    }
}
tSwitch.ignoredFields = { "width","height","tileData","randomSeed","blendEdges","id","name"}

return tSwitch