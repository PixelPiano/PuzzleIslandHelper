local blinker= {}
blinker.justification = { 0, 0 }

blinker.name = "PuzzleIslandHelper/Blinker"

blinker.depth = -10001

function blinker.texture(room, entity)
    if entity.startState then
        return "objects/PuzzleIslandHelper/blinker/on"
    else
        return "objects/PuzzleIslandHelper/blinker/off"
    end
end

blinker.placements =
{
    {
        name = "Blinker",
        data = 
        {
            index = 0,
            startState = true
        }
    }
}

return blinker