local bubble = {}
bubble.justification = {0,0}
bubble.name = "PuzzleIslandHelper/Bubble"
bubble.texture = "objects/PuzzleIslandHelper/bubble/bubble"
local bubbles = {"Straight","FullControl","FloatDown"}
bubble.placements = {
    name = "Bubble",
    data = {
        bubbleType = "Straight",
        layers = 1,
        noCollision = false,
        respawns = true,
        onlyOnPipesBroken = true
    }
}
bubble.fieldInformation = 
{
    bubbleType = {
        options = bubbles,
        editable = false,
    }
}
bubble.depth = 0

return bubble