local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ruinsPole = {}
ruinsPole.name = "PuzzleIslandHelper/RuinsPole"
ruinsPole.depth = 9001
ruinsPole.canResize = {false, true}
ruinsPole.placements = {
    name = "Ruins Pole",
    data = {
        height = 8,
        brokenTop = false,
        brokenBottom = false,
        forElevator = false,
        crystalized = false,
        topNum = -1,
        bottomNum = -1
    }
}
function ruinsPole.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "topNum",
        "bottomNum"
    }
    return ignored
end
function ruinsPole.minimumSize(room, entity)
    if entity.forElevator then
        return 5,8
    else
        return 13,8
    end
end
-- Manual offsets and justifications of the sprites
function ruinsPole.sprite(room, entity)
    local sprites = {}
    local height = math.max(entity.height or 0, 8)

    local path = "objects/PuzzleIslandHelper/ruinsPole/".. (entity.crystalized ? "crystalTexture" : "texture")

    if entity.forElevator then
        for i = 0, height-8, 8 do
            local bodSprite = drawableSprite.fromTexture(path, entity)
            bodSprite:useRelativeQuad(0, 8, 13, 8)
            bodSprite:addPosition(0,i)
            table.insert(sprites, bodSprite)
        end
        return sprites
    end
    math.randomseed(entity.x + height,entity.y - height)
    
    local num = math.random(1,2)

    local firstSprite = drawableSprite.fromTexture(path, entity)
    if entity.brokenTop then
        firstSprite:useRelativeQuad(5 * num or 1,0,5,8)
        entity.topNum = num
    else
        firstSprite:useRelativeQuad(0,0,5,8)
    end
    firstSprite:addPosition(0,0)
    table.insert(sprites,firstSprite)

    for i = 0, height-24, 8 do
        local bodySprite = drawableSprite.fromTexture(path, entity)
        bodySprite:useRelativeQuad(0, 0, 5, 8)
        bodySprite:addPosition(0,i + 8)
        table.insert(sprites, bodySprite)
    end
    math.randomseed(entity.x - height,entity.y + height)
    num = math.random(1,2)

    local lastSprite = drawableSprite.fromTexture(path, entity)
    if entity.brokenBottom then
        lastSprite:useRelativeQuad(5 * num or 1,0,5,8)
        entity.bottomNum = num
    else
         lastSprite:useRelativeQuad(0,0,5,8)
    end
    lastSprite:addPosition(0,height)
    lastSprite:setScale(1,-1)
    table.insert(sprites,lastSprite)

    return sprites
end

function ruinsPole.selection(room, entity)
    local width = 5;
    if entity.forElevator then width = 13 end
    return utils.rectangle(entity.x, entity.y, width, math.max(entity.height, 8))
end

return ruinsPole