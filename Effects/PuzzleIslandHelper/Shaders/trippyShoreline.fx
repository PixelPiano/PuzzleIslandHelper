#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
uniform float4 BackColor;
uniform float4 FrontColor;
uniform int Layers = 4;
uniform int MinSize = 3;
uniform int MaxSize = 6;
uniform int MinDepth;
uniform int MaxDepth;
uniform float2 Spacing = float2(0.05,0.05);
uniform float2 Center = float2(.5,.5);
static float invAr = Dimensions.x/Dimensions.y;
static float pixelSize = 1 / Dimensions;
const float3 cameraPosition = float3(0.0, 0.0, 10.0);
const float3 cameraDirection = float3(0.0, 0.0, -1.0);
const float3 cameraUp = float3(0.0, 1.0, 0.0);
const float fov = 50.0;
#define PI 3.14
float Sine(float3 uv, float pos, float height, float waves, float add, float thickness)
{
	height /= 2.0;
	waves = PI * waves * 2 * uv.y;
	float curve = height * sin(waves + add);
	return smoothstep(1 - clamp(distance(curve + uv.x, pos), 0, 1), 1, 1.0 - (thickness * 0.002));
}

float intersected(float3 p, float3 cam, float3 dir)
{
	float waves = 4.0;
	float height = 0.2;
	float dist = 0;
	for(int i = 0; i<20; i++)
	{
		float pi = cam + dir * dist;
		float w = waves * dist * 0.2;
		float h = height * dist * 0.2;
		float nearest = Sine(p,0.5,h, w, 2 * Time, 10);
        if(nearest < 0.01)
        {
			return 1;
        }
        dist += nearest;
	}
	return 0;
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	uv += 0.7;
	const float fovx = PI * fov / 360.0;
    float fovy = fovx * Dimensions.y/Dimensions.x;
    float ulen = tan(fovx);
    float vlen = tan(fovy);    
    float2 camUV = uv*2.0 - float2(1.0, 1.0);
    float3 nright = normalize(cross(cameraUp, cameraDirection));
    float3 pixel = cameraPosition + cameraDirection + nright*camUV.x*ulen + cameraUp*camUV.y*vlen;
    pixel.x %= 4;
	pixel.x = distance(pixel.x, 2) / 4;
	float3 rayDirection = normalize(pixel - cameraPosition);

  	float curve = intersected(pixel,cameraPosition, rayDirection);//(uv, 0.5, 0.2, 4, 2 * Time, 1);
  	float3  lineACol = (1 - curve) * float3(1,1,1);

  	return float4(lineACol, 1.0);
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