//Tells the shader "Hey, take the texture located in Engine.Graphics.GraphicsDevice.Textures at 'index' give it a name."
//You'll need to use this for every custom texture you want the shader to target.
//However, if you don't deal with shaders directly in C# custom code, then you don't need to worry about this.
#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

//Tells the shader to grab the color of the pixel located at [texCoord] in the texture named [Name].
#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // Scene.TimeActive
uniform float2 CamPos; // SceneAs<Level>().Camera.Position
uniform float2 Dimensions; // (usually) new Vector2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

//This is required in order to grab any data from the screen.
//'0' Is looking for the texture located at index 0 of the current GraphicsDevice's TextureCollection...
//...which is the GameplayBuffer (fancy term for the canvas in which everything in Celeste is rendered to)

DECLARE_TEXTURE(text, 0);
//Note: Any declared texture MUST be declared BEFORE you use it
//This also goes for functions, variables, etc.
//This is why all the variables are declared before the main functions, otherwise they wouldn't be usable there.

//Additional Note: This model of shader can't support very much compared to later models.
//As such, be careful how much load you put on any one shader. If it has too much to juggle, it will straight up not compile.
//Using 'if' statements in shaders is risky because of how they're processed. An 'if' statement essentially doubles the work each pixel needs to do.
//The shader sees the 'if' statement as a possible branch and has to calculate both outcomes, so avoid using them if your shader has a lot going on.

//The following function is run once for EVERY pixel on the screen.
//  uv  -   This is a float2, a datatype with an 'x' and a 'y' value.
//          These two values will both be between 0.0 and 1.0.
//          They represent what percentage from the top right corner of the screen the pixel is.
//          This is true regardless of what value [Dimensions] is!
//          For example:
//          (0,0)__________(0,1)    
//              |          |
//              |          |
//              |__________|
//          (0,1)           (1,1)
//
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    //convenient conversion. For screen coordinates, just remove "CamPos" from the equation.
	float2 worldPos = CamPos + uv * Dimensions;
    //The color of the pixel at the coordinates [uv]
    //You can also pass in a completely different coordinate to get the color at that pixel instead.
    float4 color = SAMPLE_TEXTURE(text,uv);

    //The return value is the color the pixel will be changed to.
    return color * 10;

    //Some examples of modifications you could make to a pixel's color:

//Only outputs the 'red' value of the pixel
    float4 redValueOnly          =  float4(color.r,0,0,1);               

//Makes the red value pulse in and out using the passed in "Time" variable
    float4 redValuePulseInAndOut =  float4(color.r * sin(Time), 0, 0, 1);

//Creates a gradient from Black (left side of screen) to Red (right side of screen)
    float4 redGradientAlongXAxis =  float4(uv.x,0,0,1);

//Repeats the first quarter of pixels from the top of the screen to the bottom of the screen
    float2 uvMod = float2(uv.x, uv.y % 0.25);
    float4 firstQuarterY = SAMPLE_TEXTURE(text, uvMod);

//Mixes the color of the current pixel with the color of the pixel at the mirrored x coordinate
    float4 currentPixelColor = SAMPLE_TEXTURE(text, uv);
    float2 mirroredPosition = float2(0.5 + (0.5 - uv.x),uv.y);
    float4 mirroredPixelColor = SAMPLE_TEXTURE(text, mirroredPosition);
    float4 mixedColor = lerp(currentPixelColor, mirroredPixelColor, 0.5);

//Outputs the same color, but with the red value removed IF the green value is below 0.5.
    float4 redValueMaybeRemoved  =  float4(color.r * step(0.5, color.g), color.g, color.b, color.a);

    //"step" is a very useful function here. If the left value is greater than the right value, it returns '0', and '1' if not.
    //You can use it to skip using 'if' statements comprised of "is value A greater than value B?"
    //ur gonna be doing a lot of math fyi
    //But dw it's really fun just messing around without knowing what you're doing
}

//Usually you should leave this untouched
//Short version: Required to deal with custom vertex structures passed in from a Code Mod.
//Feel free to mess around with it though (but make a backup of this file just in case)
void SpriteVertexShader(inout float4 color: COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	
	position = mul(position, ViewMatrix);
	position = mul(position, TransformMatrix);
}

//Also leave this untouched
//Mainly for coders to access different shader methods from the shader
technique Shader
{
	pass pass0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}

//I'd recommend using ShaderToy.com to get inspired/steal code from!
//ShaderToy uses GLSL which functions the same as HLSL but with a modified syntax.
//Quick conversion chart for ya:
// GLSL  ->  HLSL
// vec2  ->  float2
// vec3  ->  float3
// mix   ->  lerp
// vec# * matrix -> mul(float#, matrix)
// 
// 