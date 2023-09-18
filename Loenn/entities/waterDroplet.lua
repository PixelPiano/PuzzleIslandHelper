local waterDroplet = {}
waterDroplet.justification = { 0, 0 }

waterDroplet.name = "PuzzleIslandHelper/WaterDroplet"

waterDroplet.depth = -8500

waterDroplet.texture = "objects/PuzzleIslandHelper/waterDroplet/lonn"

waterDroplet.placements =
{
    {
        name = "Water Droplet",
        data = 
        {
            waitTime = 0.5,
            randomWaitTime = true,
            baseColor = "0000ff"
        }
    }
}
waterDroplet.fieldInformation = 
{
    baseColor=
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return waterDroplet