local methods = {"Add","Remove","Nothing"}
local effects = {
    ["Chorus"] = "Chorus",
    ["Distortion"] = "Distortion",
    ["Echo"] = "Echo",
    ["Flange"] = "Flange",
    ["Normalize"] = "Normalize",
    ["Oscillator"] = "Oscillator",
    ["Pitch Shift"] = "PitchShift",
    ["Tremolo"]  = "Tremolo",
}
local waves = {"Sine","Square","Sawup","Sawdown","Triangle","Noise"}
local events ={"Music","AltMusic","Ambience"}
local audioEffectTrigger = {}
audioEffectTrigger.name = "PuzzleIslandHelper/AudioEffectTrigger"
audioEffectTrigger.fieldInformation =
{
    wave =
    {
        options = waves,
        editable = false
    },
    onEnter = 
    {
        options = methods,
        editable = false
    },
    onLeave = 
    {
        options = methods,
        editable = false
    },
    onLevelStart = 
    {
        options = methods,
        editable = false
    },
    onLevelEnd = 
    {
        options = methods,
        editable = false
    },
    effect =
    {
        options = effects,
        editable = false
    },
    event =
    {
        options = events,
        editable = false
    }
}
function audioEffectTrigger.ignoredFields(entity)
    local ignored = {
        "_id",
        "_name",
        "chorusMix",
        "chorusDepth",
        "chorusRate",
        "distortionLevel",
        "echoDelay",
        "echoFeedback",
        "dryLevel",
        "wetLevel",
        "flangeMix",
        "flangeDepth",
        "flangeRate",
        "fadeTime",
        "threshhold",
        "maxAmp",
        "wave",
        "oscRate",
        "pitch",
        "fftSize",
        "tremoloFreq",
        "tremoloDepth",
        "tremoloDuty",
        "tremoloFlatness",
        "tremoloShape",
        "tremoloSkew",
        "tremoloPhase",
        "tremoloSpread"
    }

    local function doNotIgnore(value)
        for i = #ignored, 1, -1 do
            if ignored[i] == value then
                table.remove(ignored, i)
                return
            end
        end
    end

    local atype = entity.effect or "Echo"

    if atype == "Chorus" then
        doNotIgnore("chorusMix")
        doNotIgnore("chorusDepth")
        doNotIgnore("chorusRate")
    elseif atype == "Distortion" then
        doNotIgnore("distortionLevel")
    elseif atype == "Echo" then
        doNotIgnore("echoDelay")
        doNotIgnore("echoFeedback")
        doNotIgnore("dryLevel")
        doNotIgnore("wetLevel")
    elseif atype == "Flange" then
        doNotIgnore("flangeMix")
        doNotIgnore("flangeDepth")
        doNotIgnore("flangeRate")
    elseif atype == "Normalize" then
        doNotIgnore("fadeTime")
        doNotIgnore("threshold")
        doNotIgnore("maxAmp")
    elseif atype == "Oscillator" then
        doNotIgnore("wave")
        doNotIgnore("oscRate")
    elseif atype == "PitchShift" then
        doNotIgnore("pitch")
        doNotIgnore("fftSize")
    elseif atype == "Tremolo" then
        doNotIgnore("tremoloFreq")
        doNotIgnore("tremoloDepth")
        doNotIgnore("tremoloDuty")
        doNotIgnore("tremoloFlatness")
        doNotIgnore("tremoloShape")
        doNotIgnore("tremoloSkew")
        doNotIgnore("tremoloPhase")
        doNotIgnore("tremoloSpread")
    end
    return ignored
end
--thanks trigger trigger loenn plugin code for saving me hours of frustration :)
audioEffectTrigger.placements = {}
for _, mode in pairs(effects) do
    local placement = {
        name = "Audio Effect ("..mode..")",
        data = {
            width = 8,
            height = 8,
            flag = "",
            inverted = false,
            onEnter = "Add",
            onLeave = "Remove",
            onLevelStart = "Nothing",
            onLevelEnd = "Nothing",
            effect = mode,
            effectID = "changeToUniqueName",
            event = "Music",
            chorusMix = 50,
            chorusRate = 0.8,
            chorusDepth = 3,
            distortionLevel = 0.5,
            echoDelay = 500,
            echoFeedback = 50,
            dryLevel = 0,
            wetLevel = 0,
            flangeMix = 50,
            flangeDepth = 1,
            flangeRate = 0.1,
            fadeTime = 5000,
            threshold = 0.1,
            maxAmp = 20,
            wave = "Sine",
            oscRate = 220,
            pitch = 1,
            fftSize = 1024,
            tremoloFreq = 5,
            tremoloDepth = 1,
            tremoloShape = 1,
            tremoloSkew = 0,
            tremoloDuty = 0.5,
            tremoloFlatness = 1,
            tremoloPhase = 0,
            tremoloSpread = 0,
        }
    }
    table.insert(audioEffectTrigger.placements,placement)
end

audioEffectTrigger.fieldOrder =
{
    "x","y","width","height","flag","inverted",
    "effect","effectID","event","onEnter",
    "onLeave","onLevelStart","onLevelEnd"
}
return audioEffectTrigger