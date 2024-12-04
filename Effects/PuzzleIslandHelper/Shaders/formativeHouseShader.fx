#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

float4x4 World;
uniform float Time;
float Speed;
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
};
VertexShaderOutput _VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.position = mul(input.Position, World) + input.Color;
    output.color = input.Color;

    return output;
}

float4 _PixelShader(VertexShaderOutput input) : COLOR0
{
    return input.color;
}

technique Primitive
{
    pass Main
    {
        VertexShader = compile vs_3_0 _VertexShader();
        PixelShader = compile ps_3_0 _PixelShader();
    }
};