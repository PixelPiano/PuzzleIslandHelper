local roomStruct = require("structs.room")

local script = {
    name = "setRoomMusic",
    displayName = "Set Music",
    tooltip = "Replaces the music event in the current room",
    parameters = {
        musicEvent = "",
        ambienceEvent = "",
        roomName = "*"
    }
}

function script.run(room, args)
    if (args.musicEvent ~= "") then
        room.music = args.musicEvent or "music_oldsite_awake"
    end
    if(args.ambienceEvent ~= "") then
        room.music = args.ambienceEvent or ""
    end
end

return script
