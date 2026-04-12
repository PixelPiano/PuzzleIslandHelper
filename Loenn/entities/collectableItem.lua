local drawableSprite = require("structs.drawable_sprite")
local collectableItem = {}


collectableItem.name = "PuzzleIslandHelper/CollectableItem"
collectableItem.justification = {0,0}
collectableItem.placements = 
{
    name = "Collectable Item",
    data = 
    {
        flag = "",
        flagOnCollect = "",
        depth = 2,
        texturePath = "1-forsakencity/flag",
        permanent = false,
        removeOnCollect = false,
        doCutscene = true,
        mainText = "",
        subText = "",
        waitForInput = true
    }
}
function collectableItem.depth(room, entity)
    return entity.depth or 0
end
function collectableItem.sprite(room, entity)
    local path;
    if drawableSprite.fromTexture("decals/" .. entity.texturePath .. "00") ~= nil then
        path = "decals/" .. entity.texturePath .. "00"
    else
        path = "decals/" .. entity.texturePath
    end
    local sprite = drawableSprite.fromTexture(path, entity)
        sprite:setScale(1,1)
        sprite:setJustification(0, 0)
    return sprite
end

return collectableItem
