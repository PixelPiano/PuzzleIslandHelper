#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)
uniform float2 Offset;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Area = 0.02;

// unsigned round box
float udRoundBox( float3 p, float3 b, float r )
{
  return length(max(abs(p)-b,0.0))-r;
}

// substracts shape d1 from shape d2
float opS( float d1, float d2 )
{
    return max(-d1,d2);
}
float rand(float2 co){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}
float4 plt(float4 t){
    float3 a = float3(t.r, 0, 1);
    float3 b = float3(0.5, t.g, 1);
    float3 c = float3(1.0, 0.0, t.b);
    float3 d = float3(0.263, 0.416, 0.557);
    
    return float4(a + b*sin(6.28318*(c*t+d)),1);
}

// to get the border of a udRoundBox, simply substract a smaller udRoundBox !
float udRoundBoxBorder( float3 p, float3 b, float r, float borderFactor )
{
  return opS(udRoundBox(p, b*borderFactor, r), udRoundBox(p, b, r));
}
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
float PulseRate(float mult, float intensity = 2, float offset = 0.5)
{
	return sin(Time * mult)/intensity + offset;
}

float DreamBorder(float2 uv, float2 center,float2 curve, float2 zoom, float2 area, float speed = 1)
{
	float2 p = uv;
	float distx = abs(center.x - p.x);
	float disty = abs(center.y - p.y);
	float ydir = -1 + step(0, 0.5 - uv.x) * 2;
	float xdir = 1 - step(0, 0.5 - uv.y) * 2;
	float2 dist = float2(distx,disty);
	dist.y += sin(Time * speed * xdir + p.x * curve.x) * zoom.x;
	dist.x += sin(Time * speed * ydir + p.y * curve.y) * zoom.y;
	float2 len = length(dist);
    float lx = 1 - step(0.99- len.x,area.x);
	float ly = 1 - step(0.99 - len.y,area.y);
	return 1 - (lx * ly);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 worldPos = (uv * Dimensions) + CamPos;
	float2 invAr = float2(Dimensions.x / Dimensions.y,1);
	float2 p = uv;
	float s1 = (sin(Time * 0.1 *  0.3) + 1) / 2;
	float s2 = (sin(Time * 0.4 *  0.3) + 1) / 2;
	float s3 = (sin(Time * 0.8 *  0.3) + 1) / 2;
	float s4 = (sin(Time * 0.5 *  0.3) + 1) / 2;
	float s5 = (sin(Time * 0.2 *  0.3) + 1) / 2;
	float s6 = (sin(Time * 0.9 *  0.3) + 1) / 2;
	float s7 = (sin(Time * 1.1 *  0.3) + 1) / 2;

    float a = 5 * 10; //curve intensity
	float zoom = 0.02; //zoom
	float area = 0.48; //total visible game area
	float d = 0.5; //center
	float4 color = SAMPLE_TEXTURE(text, uv);
	float4 b1 = float4(1,0,0,1);
	float4 b2 = float4(0,1,0,1);
	float4 b3 = float4(1,1,1,1);
	float4 b4 = float4(0,0,1,1);
	float4 b5 = float4(1,0,1,1);
	float4 b6 = float4(0,1,1,1);
	float4 b7 = float4(1,1,1,1);
	float l1 = DreamBorder(uv, d, a,	 zoom, area,	              1);
	float l2 = DreamBorder(uv, d, a - 1, zoom, area - 0.02  + s1 * 0.1,1.2);
	float l3 = DreamBorder(uv, d, a - 2, zoom, area - 0.06  + s2 * 0.1,1.4);
	float l4 = DreamBorder(uv, d, a - 3, zoom, area - 0.1   + s3 * 0.1,1.6);
	float l5 = DreamBorder(uv, d, a - 4, zoom, area - 0.14  + s4 * 0.1,1.8);
	float l6 = DreamBorder(uv, d, a - 5, zoom, area - 0.18  + s5 * 0.1, 2);
	float l7 = DreamBorder(uv, d, a - 6, zoom, area - 0.22  + s6 * 0.1,2.3);
	return lerp(color,lerp(b1, lerp(b2, lerp(b3,lerp(b4,lerp(b5,lerp(b6,b7,
	plt(l7)),
	plt(l6)),
	plt(l5)),
	plt(l4)),
	plt(l3)),
	plt(l2)),
	l1 * (0.5 + 0.5 / length(uv - 0.5)) * length(uv - 0.5));
}

void SpriteVertexShader(inout float4 color: COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	float2 offset = float2(Offset.x, -Offset.y);
	position = mul(position, ViewMatrix);
	position = mul(position, TransformMatrix);
    position.xy += ((offset/Dimensions) * 2);
}

technique Shader
{
	pass pass0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}