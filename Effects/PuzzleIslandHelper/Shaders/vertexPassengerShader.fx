float4x4 World;
uniform float Time;
uniform float4 Color;
uniform int Shift;
#define NUM_LAYERS 120.
#define ITER 100
float4 tex(float3 p)
{
    float t = Time+78.;
    float4 o = float4(p.xyz,3.*sin(t*.1));
    float4 dec = float4 (1.,.9,.1,.15) + float4(.06*cos(t*.1),0,0,.14*cos(t*.23));
    for (int i=0 ; i++ < ITER;) o.xzyw = abs(o/dot(o,o)- dec);
    return o;
}

static float4 lastPosition = float4(0,0,0,0);
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
    float4 lastPosition : TEXCOORD0;
};
VertexShaderOutput _VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.position = mul(input.Position, World);
    output.lastPosition = lastPosition;
    output.color = lerp(input.Color,Color,Shift);
    lastPosition = input.Position;
    return output;
}

float4 _PixelShader(VertexShaderOutput input) : COLOR0
{
    return input.color;
}

technique shader
{
    pass Main
    {
        VertexShader = compile vs_3_0 _VertexShader();
        PixelShader = compile ps_3_0 _PixelShader();
    }
};