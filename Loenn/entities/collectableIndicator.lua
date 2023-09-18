local collectableIndicator = {}
collectableIndicator.justification = { 0, 0 }

collectableIndicator.name = "PuzzleIslandHelper/CollectableIndicator"

collectableIndicator.depth = -10001

collectableIndicator.texture = "objects/PuzzleIslandHelper/collectableIndicator/tI"

collectableIndicator.placements =
{
    {
        name = "Collectable Indicator",
        data = {
        miniHeart = true,
        collectables = 1,
        flags = "flag1,flag2,flag3"
        }
    }
}

return collectableIndicator