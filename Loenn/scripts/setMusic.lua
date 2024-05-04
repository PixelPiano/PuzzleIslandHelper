local roomStruct = require("structs.room")
local state = require("loaded_state")
local script = {
    name = "setRoomMusic",
    displayName = "Set Music",
    tooltip = "Replaces the music event in the current room",
    parameters = {
        musicEvent = "",
        ambienceEvent = "",
        roomName = "*",
        onlyAffect = "0000",
        changeTo = "1111"
    }
}
function script.prerun(args)
    local function run(room, args)
        local function getBools(data)
            local result = {}
                for i = 1, 4 do
                    table.insert(result,data[i] == "1")
                end
                return result
        end
        local str  = ""
        if(room.musicLayer1) then
            str = str.."1"
        else
            str = str.."0"
        end
        if(room.musicLayer2) then
            str = str.."1"
        else
            str = str.."0"
        end
        if(room.musicLayer3) then
            str = str.."1"
        else
            str = str.."0"
        end
        if(room.musicLayer4) then
            str = str.."1"
        else
            str = str.."0"
        end
        local match = getBools(str)
        local from = getBools(args.onlyAffect)
        local to = getBools(args.changeTo)
        if(match[1] == from[1] and match[2] == from[2] and match[3] == from[3] and match[4] == from[4]) then
            room.musicLayer1 = false
            room.musicLayer2 = false
            room.musicLayer3 = false
            room.musicLayer4 = false
            if (args.musicEvent ~= "") then
                room.music = args.musicEvent or "music_oldsite_awake"
            end
            if(args.ambienceEvent ~= "") then
                room.music = args.ambienceEvent or ""
            end
            if (args.musicEvent ~= "") then
                room.music = args.musicEvent or "music_oldsite_awake"
            end
            if(args.ambienceEvent ~= "") then
                room.music = args.ambienceEvent or ""
            end
        end
    end

    for k,v in ipairs(state.map.rooms) do
        run(v, args)
    end
end



return script
