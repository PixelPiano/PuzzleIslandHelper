local _q = {}

_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/PassiveSecurity"

_q.depth = -10001
local modes = {"LaserActivated","Monitoring","Stationary"}
local bullets = {"Hot","Sticky","Bouncy","Default"}
_q.texture = "objects/PuzzleIslandHelper/passiveSecurity/verySafe00"
_q.canResize = {false, false}
_q.placements = {
    {
        name = "Passive Security (Laser Activated)",
        data = {
            mode = "LaserActivated",
            flipX = false,
            laserID = "laser_to_watch",
            realisticAim = true,
            bulletType = "Default",
            bulletsCollideWithSolids = false,
            bulletsPerShot = 1
        }
    },
    {
        name = "Passive Security (Monitoring)",
        data = {
            mode = "Monitoring",
            flag = "really_big_gun",
            inverted = false,
            flipX = false,
            rotateRange = 5,
            viewRange = 5,
            lookTime = 4,
            realisticAim = true,
            bulletType = "Default",
            bulletsCollideWithSolids = false,
            bulletsPerShot = 1
        }
    },
    {
        name = "Passive Security (Stationary)",
        data = {
            mode = "Stationary",
            flag = "really_big_gun",
            inverted = false,
            flipX = false,
            fireRate = 1,
            realisticAim = true,
            bulletType = "Default",
            bulletsCollideWithSolids = false,
            bulletsPerShot = 1
        }
    }
}

_q.fieldInformation =
{
    mode =
    {
        options = modes,
        editable = false
    },
    bulletType = 
    {
        options = bullets,
        editable = false
    }
}

return _q