#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new float2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;


float rand(float2 co){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = SAMPLE_TEXTURE(text, uv);
    
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