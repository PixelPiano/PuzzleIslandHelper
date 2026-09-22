local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local veryDirectionSpecificDashBlock = {}

veryDirectionSpecificDashBlock.justification = { 0, 0 }

veryDirectionSpecificDashBlock.name = "PuzzleIslandHelper/VeryDirectionSpecificDashBlock"
veryDirectionSpecificDashBlock.minimumSize = {8,8}
veryDirectionSpecificDashBlock.depth = 1
veryDirectionSpecificDashBlock.placements =
{
    name = "Very Direction Specific Dash Block",
    data = 
    {
        tiletype = "3",
        blendin = true,
        permanent = false,
        canDash = true,
        upFlag = "",
        downFlag = "",
        leftFlag = "",
        rightFlag = "",
        upLeftFlag = "",
        downLeftFlag = "",
        upRightFlag = "",
        downRightFlag = "",
        flagOnBreak = "",
        width = 8,
        height = 8,
    }
}

veryDirectionSpecificDashBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
veryDirectionSpecificDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return veryDirectionSpecificDashBlock