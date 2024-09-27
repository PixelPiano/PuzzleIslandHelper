
local voidLightShooter = {}

voidLightShooter.justification = { 0, 0 }

local directions = {"Right", "Left","Up","Down"}
voidLightShooter.name = "PuzzleIslandHelper/VoidLightShooter"
voidLightShooter.texture = "objects/PuzzleIslandHelper/wip/texture"
voidLightShooter.depth = 1

voidLightShooter.placements =
{
    {
        name = "Void Light Shooter",
        data = {
            direction = "Left",
            bulletRadius = 16,
            bulletSpeed = 30,
            shootInterval = 1.2
        }
    }
}
voidLightShooter.fieldInformation =
{
    direction = 
    {
        options = directions,
        editable = false
    }
}
return voidLightShooter