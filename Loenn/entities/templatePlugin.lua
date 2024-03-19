local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local xnaColors = require("consts.xna_colors")

local templatePlugin= {}

templatePlugin.name = "PuzzleIslandHelper/TemplateEntity" --The "link" back to our custom entity's cs file. (see TemplateEntity.cs)

templatePlugin.depth = 0

templatePlugin.texture = "objects/PuzzleIslandHelper/templatePlugin/texture"

templatePlugin.justification = { 0.5, 0.5 }

--A table of presets the editor will display for this entity.
templatePlugin.placements =
{
    { 
        --Searchable name in the editor
        name = "Template Plugin (Default)", 

        --Table of custom attributes that can be grabbed in code from EntityData (see TemplateEntity.cs)
        data =
        {
            flag = "",
            inverted = false,
        }
    },
        {
        name = "Template Plugin (Advanced)",
        data =
        {
            flag = "",
            inverted = false,
            mode = "Mode 1",
            color = "000000",
        }
    }
}
local modes = {"Mode 1","Mode 2","A very important mode", "The least liked mode", "Jerry"}

--Lets us assign more varied information to an attribute
templatePlugin.fieldInformation = 
{
    mode = 
    {
        options = modes,
        editable = false,
    },
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}
--we can create a function for an attribute of the plugin if we want it to update itself over time
--in this case, we want to shade the texture using the 'color' attribute we gave the entity
function templatePlugin.sprite(room,entity)
   local path = "objects/PuzzleIslandHelper/templatePlugin/texture"
   local sprite = drawableSprite.fromTexture(path,entity)
   sprite:setColor(utils.getColor(entity.color or "FFFFFF"))
   sprite:setJustification(-0.25, -0.25)
   return sprite
end

return templatePlugin