local _q = {}

_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/PassiveSecurity"

_q.depth = -10001
_q.texture = "objects/PuzzleIslandHelper/passiveSecurity/verySafe00"
_q.canResize = {false, false}
_q.placements = {
    {
        name = "Passive Security (Laser Activated)",
        data = {
            bulletsPerShot = 1,
            stationaryDirectionX = 0,
            stationaryDirectionY = 0,
            minAngle = 0,
            maxAngle = 360,
            shootInterval = 0.5,
            flag = ""
        }
    },
    {
        name = "Security Gun",
        data = {
            bulletsPerShot = 1,
            stationaryDirectionX = 0,
            stationaryDirectionY = 0,
            minAngle = 0,
            maxAngle = 360,
            shootInterval = 0.5,
            flag = ""
        }
    }
}

return _q