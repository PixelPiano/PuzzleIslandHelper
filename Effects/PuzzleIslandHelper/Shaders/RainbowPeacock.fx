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
float3 hashOld33( float2 p )
{
	float3 p2 = float3( dot(p,float2(127.1,311.7)),
			  dot(p,float2(269.5,183.3)),
			  dot(p, float2(443.3, 75.1)));

	return abs(frac(sin(p2)*43758.5453123));
}
float3 GetColor(float2 pos, float radius,float length, float fade)
{
	float rot = Time;
	float angle = atan2(pos.y, pos.x) * rot;
	float t = frac((angle + rot) / TAU * 8.0);
	float3 r = hashOld33(pos);
	float x = r.x;
	float y = r.y;
	float z = r.z;
	float mult = step(t, 0.5);
	return float3(x, y, z) * mult;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	float2 invAr = float2(Dimensions.x / Dimensions.y,1);

	float2 pos = (MonsterPos - uv) * invAr;
	float r = 0.04;
	float l = length(pos) - r;
	float fade = smoothstep(r, 0, l);





	return float4(GetColor(pos,r,l,fade), 1);
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