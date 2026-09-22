#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
float rand(float2 co){return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);}
#define M_PI 3.14159265358979323846
DECLARE_TEXTURE(text, 0);
DECLARE_TEXTURE(map, 1);
DECLARE_TEXTURE(polish, 2);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 offset = SAMPLE_TEXTURE(map, uv);
    uv.x += offset.r;
    uv.y += offset.g;
    float dist = 0;
    float2 angleOffset =float2(cos(offset.b * M_PI * 2) * dist, sin(offset.b * M_PI * 2) *dist);
    float4 p = SAMPLE_TEXTURE(polish,uv + angleOffset);
    float4 color = SAMPLE_TEXTURE(text, uv);
    float f = 1 - (offset.r + offset.g + offset.b) / 3;
    float4 pFactor = float4(f - color.r, f - color.g, f - color.b, p.a);
    return color + (p / pFactor);// * 1 + step(0,offset.a) * 10;
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