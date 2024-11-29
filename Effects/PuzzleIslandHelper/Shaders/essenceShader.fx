#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float2 Position;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;

uniform float2 Radiuses[64];
uniform float2 Positions[64];
uniform int InUse[64];
uniform int BreakOffIndex;
#define M_PI 3.14159265358979323846
#define MOE 0.6
float2 hashOld22( float2 p )
{
	p = float2( dot(p,float2(127.1,311.7)),
			  dot(p,float2(269.5,183.3)));

	return abs(frac(sin(p)*43758.5453123));
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	float amount = 0;
	float2 invAr = float2(Dimensions.x / Dimensions.y,1);
	float2 offset = hashOld22(uv);
	offset = pow(offset, uv);
	uv += offset / 200;
	for(int i = 0; i<32; i++)
	{
		float2 p = Positions[i];
		float2 r = Radiuses[i] * 2;
		float l = length((uv - Positions[i]) * invAr);
		float a = (r - l) * r / 2;
		bool b = l <= r;
		amount += b * a * (78 + a * 200);
	}
	return float4(amount,amount,0,amount);
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