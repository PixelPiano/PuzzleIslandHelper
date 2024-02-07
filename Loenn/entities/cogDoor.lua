local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local cogDoor= {}
cogDoor.justification = { 0, 0 }
cogDoor.name = "PuzzleIslandHelper/CogDoor"

cogDoor.depth = 0
function cogDoor.canResize(room, entity)
    if entity.vertical then
        return false, true
    else
        return true, false
    end
end
function cogDoor.minimumSize(room, entity)
    if entity.vertical then
        return 8, 24
    else
        return 24, 8
    end
end
function cogDoor.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    if entity.vertical then
        entity.width = 8
        entity.height =  math.max(entity.height, 24)
    else
        entity.width = math.max(entity.width, 24)
        entity.height = 8
    end
    return utils.rectangle(x,y, entity.width or 8, entity.height or 8)
end
function cogDoor.sprite(room, entity)
    local sprites = {}
    local width = math.max(entity.width or 0, 8)  
    local height = math.max(entity.height or 0, 8)
    local path = "objects/PuzzleIslandHelper/cog/door"

    local xm, ym
    if entity.vertical then
        xm = 0
        ym = 1
    else
        xm = 1
        ym = 0
    end
    local rotation = xm * (-math.pi / 2)
    local xOff = xm * 8
    local top = drawableSprite.fromTexture(path, entity)
    top:useRelativeQuad(0, 0, 8, 8)
    top.rotation = rotation
    top:setOffset(xOff,0)
    table.insert(sprites, top)

      for i = 8, ((xm * width) + (ym * height)) -16, 8 do
        local body = drawableSprite.fromTexture(path, entity)
        body:useRelativeQuad(0,8,8,8)
        body:addPosition(xm * i, ym * i)
        body.rotation = rotation
        body:setOffset(xOff,0)
        table.insert(sprites, body)
       end
    local bottom = drawableSprite.fromTexture(path, entity)
    bottom:useRelativeQuad(0,16,8,8)
    bottom:addPosition(xm * (width-8),ym * (height-8))
    bottom.rotation = rotation
    bottom:setOffset(xOff,0)
    table.insert(sprites,bottom)
    return sprites
end
cogDoor.placements =
{
    {
        name = "Cog Door",
        data =
        {
            width = 8,
            height = 16,
            canRevert = true,
            persistent = false,
            vertical = true,
            upOrLeft = true,
            doorID = ""
        }
    }

}


return cogDoor