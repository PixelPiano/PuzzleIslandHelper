local easeHelper = {}

local easeOptions = {"Linear","SineIn","SineOut","SineInOut","CubeIn","CubeOut","CubeInOut",
"QuadIn","QuadOut","QuadInOut","QuintIn","QuintOut","QuintInOut",
"ExpoIn","ExpoOut","ExpoInOut","ElasticIn","ElasticOut","ElasticInOut",
"BounceIn","BounceOut","BounceInOut","BackIn","BackOut","BackInOut",
"BigBackIn","BigBackOut","BigBackInOut"}
function easeHelper.getEaseOptions()
	return easeOptions
end

return easeHelper