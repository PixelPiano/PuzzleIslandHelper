#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

float4x4 World;
float Push;
float Time;
float MaxZ;
float MinZ;
float MaxShade;
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
    float Z : FOG;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    float maxZ = 30;

    VertexShaderOutput output;
    output.color = input.Color;
    output.position = mul(input.Position, World);
    output.Z = clamp(output.position.z, -MaxZ, MaxZ);
    //if(output.Z > 0) output.color = lerp(output.color, float4(1,0,0,1),output.Z / MaxZ);
    //if(output.Z < 0) output.color = lerp(output.color, float4(0,1,0,1),output.Z / MinZ);

    float shadeAbove = (1 - (output.Z / MaxZ)) * 0.5;
    float shadeBelow = 0.5 + (output.Z / MinZ) * 0.5;
    float shadeAmount = lerp(shadeBelow, shadeAbove, step(output.Z, 0));

    //output.color = lerp(output.color, float4(0,0,0,1), shadeAmount); 
    output.position.z = 0;
   	//output.position.z = 0;
	//float l = step(0,output.position.z) * min(output.position.z / 160,1);
    //if(output.position.z > 0) output.color = float4(1,0,0,1);

    //else output.color = lerp(input.Color, float4(0,0,0,input.Color.a),l);
	
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