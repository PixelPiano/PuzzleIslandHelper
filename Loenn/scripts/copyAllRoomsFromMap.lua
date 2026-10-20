local state = require("loaded_state")
local snapshot = require("structs.snapshot")
local utils = require("utils")
local mapcoder = require("mapcoder")
local mapStruct = require("structs.map")
local mapItemUtils = require("map_item_utils")

local script = {
    name = "copyRoomsFromMap",
    displayName = "Copy Rooms From Map",
    parameters = {
        from = "",
        xOffset = 0,
        yOffset = 0,
        replace = false,
    },
    fieldOrder = { "xOffset", "yOffset","from", "replace" },
    fieldInformation = {
        from = {
            fieldType = "loennScripts.directFilepath",
            extension = "bin"
        },
    },
    tooltip = "Copies every room from another map",
    tooltips = {
        from = "The file from which the rooms will be copied",
        replace = "If enabled, any room in the current map with the same name as the a New Room Name gets replaced with the newly copied room.",
        xOffset = "Additional x offset for the copied room.",
        yOffset = "Additional y offset for the copied room."
    },
}

function script.prerun(args)
    local targetMap = mapStruct.decode(mapcoder.decodeFile(args.from))
    local oldRooms = {}
    for i, r in ipairs(targetMap.rooms) do
        table.insert(oldRooms, r)
    end
    local newRooms = {}
    targetMap = nil
    local function forward(data)
        local map = state.map
        for index, room in ipairs(oldRooms) do
            local newRoom = utils.deepcopy(room)
            newRoom.x = newRoom.x + args.xOffset
            newRoom.y = newRoom.y + args.yOffset
            mapItemUtils.addItem(map, newRoom)
            if state.isItemSelected(room) then
                state.selectItem(newRoom)
            end
            table.insert(newRooms, newRoom);
        end
    end

    local function backward(data)
        local map = state.map
        for k, v in ipairs(newRooms) do
            mapItemUtils.deleteRoom(map, v)
        end
    end

    forward()

    return snapshot.create(script.name, {}, backward, forward)
end

return script