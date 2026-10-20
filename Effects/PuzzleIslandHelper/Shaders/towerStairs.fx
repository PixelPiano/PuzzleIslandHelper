#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

float4x4 World;
float Time;
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.color = float4(1,0,1,1);
    output.position = mul(input.Position, World);
    output.color = lerp(output.color, float4(0,0,0,1),(-output.position.z / 36));
    output.position.z = 0;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return input.color;
}

technique NormalTechnique
{
    pass Base
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader =  compile ps_3_0  PixelShaderFunction();
    }
}