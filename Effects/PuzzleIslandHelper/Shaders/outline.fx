#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float2 Dimensions;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    float2 pixel = 1 / Dimensions;
    float4 color = SAMPLE_TEXTURE(text,uv);
    float left = SAMPLE_TEXTURE(text, float2(uv.x - pixel.x, uv.y)).a;
    float right = SAMPLE_TEXTURE(text, float2(uv.x + pixel.x, uv.y)).a;
    float up = SAMPLE_TEXTURE(text, float2(uv.x, uv.y - pixel.y)).a;
    float down = SAMPLE_TEXTURE(text, float2(uv.x, uv.y + pixel.y)).a;
    return lerp(0, color, step(left * right * up * down,0));
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