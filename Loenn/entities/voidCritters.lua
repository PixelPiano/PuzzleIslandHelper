local voidCritters = {}
voidCritters.justification = { 0, 0 }

voidCritters.name = "PuzzleIslandHelper/VoidCritters"

voidCritters.depth = -8500

voidCritters.texture = "objects/PuzzleIslandHelper/voidBugs/lonn"

voidCritters.placements =
{
    {
        name = "Void Critters",
        data = {
            time = 2,
            cutsceneOnDeactivate = false,
            usesFlag = false,
            flag = "void_critters"
        }
    }
}

return voidCritters