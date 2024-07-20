local script = {
    name = "decalTargetToDecal",
    displayName = "Decal Targets to Decals",
    tooltip = "Replaces Decal Targets with regular decals",
    parameters = {
        groupId = ""
    }
}

local function doLayer(args, room)
    local decals = {}
    for _, entity in ipairs(room.entities) do
        if entity._name == "PuzzleIslandHelper/DecalEffectTarget" then
            if string.len(args.groupId) == 0 or entity.groupId == args.groupdId then
                local decal = {
                    _type = "decal"
                }
                decal.texture = "decals/" .. entity.decalPath
                decal.x = entity.x
                decal.y = entity.y
                decal.scaleX = entity.scaleX or 1
                decal.scaleY = entity.scaleY or 1
                decal.rotation = entity.rotation or 0
                decal.color = "ffffff"
                table.insert(decals,decal)
            end
        end
    end
    for _, decal in ipairs(decals) do
        table.insert(room.decalsBg, decal)
    end
end

function script.run(room, args)
    doLayer(args, room)
end

return script
