local drawableSprite = require("structs.drawable_sprite")
local talkingDecal = {}


local types = {"Teleport","Dialog","Cutscene"}
local usage = {"DontUse","Use","UseAndWaitFor"}
local zoomModes = {"Screen","World"}
local camModes = {"World","Naive"}

talkingDecal.name = "PuzzleIslandHelper/TalkingDecal"
talkingDecal.justification = {0,0}
function talkingDecal.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "mode",
        "onCutscene",
        "offCutscene",
        "room",
        "nearestSpawnX",
        "nearestSpawnY",
        "useNearestSpawn",
        "onDialog",
        "offDialog",
        "zoomUsage",
        "zoomMode",
        "zoomX",
        "zoomY",
        "zoomAmount",
        "zoomDuration",
        "cameraUsage", 
        "camMode", 
        "camX",
        "camY",
        "cameraDuration",
        "walkUsage",
        "walkMode",
        "walkToX",
        "speedMult",
        "walkIntoWalls",
        "walkBackwards",
        "teleportMode",
        "glitchAmount",
        "wipe"
    }

    local function doNotIgnore(value)
        for i = #ignored, 1, -1 do
            if ignored[i] == value then
                table.remove(ignored, i)
                return
            end
        end
    end

    local atype = entity.mode or "Dialog"

    if atype == "Dialog" then
        doNotIgnore("onDialog")
        doNotIgnore("offDialog")
        doNotIgnore("zoomUsage")
        doNotIgnore("cameraUsage") 
        doNotIgnore("walkUsage")
        doNotIgnore("zoomDuration")
        doNotIgnore("walkToX")
        doNotIgnore("cameraDuration")
        doNotIgnore("zoomAmount")
        doNotIgnore("zoomX")
        doNotIgnore("zoomY")
        doNotIgnore("camX")
        doNotIgnore("camY")
        doNotIgnore("zoomMode")
        doNotIgnore("camMode") 
        doNotIgnore("walkMode")
        doNotIgnore("speedMult")
        doNotIgnore("walkIntoWalls")
        doNotIgnore("walkBackwards")
    elseif atype == "Cutscene" then
        doNotIgnore("onCutscene")
        doNotIgnore("offCutscene")
    else
        doNotIgnore("room")
        doNotIgnore("useNearestSpawn")
        doNotIgnore("nearestSpawnX")
        doNotIgnore("nearestSpawnY") 
        doNotIgnore("teleportMode")
        doNotIgnore("glitchAmount")
        doNotIgnore("wipe")
    end
    return ignored
end
talkingDecal.fieldOrder = {
    "x","y",
    "onDecalPath","offDecalPath","color", "depth","mode", "visibilityFlag", "talkEnabledFlag",
    "onCutscene",
    "offCutscene",
    "room","teleportMode","wipe","glitchAmount","nearestSpawnX","nearestSpawnY","useNearestSpawn",
    "onDialog","offDialog", 
    "zoomUsage","zoomMode","zoomX","zoomY","zoomAmount","zoomDuration",
    "cameraUsage", "camMode", "camX","camY","cameraDuration",
    "walkUsage","walkMode","walkToX","speedMult",
    "walkIntoWalls","walkBackwards","outline"
}
talkingDecal.placements = {}
for _, type in ipairs(types) do
    local placement = {
        name = "Talking Decal ("..type..")",
        data = {
            mode = type,
            outline = false,
            visibilityFlag = "",
            talkEnabledFlag = "",
            onCutscene = "",
            offCutscene = "",
            onDialog = "",
            offDialog = "",
            room = "",
            useNearestSpawn = false,
            nearestSpawnX = 0,
            nearestSpawnY = 0,
            zoomUsage = "DontUse",
            cameraUsage = "DontUse",
            walkUsage = "DontUse",
            zoomDuration = 1,
            walkToX = 0,
            cameraDuration = 1,
            zoomAmount = 1.5,
            zoomX = 0,
            zoomY = 0,
            camX = 0,
            camY = 0,
            zoomMode = "Screen",
            camMode = "World",
            walkMode = "World",
            speedMult = 1,
            walkIntoWalls = false,
            walkBackwards = false,
            depth = 2,
            onDecalPath = "1-forsakencity/flag",
            offDecalPath = "1-forsakencity/flag",
            color = "FFFFFF",
            teleportMode = "Instant",
            glitchAmount = 1,
            wipe = "Normal"
        }
    }
    table.insert(talkingDecal.placements,placement)
end
local teleportModes = {"Instant","Glitch","Wipe"}
local wipes = {"Normal"}

talkingDecal.fieldInformation =
{
    zoomUsage= {
        options = usage,
        editable = false
    },
    cameraUsage= {
        options = usage,
        editable = false
    },
    walkUsage= {
        options = usage,
        editable = false
    },
    zoomMode= {
        options = zoomModes,
        editable = false
    },
    camMode= {
        options = camModes,
        editable = false
    },
    walkMode= {
        options = camModes,
        editable = false
    },
    teleportMode= {
        options = teleportModes,
        editable = false
    },
    wipe= {
        options = wipes,
        editable = false
    },
    color = 
    {
        fieldType = "color",
        allowXNAColors = true
    }
}
function talkingDecal.depth(room, entity)
    return entity.depth or 0
end
function talkingDecal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(getTex(entity), entity)
        sprite:setScale(1,1)
        sprite:setJustification(0, 0)
        sprite.rotation = math.rad(0)
    return sprite
end
function getTex(entity)
    if drawableSprite.fromTexture("decals/" .. entity.onDecalPath .. "00") ~= nil then
        return "decals/" .. entity.onDecalPath .. "00"
    else
        return "decals/" .. entity.onDecalPath
    end
end

return talkingDecal
