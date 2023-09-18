local stoolPickupBarrier= {}
stoolPickupBarrier.justification = { 0, 0 }
stoolPickupBarrier.name = "PuzzleIslandHelper/StoolPickupBarrier"

stoolPickupBarrier.depth = -8500
stoolPickupBarrier.fillColor = {0.4, 0.4, 1.0, 0.4}
stoolPickupBarrier.borderColor = {0.4, 0.4, 1.0, 1.0}
stoolPickupBarrier.placements =
{
    {
        name = "Stool Pickup Barrier",
        data =
        {
            lineThickness = 1,
            affectIcons = false,
            width = 16,
            height = 16,
            moveObjectsToEdge = false
        }
    }

}
return stoolPickupBarrier