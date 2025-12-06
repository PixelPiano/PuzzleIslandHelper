local towerInsideSegment = {}
local xnaColors = require("consts.xna_colors")
local gray = xnaColors.LightGray

towerInsideSegment.justification = { 0, 0 }

towerInsideSegment.name = "PuzzleIslandHelper/TowerInsideSegment"

towerInsideSegment.fillColor = {gray[1] * 0.3, gray[2] * 0.3, gray[3] * 0.3, 0.6}
towerInsideSegment.borderColor = {gray[1] * 0.8, gray[2] * 0.8, gray[3] * 0.8, 0.8}
towerInsideSegment.depth = 1
towerInsideSegment.minimumSize = {8,8}
towerInsideSegment.placements =
{
    {
        name = "Tower (Inside Segment)",
        data = {
            width = 8,
            height = 8,
            depth = 1
        }
    }
}
function towerInsideSegment.depth(room, entity)
    return entity.depth or 1
end
return towerInsideSegment