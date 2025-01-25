#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
texture2D atlas_texture;
sampler2D atlas_sampler = sampler_state
{
    Texture = <atlas_texture>;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
texture2D color_texture;
sampler2D color_sampler = sampler_state
{
    Texture = <color_texture>;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
uniform float Time; // level.TimeActive
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Scroll;
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float x = (uv.x + Scroll.x) % 1;
    float y = (uv.y + Scroll.y) % 1;
    if(x < 0)
    {
        x = 1 + x;
    }
    if(y < 0)
    {
        y = 1 + y;
    }
    float4 color = tex2D(atlas_sampler, float2(x, y));
    return color;
    return tex2D(atlas_sampler, uv) * color;
    return SAMPLE_TEXTURE(text, float2(x,y));
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