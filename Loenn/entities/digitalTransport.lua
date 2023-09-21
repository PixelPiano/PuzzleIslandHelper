local digitalTransport= {}
digitalTransport.justification = { 0, 0 }

digitalTransport.name = "PuzzleIslandHelper/DigitalTransport"

digitalTransport.depth = -8500

digitalTransport.texture = "objects/PuzzleIslandHelper/transport/outlet00"

digitalTransport.placements =
{
    {
        name = "Digital Transport",
        data = 
        {
            targetId = "teleportID",
            roomName = "room",
            toHub = false
        }
    }
}

return digitalTransport