local passenger= {}
passenger.justification = { 0, 0 }

passenger.name = "PuzzleIslandHelper/PassengerMapProcessorDummy"
local names ={"Old","Tall","Civilian","Group","Dog","Child","3D","FestivalBoy","FormativeRival","PrimitiveRival"
              ,"Gravedigger","Mourner", "Gardener","GroveDweller","LostGirl","WorriedGirl","OceanGranny","MusicFanatic","RecordReceptionist"
              ,"ClinicReceptionist","Doctor"}
passenger.depth = 1

passenger.texture = "objects/PuzzleIslandHelper/passenger/wip"

passenger.placements = {}
for _, type in ipairs(names) do
    local placement = {
        name = "Passenger ("..type..")",
        data = {
            cutsceneID = "",
            passengerType = type,
            groups = 3,
            groupWidth = 10
        }
    }
    table.insert(passenger.placements,placement)
end

function passenger.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "passengerType",
        "groups",
        "groupWidth"
    }

    local function doNotIgnore(value)
        for i = #ignored, 1, -1 do
            if ignored[i] == value then
                table.remove(ignored, i)
                return
            end
        end
    end

    local atype = entity.passengerType or "Civilian"

    if atype == "Group" then
        doNotIgnore("groups")
        doNotIgnore("groupWidth")
    end
    return ignored
end
return passenger