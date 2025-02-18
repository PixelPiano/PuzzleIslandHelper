local sideFlag = {}

local states = {"True","False","Inactive"}
sideFlag.name = "PuzzleIslandHelper/SideFlag"
sideFlag.minimumSize = {8,8}
sideFlag.placements =
{
    {
        name = "Side Flag (Vertical)",
        data = {
            flag = "",
            vertical = true,
            onlyOnLeave = false,
            transitionUpdate = true,
            left = "True",
            right = "False",
            onlyBetweenTopAndBottom = false,
            useCenter = false

        }
    },
    {
        name = "Side Flag (Horizontal)",
        data = {
            flag = "",
            vertical = false,
            onlyOnLeave = false,
            transitionUpdate = true,
            top = "True",
            bottom = "False",
            onlyBetweenLeftAndRight = false,
            useCenter = false
        }
    }
}
function sideFlag.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "vertical",
        "top",
        "bottom",
        "left",
        "right",
        "onlyBetweenTopAndBottom",
        "onlyBetweenLeftAndRight"
    }
    local function doNotIgnore(value)
       for i = #ignored, 1, -1 do
           if ignored[i] == value then
               table.remove(ignored, i)
               return
           end
       end
    end
    if entity.vertical then
        doNotIgnore("left")
        doNotIgnore("right")
        doNotIgnore("onlyBetweenTopAndBottom")
    else
        doNotIgnore("top")
        doNotIgnore("bottom")
        doNotIgnore("onlyBetweenLeftAndRight")
    end
    return ignored
end
sideFlag.fieldInformation =
{
    top =
    {
        options = states,
        editable = false
    },
    bottom =
    {
        options = states,
        editable = false
    },
    left =
    {
        options = states,
        editable = false
    },
    right =
    {
        options = states,
        editable = false
    },
}
function sideFlag.canResize(room, entity)
    if entity.vertical then
        entity.height = 8
        return {true, false}
    end
    entity.width = 8
    return {false, true}
end
return sideFlag