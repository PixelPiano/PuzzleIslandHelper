local passenger= {}
passenger.justification = { 0, 0 }

passenger.name = "PuzzleIslandHelper/PassengerMapProcessorDummy"
local names ={"Old","Tall","Civilian","Group","Dog","Child","3D","FestivalBoy","FormativeRival","PrimitiveRival"
              ,"Gravedigger","Mourner", "Gardener","GroveDweller","LostGirl","WorriedGirl","OceanGranny","MusicFanatic","RecordReceptionist"
              ,"ClinicReceptionist","Doctor","Tutorial"}
passenger.depth = 1

passenger.texture = "objects/PuzzleIslandHelper/passenger/wip"
local methods = {"OnlyOnce","RepeatLast","Loop"}
passenger.placements = {}
for _, type in ipairs(names) do
    local placement = {
        name = "Passenger ("..type..")",
        data = {
            cutsceneID = "",
            passengerType = type,
            groups = 3,
            groupWidth = 10,
            dialog = "",
            dialogMethod = "OnlyOnce",
            dialogFlags = "",
            flag = "",
            cutsceneArgs = "",
            cutsceneOnTransition = false,
            facing = "Left"
        }
    }
    table.insert(passenger.placements,placement)
end
local facings = {"Left","Right"}
passenger.fieldInformation =
{
    dialogMethod = {
        options = methods,
        editable = false
    },
    facing = {
        options = facings,
        editable = false
    }
}
function passenger.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
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