#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

float4x4 World;
float Push;
float Time;
float MaxZ;
float MinZ;
float Width;
float Height;
float3 Offset;
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
    output.color = input.Color;
    output.position = mul(input.Position, World);
    float z = -clamp(output.position.z, (-Height / 2.0), 0);
    //float dist = (MaxZ - MinZ);
    //float amount = 1 - ((z + dist / 2) / dist);
    float amount = pow(abs(z / (Height / 2.0)), 0.8);
    output.color = lerp(output.color, float4(0,0,0,0),amount);
    output.position +=  mul(float4(Offset.xyz,0), World);
    output.position.z = 0;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //return sin(input.color / Time) + (cos(input.color) * cos(Time + input.position.x)) / 10; //cool pattern #1
    //return sin(input.color / Time) + (cos(input.color) * cos(Time + input.position.x + input.position.y)) / 10; //cool pattern #2
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