#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define HALF_MAX 1073741823
//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

float2 dimensions;
float amplitude;
float minimum;
float glitch;
float timer;
float seed;

DECLARE_TEXTURE(text, 0);

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float4 PS_Glitch(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    // get the multiplier (based on "chunk")
    float mult = abs(max(minimum, sin(timer + uv.y * amplitude))) * glitch;
    float d = dimensions.x / 3;
    float pixelSize = max(1, floor(16.0 * mult));
    float x = -glitch + rand(float2(mult * dimensions.x, pixelSize)) * (glitch * 2);
    return SAMPLE_TEXTURE(text, float2(uv.x + x * 0.3, uv.y));
}
float4 PS_Chop(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
                // get the multiplier (based on "chunk")
        float mult = abs(max(minimum, sin(timer + uv.y * amplitude))) * glitch;

        float pixelSize = max(1, floor(16.0 * mult));
        float offset = rand(float2(seed, pixelSize));

        float2 org = float2(max(0, min(1, uv.x + (offset * 0.1 - 0.05) * mult)), uv.y);
        float2 size = dimensions.xy / pixelSize;
        float2 xy = floor(org * size) / size + pixelSize / dimensions.xy * 0.5;

        // grab the color
        float4 color = SAMPLE_TEXTURE(text, xy);
        return color + float4(offset, (offset + 0.34) % 1, (offset + 0.66) % 1, 1) * color.a * 0.25 * mult;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique Glitch
{
    pass
    {
        PixelShader = compile ps_3_0 PS_Glitch();
    }
    pass
    {
        PixelShader = compile ps_3_0 PS_Chop();
    }
}