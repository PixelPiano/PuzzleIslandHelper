local compassNode= {}
compassNode.justification = {0,0}
compassNode.name = "PuzzleIslandHelper/CompassNode"
compassNode.depth = 1

function compassNode.texture(room, entity)
    if entity.startEmpty then
        return "objects/PuzzleIslandHelper/wallbuttonEmpty"
    end
    return "objects/PuzzleIslandHelper/wallbutton00"
end
local directions = {"Left","Right","Up","Down"}
compassNode.placements =
{
    {
        name = "Compass Node",
        data = 
        {
            nodeID = "",
            compassID = "",
            direction = "Left",
            startEmpty = false

        }
    }
}
compassNode.fieldInformation = 
{
    direction =
    {
        options = directions,
        editable = false
    }
}
return compassNode