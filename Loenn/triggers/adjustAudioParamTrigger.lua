local enums = require "consts.celeste_enums"
local adjustAudioParamTrigger = {}
adjustAudioParamTrigger.name = "PuzzleIslandHelper/AdjustAudioParamTrigger"

local effects = {"Chorus","Distortion","Echo","Flange","Normalize","Oscillator","PitchShift","Tremolo"}
local waves = {"Sine","Square","Sawup","Sawdown","Triangle","Noise"}
local params =
{
    ["Chorus Mix"] = "ChorusMix",
    ["Chorus Depth"] = "ChorusDepth",
    ["Chorus Rate"] = "ChorusRate",
    ["Distortion Level"] = "DistortLevel",
    ["Echo Delay"] = "EchoDelay",
    ["Echo Feedback"] = "EchoFeedback",
    ["Dry Level"] = "DryLevel",
    ["Wet Level"] = "WetLevel",
    ["Flange Mix"] = "FlangeMix",
    ["Flange Depth"] = "FlangeDepth",
    ["Flange Rate"] = "FlangeRate",
    ["Fade Time"] = "NormFadeTime",
    ["Threshhold"] = "NormThresh",
    ["Max Amp"] = "NormMaxAmp",
    ["Wave"] = "OscWave",
    ["Osc Rate"] = "OscRate",
    ["Pitch"] = "PitchPitch",
    ["Fft Size"] = "PitchSize",
    ["Tremolo Freq"] = "TremFreq",
    ["Tremolo Depth"]= "TremDepth",
    ["Tremolo Duty"] = "TremShape",
    ["Tremolo Flatness"] = "TremSkew",
    ["Tremolo Shape"] = "TremDuty",
    ["Tremolo Skew"] = "TremFlatness",
    ["Tremolo Phase"] = "TremPhase",
    ["Tremolo Spread"] = "TremSpread",
}
local easings = 
{
    "Linear",
    "SineIn",
    "SineOut",
    "SineInOut",
    "CubeIn",
    "CubeOut",
    "CubeInOut",
    "QuintIn",
    "QuintOut",
    "QuintInOut",
    "QuadIn",
    "QuadOut",
    "QuadInOut",
    "BounceIn",
    "BounceOut",
    "BounceInOut",
    "ElasticIn",
    "ElasticOut",
    "ElasticInOut",
    "BackIn",
    "BackOut",
    "BackInOut",
    "BigBackIn",
    "BigBackOut",
    "BigBackInOut",
    "ExpoIn",
    "ExpoOut",
    "ExpoInOut"
}
local modes = {"OnEnter","OnLeave","OnLevelStart","OnLevelEnd"}
adjustAudioParamTrigger.fieldInformation =
{
    mode =
    {
        options = modes,
        editable = false
    },
    wave =
    {
        options = waves,
        editable = false
    },
    effect =
    {
        options = effects,
        editable = false
    },
    parameter = 
    {
        options = params,
        editable = false
    },
    ease =
    {
        options = easings,
        editable = false
    }
}

adjustAudioParamTrigger.placements =
{
    {
        name = "Adjust Audio Param",
        data = {
            width = 8,
            height = 8,
            flag = "",
            inverted = false,
            mode = "OnEnter",
            effect = "Chorus",
            parameter = "Chorus Mix",
            time = 1,
            delay = 0,
            value = 1,
            ease = "SineIn",
            onlyOnce = true,
            persistUntilComplete = true,
            allowFlagInterrupt = false,
            snapValueIfInterrupted = false,
            effectID = "changeToUniqueName",
           
        }
    },
}
adjustAudioParamTrigger.fieldOrder =
{
    "x","y","width","height","flag","inverted",
    "time","delay","effect","effectID","parameter","value","mode","ease",
    "onlyOnce","persistUntilComplete","allowFlagInterrupt","snapValueIfInterrupted"
}

return adjustAudioParamTrigger