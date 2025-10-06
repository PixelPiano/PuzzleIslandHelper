local memoryGravZone = {}

memoryGravZone.name = "PuzzleIslandHelper/MemoryGravZone"
memoryGravZone.depth = 1

local function parseColor(c, rgbMult, alphaMult)
    local array = {}
    table.insert(array,tonumber(c:sub(1,2),16) * rgbMult)
    table.insert(array,tonumber(c:sub(3,4),16) * rgbMult)
    table.insert(array,tonumber(c:sub(5,6),16) * rgbMult)
    table.insert(array,tonumber(c:sub(7,8),16) * alphaMult)
    return array
end
function memoryGravZone.fillColor(entity, room)
    return parseColor(entity.color, 0.3, 0.6)
end
function memoryGravZone.borderColor(entity, room)
    return parseColor(entity.color, 0.8, 0.8)
end
memoryGravZone.placements = {
    {
        name = "Memory Grav Zone",
        data = {
            visibleFlag = "",
            activeFlag = "",
            color = "FF0000FF",
            agitateMult = 1,
            agitateFade = 0.2,
            agitateThresh = 1,
            crystalizeThresh = 2
        }
    },
}
memoryGravZone.fieldInformation
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
        useAlpha = true
    },
}
return memoryGravZone