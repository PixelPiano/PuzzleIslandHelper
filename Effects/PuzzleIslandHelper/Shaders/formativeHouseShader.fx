#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
float hashOld( float2 p )
{
	float p2 = float(dot(p,float2(127.1,311.7)));

	return abs(frac(sin(p2)*43758.5453123));
}
float3 hashOld33( float2 p )
{
	float3 p2 = float3( dot(p,float2(127.5,311.7)),
			  dot(p,float2(269.5,183.3)),
			  dot(p, float2(443.3, 75.1)));

	return abs(frac(sin(p2)*43758.5453123));
}
float4x4 World;
uniform float Time;
struct VertexShaderInput
{
    //float Multiplier : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Multiplier : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
};
VertexShaderOutput doCoolWavyEffect(VertexShaderOutput output, VertexShaderInput input)
{
    float3 offset = hashOld33(input.Position.xy);
    float sinvalue = sin((Time * (0.1 + offset.x) * (0.1 + offset.z)) + offset.y);
    output.position.z += sin(Time + input.Position.x) * 2;
    output.position.y += (hashOld(input.Position.xy) * sinvalue) * 0.1;
    output.color -= (float4)output.position.z * 0.3;
    output.position.x += sin(input.Position.y) * 0.05;
    return output;
}
VertexShaderOutput _VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    float3 offset = hashOld33(input.Position.xy);
    float sinvalue = sin((Time * (0.1 + offset.x) * (0.1 + offset.z)) + offset.y);
    output.position = mul(input.Position, World);
        output.position.y += (hashOld(input.Position.xy) * sinvalue) * 0.1 * input.Multiplier.x;
    output.color = input.Color;
    return output;
}

float4 _PixelShader(VertexShaderOutput input) : COLOR0
{
    return input.color;
}

technique houseTechnique
{
    pass Main
    {
        VertexShader = compile vs_3_0 _VertexShader();
        PixelShader = compile ps_3_0 _PixelShader();
    }
};