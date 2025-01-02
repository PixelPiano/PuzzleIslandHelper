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
uniform bool combo[4];
DECLARE_TEXTURE(text, 0);
#define NUM_LAYERS 120.
#define ITER 100
//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}
float2 hashOld22( float2 p )
{
	p = float2( dot(p,float2(127.1,311.7)),
			  dot(p,float2(269.5,183.3)));

	return abs(frac(sin(p)*43758.5453123));
}
float4 PS_Specialist(float2 start,float size, float4 inColor, float2 uv, int xBlocks)
{
    uv.x += max(0, uv.y - start.y);
    int mult = step(start.y,uv.y) * step(uv.y, start.y + size) * step(start.x,uv.x) * step(0.02,(uv -start) % size) * step(uv.x, start.x + xBlocks * size);
    return lerp(inColor, float4(1,1,1,1),mult);
}

float4 PS_Glitch(float4 inPosition, float4 inColor, float2 uv)
{
    // get the multiplier (based on "chunk")
    float mult = abs(max(minimum, sin(timer + uv.y * amplitude))) * glitch;
    float d = dimensions.x / 3;
    float pixelSize = max(1, floor(16.0 * mult));
    float x = -glitch + rand(float2(mult * dimensions.x, pixelSize)) * (glitch * 2);
    return SAMPLE_TEXTURE(text, float2(uv.x + x * rand(timer % 0.5) * 0.1, uv.y));
}

float2 PS_Chop(float4 inPosition, float4 inColor, float2 uv)
{
        float mult = abs(max(minimum, sin(timer % 0.6 + uv.y * amplitude))) * glitch;

        float pixelSize = max(1, floor(16.0 * mult));
        float offset = rand(float2(seed, pixelSize));

        float2 org = float2(max(0, min(1, uv.x + (offset * 0.1 - 0.05) * mult)), uv.y);
        float2 size = dimensions.xy / pixelSize;
        float2 xy = floor(org * size) / size + pixelSize / dimensions.xy * 0.5;
        xy.x += (((int)(uv.y / 0.05) % 2) - 1) * 0.1;
        
        return (xy - uv) * mult;
}
float2 PS_ChopY(float4 inPosition, float4 inColor, float2 uv)
{
        float mult = abs(max(minimum, sin(timer + uv.x * amplitude))) * glitch;

        float pixelSize = max(1, floor(16.0 * mult));
        float offset = rand(float2(pixelSize,seed));

        float2 org = float2(uv.x, max(0, min(1, uv.y + (offset * 0.1 - 0.05) * mult)));
        float2 size = dimensions.xy / pixelSize;
        float2 xy = floor(org * size) / size + pixelSize / dimensions.xy * 0.5;
        xy.y += (((int)(uv.x / 0.05) % 2) - 1) * 0.1;

        return (xy - uv) * mult;
}
float4 PS_Obscured(float4 inPosition, float4 inColor, float2 uv)
{

    float mult = abs(max(minimum, sin(timer + uv.x * amplitude))) * glitch;
    float size = rand(float2(timer, seed));

    float2 p = uv;
    float s = min(0.11, max(0, rand(seed) * 0.11));
    float2 square = p % s;
    float2 offset = float2(rand(s),rand(s)) *0.01;
    p *= step(0.04,square);

    float4 orig = SAMPLE_TEXTURE(text,p + offset);
    return lerp(float4(0,0,0,0),orig,float4(step(size * 0.3, uv/size),1,1)*mult);
}
int randomSquare(float4 color, float2 uv, float2 offset, int div)
{
    float x = uv.x;
    float y = uv.y;
    float size = 0.33 / div;
    float xba = 0.33 + size * (int)(abs(rand(timer * seed + offset.x)) *div);
    float xbb = xba + size;
    float yba = 0.33 + size * (int)(abs(rand(timer * seed+10 + offset.y)*div));
    float ybb = yba + size;
    return step(xba, uv.x) * step(uv.x, xbb) * step(yba,uv.y) * step(uv.y,ybb);
}
float4 balls(float2 start,float size, float4 inColor, float2 uv, int xBlocks)
{
    int mult = step(start.y,uv.y) * step(uv.y, start.y + size) * step(start.x,uv.x) * step(0.02,(uv -start) % size) * step(uv.x, start.x + xBlocks * size);
    mult *= step(size,max(0, length((start - uv) % size)));
    return lerp(inColor, float4(1,1,1,1),mult);
}
float4 lines(float4 inColor, float2 uv, float angle)
{
    return lerp(inColor, float4(0.2,1,0.2,inColor.a),step(0.15,(uv.x + uv.y) % 0.2) * step(0.44,uv.x) * step(0.44,uv.y) * step(uv.x,0.55) * step(uv.y,0.55));
}
float4 tex(float3 p)
{
    float t = timer+78.;
    float4 o = float4(p.xyz,3.*sin(t*.1));
    float4 dec = float4 (1.,.9,.1,.15) + float4(.06*cos(t*.1),0,0,.14*cos(t*.23));
    for (int i=0 ; i++ < ITER;) o.xzyw = abs(o/dot(o,o)- dec);
    return o;
}
float4 PS_Artist(float4 color, float2 uv)
{
    color = float4(0,0,0,step(0.33, uv.x) * step(uv.x, 0.66) * step(0.33,uv.y) * step(uv.y, 0.66));
    color *= sign(randomSquare(color, uv, float2(10, 40),3) + randomSquare(color, uv, float2(0, 30),3));
    //color = lines(color, uv, 0.2);
    color += PS_Specialist(float2(0.8, 0.8) * 0.33 + 0.33, rand(seed) * 0.05,color,uv,4);
    color += balls(float2(0.40, 0.40),0.04,color,uv,4);
    return color;
}
float4 Pixel(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : SV_TARGET0
{
    float4 color = float4(0,0,0,0);
    color = PS_Glitch(inPosition, color, uv);
    if(combo[1])
    {
        color = lerp(color,PS_Obscured(inPosition, color, uv + PS_Chop(inPosition, color, uv)),glitch);
    }
    else if(combo[2])
    {
        color = lerp(color,PS_Obscured(inPosition, color, uv + PS_ChopY(inPosition, color, uv)),glitch);
    }
    else if(combo[3])
    {
        color = lerp(color,PS_Obscured(inPosition, color, uv + PS_Chop(inPosition, color, uv)),glitch);
    }
    if(combo[0] && combo[1])
    {
        color.r = lerp(color.r,step(0.1, color.r) * (1 - color.r),glitch);
    }
    else if(combo[1] && combo[2])
    {
        color.g = lerp(color.g,step(0.1, color.g) * (1 - color.g),glitch);
    }
    else if(combo[2] && combo[3])
    {
        color.b = lerp(color.b, step(0.1, color.b) * (1 - color.b),glitch);
    }else if(combo[0] && combo[3])
    {
        color = lerp(color, PS_Obscured(inPosition, float4(0,0,0,1), uv + PS_Chop(inPosition, color, uv)),glitch);
    }
    
    return color;
}
//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique Glitch
{
    pass
    {
        PixelShader = compile ps_3_0 Pixel();
    }
}