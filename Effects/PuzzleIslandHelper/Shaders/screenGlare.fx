#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Center;
uniform float Amplitude;
uniform float Size;
uniform float Random;
uniform bool Strong;
float2 Circle(float2 uv, float2 center, float size)
{
    float r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x)*invAr;
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
static float dis=.5;
static float width=.1;
static float blur=.1;
static float circles = 4;
#define PI 3.14159265358979323846
#define e  2.71828182845904523

float2x2 rotate2D(float angle){
	return float2x2(cos(angle),-sin(angle),
               sin(angle),cos(angle));
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
	float4 color = SAMPLE_TEXTURE(text, uv);
	float2 pos = uv - .5;
	uv.x = (Dimensions.x/Dimensions.y) * uv.x;
	float dist = pos.x * pos.x + pos.y * pos.y;

	float2 o =uv+float2(pos.x,pos.y);
	float len = length(o);
	float angle = atan2(uv.x, uv.y);
	float a = fmod(angle * PI, angle);
	color = lerp(color, (float4)1, a);
	return color;
}

void SpriteVertexShader(inout float4 color: COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	
	position = mul(position, ViewMatrix);
	position = mul(position, TransformMatrix);
}

technique Shader
{
	pass pass0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}