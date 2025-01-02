#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define NUM_LAYERS 120.
#define ITER 100
uniform float4x4 World;
uniform float Time;
uniform float2 Dimensions;
uniform float2 CamPos;
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
    float4 origColor : COLOR1;
};
float4 tex(float3 p)
{
    float t = Time+78.;
    float4 o = float4(p.xyz,3.*sin(t*.1));
    float4 dec = float4 (1.,.9,.1,.15) + float4(.06*cos(t*.1),0,0,.14*cos(t*.23));
    for (int i=0 ; i++ < ITER;) o.xzyw = abs(o/dot(o,o)- dec);
    return o;
}
VertexShaderOutput _VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.origColor = input.Color;
    output.color = input.Color;
    output.position = mul(input.Position, World);
    output.position.z = 0;
    return output;
}
DECLARE_TEXTURE(text, 0);
float4 _PixelShader(VertexShaderOutput input) : COLOR0
{
    float4 c = input.color;
    c.g += (sin((input.position.x + CamPos.x)/20) + 1)/6;
    return c * c.a;
    //return tex(input.color.rgb  + float3((input.position.xy + (sin(Time) + 1) / 2 * 0.005 ), input.color.r/input.color.b) * 0.1);
}

technique houseTechnique
{
    pass Main
    {
        VertexShader = compile vs_3_0 _VertexShader();
        PixelShader = compile ps_3_0 _PixelShader();
    }
};