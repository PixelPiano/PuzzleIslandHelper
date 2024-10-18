#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define PI 3.14
uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float2 Position;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
#define M_PI 3.14159265358979323846
#define MOE 0.6
float hashOld22( float2 p )
{
	p = float2( dot(p,float2(127.1,311.7)),
			  dot(p,float2(269.5,183.3)));

	return frac(sin(p)*43758.5453123);
}
float randomize(float val, float2 seed)
{
	float r =hashOld22(seed) * val;
	return smoothstep(0, val, r * val);
}
int randBin(float2 seed)
{
	return step(0.5, hashOld22(seed));
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	float2 worldPos = CamPos + (uv * Dimensions);
	//float rate = 0.04;
	float t =Time;// (Time - Time % rate) / rate;
	float2 ang = (uv  - 0.5);
	float2 size = (float2)(1 + MOE) - Amplitude * (MOE + 0.05); //size of the circle
	float radius = length(ang/size) * 2;
	float rand = randomize(radius, uv * t * hashOld22(uv * t)); //random chance of pixel being visible
	float3 col = (float3)lerp(0.4, 0, randBin(uv * t * rand)); //randomly choose black or white
	float alpha = smoothstep(0.95, 1, rand) * 0.5;
	float4 color = float4(col * alpha, alpha);
	float vignette = radius * Amplitude;
	color = lerp(color, float4((float3)0, 0.6), vignette * 0.55) * SAMPLE_TEXTURE(text, uv);
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