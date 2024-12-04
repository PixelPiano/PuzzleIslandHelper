local drawableSprite = require("structs.drawable_sprite")

local housePart = {}

--housePart.justification = { 0, 0 }

housePart.name = "PuzzleIslandHelper/HousePart"
function housePart.depth(room, entity)
    return entity.depth or 0
end
housePart.placements = {
    name = "House Part",
    data = {
        outline = false,
        decalPath = "house1A",
        depth = 2,
        scaleX = 1,
        scaleY = 1,
        rotation = 0,
        rotationRate = 0,
        color = "FFFFFF"
    }
}
housePart.fieldInformation =
{
    depth =
    {
        fieldType = "integer"
    },
    color = 
    {
        fieldType = "color",
        allowXNAColors = true
    }
}
function housePart.sprite(room, entity)
    local function getHouseTex(entity)
        if drawableSprite.fromTexture("objects/PuzzleIslandHelper/houseParts/" .. entity.decalPath .. "00") ~= nil then
            return "objects/PuzzleIslandHelper/houseParts/" .. entity.decalPath .. "00"
        else
            return "objects/PuzzleIslandHelper/houseParts/" .. entity.decalPath
        end
    end
    local sprite = drawableSprite.fromTexture(getHouseTex(entity), entity)
        sprite:setScale(entity.scaleX or 1, entity.scaleY or 1)
        sprite:setJustification(0.5, 0.5)
        sprite.rotation = math.rad(entity.rotation or 0)
    return sprite
end

return housePart
