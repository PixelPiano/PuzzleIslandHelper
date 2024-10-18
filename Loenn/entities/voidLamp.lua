local voidLamp = {}
voidLamp.justification = { 0.5, 1 }

voidLamp.name = "PuzzleIslandHelper/VoidLamp"

voidLamp.depth = -8500
voidLamp.texture = "objects/PuzzleIslandHelper/voidLamp/wip"
voidLamp.placements =
{
    {
        name = "Void Lamp",
        data = {
            flag = "",
            inverted = false,
            transitionCheck = true,
            groupID = "",
            isGroupLeader = true,
            removeIfNotClosestToSpawn = false
        }
    }
}

return voidLamp