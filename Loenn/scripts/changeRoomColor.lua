local script = {
    name = "changeRoomColor",
    displayName = "Change Room Color",
    parameters = {
        to = "0",
        uselessThingToMakeTheBoxNotDisappear = true
    },
    fieldInformation =
    {
        to =
        {
            options = {"0","1","2","3","4","5","6","7"},
            editable = false
        }
    },
    tooltip = "Changes the color of the selected rooms",
    tooltips = {
        to = "The color id the room will be changed to",
    },
}
local function encodeString(str)
    return { innerText = str }
end

function script.run(room, args)
    local to = args.to.tonumber() or 0

    local propertyName = "color"
    room[propertyName] = to;
end
return script