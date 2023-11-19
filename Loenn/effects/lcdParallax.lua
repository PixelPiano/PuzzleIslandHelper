local lcdParallax = {}

lcdParallax.name = "PuzzleIslandHelper/LCDParallax"

lcdParallax.defaultData = 
{
    texture = "",
    jitterAmount = 0.02,
	scrollX = 1.0,
	scrollY = 1.0,
	speedX = 0.0,
    speedY = 0.0,

    redOffset = 0.025,
    greenOffset = 0,
    blueOffset = -0.025,
    alpha = 1.0,
    color = "FFFFFF",

    flipX = false,
    flipY = false,
    loopX = true,
    loopY = true,

    simple = true,
    drawBase = true,
    clipAreas = 0,
    clipAreaTime = 0.5,
    instantIn = false,
    instantOut = false,
    fadeIn = false,

    fadeX = "",
    fadeY = "",
}
lcdParallax.fieldInformation = {
    color = {
        fieldType = "color"
    },
    texture = {
        options = {},
        editable = true
    }
}
return lcdParallax