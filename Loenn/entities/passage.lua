local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local passage = {}
local facings = {"Left","Right"}
passage.name = "PuzzleIslandHelper/Passage"
passage.depth = 1
passage.placements = 
{
    {
        name = "Passage (Default)",
        data = 
        {
            facing = "Left",
            teleportTo= "",
            fadeTime = 1,
            lightColor = "FFFFFF",
            folderPath = "objects/PuzzleIslandHelper/gameshow/passage/",
            custom = false
        }
    },
    {
        name = "Passage (Custom)",
        data = 
        {
            facing = "Left",
            teleportTo= "",
            fadeTime = 1,
            lightColor = "FFFFFF",
            folderPath = "objects/PuzzleIslandHelper/gameshow/passage/",
            custom = true
        }
    }
}
function passage.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "folderPath",
        "lightColor",
        "custom"
    }
    local function doNotIgnore(value)
       for i = #ignored, 1, -1 do
           if ignored[i] == value then
               table.remove(ignored, i)
               return
           end
       end
    end
    if entity.custom then
        doNotIgnore("folderPath")
        doNotIgnore("lightColor")
    end
    return ignored
end
passage.fieldInformation =
{
    facing = 
    {
        options = facings,
        editable = false
    },
    lightColor =
    {
        fieldType = "color",
        allowXNAColors = true
    }
}

-- Manual offsets and justifications of the sprites
function passage.sprite(room, entity)
    local sprites = {}
    local width = 26
    local height = 32
    local path = entity.folderPath or "objects/PuzzleIslandHelper/gameshow/passage/"
    local color = entity.lightColor or "ffffff"
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