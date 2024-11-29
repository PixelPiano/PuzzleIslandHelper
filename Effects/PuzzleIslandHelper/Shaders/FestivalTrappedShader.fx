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
uniform float2 MonsterPos;
#define M_PI 3.14159265358979323846
#define TAU 6.28318530718
#define MOE 0.6
float hashOld( float2 p )
{
	float p2 = float(dot(p,float2(127.1,311.7)));

	return abs(frac(sin(p2)*43758.5453123));
}
float3 hashOld33( float2 p )
{
	float3 p2 = float3( dot(p,float2(127.1,311.7)),
			  dot(p,float2(269.5,183.3)),
			  dot(p, float2(443.3, 75.1)));

	return abs(frac(sin(p2)*43758.5453123));
}
float lenSin(float x, float y, float size)
{
	float fny;
    float fny1 = sin(x*20.0 + Time * 0.8)*0.4 + 0.5;
	fny = (step(fny1 - size, y) * step(y, fny1 + size));
	return fny;
}
float3 GetColor(float4 color, float2 pos, float radius,float length,float2 dist, float s,float fade)
{
	float angle = atan2(pos.y,pos.x);
	float ff = (sin(length * 8 - Time) + 1) / 2;
	float t = frac((angle) / TAU * 8.0);
	float3 r = hashOld33(pos);
	float height = 0.2;
	float upper = step((0.5 -height) - (1 -ff) / 4, t);
	float lower = step(t, (0.5 + height) + ff / 4);
	float rand = step(r * ff - 0.5,0);
	float inside = upper * lower;

	//float x = pow(r.x * angle, (0.5 - height) - (1 - ff) / 4);
	//float y = pow(r.y * angle, (0.5 + height) + ff / 4);
	//float z = pow(r.z * angle, ff * angle);
	return float3(r) * inside;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	float2 invAr = float2(Dimensions.x / Dimensions.y,1);

	float2 pos = (MonsterPos - uv) * invAr;
	float r = 0.04;
	float l = length(pos) - r;
	float2 dist = float2(length(pos.x) - r, length(pos.y) - r);
	float fade = smoothstep(r, 0, l);
	float size = 0.1;
    float fny;
    float fny1 = sin(uv.x*20.0 + Time * 0.8)*0.4;
	fny = (step(fny1 - size, uv.y) * step(uv.y, fny1 + size));

    //return float4(fny,fny,fny,1);




	return float4(GetColor(SAMPLE_TEXTURE(text, uv),pos,r,l,dist,fny,fade), 1);
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