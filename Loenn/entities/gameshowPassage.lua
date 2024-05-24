local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local passage = {}
local path ="objects/PuzzleIslandHelper/gameshow/gameshowPassage/"
local facings = {"Left","Right"}
passage.name = "PuzzleIslandHelper/GameshowPassage"
passage.depth = 1
passage.placements = 
{
    {
        name = "Gameshow Passage",
        data = 
        {
            facing = "Left",
            teleportTo= "",
            returningToSet = false,
            fadeTime = 1,
        }
    },
}
passage.fieldInformation =
{
    facing = 
    {
        options = facings,
        editable = false
    },
}

-- Manual offsets and justifications of the sprites
function passage.sprite(room, entity)
    local sprites = {}
    local width = 26
    local height = 32
    local color = "ffff00"
    local bg = drawableSprite.fromTexture(path .. "texture", entity)
    local fg = drawableSprite.fromTexture(path .. "texture", entity)
    local backlight = drawableSprite.fromTexture(path .. "light(side)",entity)
    local frontlight = drawableSprite.fromTexture(path .. "lightFg(side)",entity)
    bg:useRelativeQuad(0, 0, width, height)
    fg:useRelativeQuad(width,0,width,height)
    backlight:useRelativeQuad(0,0,width,height)
    frontlight:useRelativeQuad(0,0,width*2,height)
    frontlight:addPosition(-width,0)
    backlight:setColor(color)
    frontlight:setColor(color)
    if entity.facing == "Right" then
        bg:setScale(-1, 1)
        fg:setScale(-1, 1)
        backlight:setScale(-1,1)
        frontlight:setScale(-1,1)
        bg:addPosition(width,0)
        fg:addPosition(width,0)
        backlight:addPosition(width,0)
        frontlight:addPosition(width + 52,0)
    end
    table.insert(sprites,backlight)
    table.insert(sprites,bg)
    table.insert(sprites,fg)
    table.insert(sprites,frontlight)
    return sprites
end
function passage.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, 26, 32)
end
return passage