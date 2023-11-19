local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local pipeSpout= {}
pipeSpout.justification = { 0, 0 }

pipeSpout.name = "PuzzleIslandHelper/PipeSpout"

pipeSpout.depth = -8500

pipeSpout.minimumSize = {8,8}

local directions = {"Up","Down","Left","Right"}
local hideMethods = {"Retreat","Dissolve"}
pipeSpout.placements =
{
    {
        name = "Pipe Spout",
        data = 
        {
            width = 8,
            height = 8,
            hideMethod = "Retreat",
            startDelay = 0.0,
            moveDuration = 1,
            direction = "Up",
            flag = "",
            inverted = false,
            isTimed = false,
            waitTime = 0.7
        }
    }
}
pipeSpout.fieldInformation =
{
    direction =
    {
        options = directions,
        editable = false
    },
    hideMethod =
    {
        options = hideMethods,
        editable = false
    }
}

local path = "objects/PuzzleIslandHelper/waterPipes/"
function pipeSpout.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    return utils.rectangle(x, y, width, height)
end
function pipeSpout.sprite(room, entity, viewport)
    local splash = drawableSprite.fromTexture("objects/PuzzleIslandHelper/waterPipes/splashtexture2lonn", entity)
    local sprites = {}
    local dir = entity.direction or "Up"
    local switch = true

    
    if dir == "Up" or dir == "Down" then
        if dir == "Down" then
            splash.rotation = 3 * math.pi / 2
            splash.y = entity.y + entity.height + 8
        else
            splash.rotation = math.pi/2
            splash.y = entity.y - 8
        end
        splash.x = splash.x + 4
        for i = 0, entity.height - 8, 8 do
            local spr = drawableSprite.fromTexture(path .. "streamSub2",entity)
            spr.x = entity.x + 8
            spr.rotation = math.pi/2
            if switch then
                spr:useRelativeQuad(0, 0, 8, 10)
            else
                spr:useRelativeQuad(8, 0, 8, 10)
            end

            spr.y = entity.y + i
            table.insert(sprites,spr)
            switch = not switch
        end
    else
        if dir == "Left" then
            splash.x = entity.x - 8
        else
            splash.rotation = math.pi
            splash.x = entity.x + entity.width + 8
        end
        splash.y = splash.y + 4
        for i = 0, entity.width - 8, 8 do
            local spr = drawableSprite.fromTexture(path .. "streamSub2",entity)
            spr.x = entity.x + i
            if switch then
                spr:useRelativeQuad(0, 0, 8, 10)
            else
                spr:useRelativeQuad(8, 0, 8, 10)
            end
            table.insert(sprites,spr)
            switch = not switch
        end
    end

    table.insert(sprites,splash)
    return sprites
end

return pipeSpout