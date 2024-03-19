#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
uniform int Rings = 4;
uniform float ShiftLength = 0.05;
uniform float ShiftGap = 0.1;
uniform float ShiftDistance = 0.005;
uniform int State = 1;
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}


float random(float2 st)
{
    return frac(sin(dot(st, float2(1.0,113.0)))*43758.5453123);
}
float2 Wiggle(float2 uv, float length, float gap, float peak)
{
    float foo = uv.y % gap;
    float xShift = 0;
    if(foo <= length* 2)
    {
        float p = (foo % length) / length;
        xShift = (-1 + (foo <= length) * 2) * (p < 0.5 ? p : (0.5 - (p - 0.5))) * 2;
    }
    float shiftAmount = xShift * peak;
    return uv - float2(shiftAmount,0);
}
float2 ScreenTear(float2 uv, float gap, float offset, bool vertical)
{
    float invAr = Dimensions.x / Dimensions.y;
    float foo = vertical ? uv.x : uv.y;
    int sign = (int)(foo / gap) % 2 == 0 ? -1 : 1;
    float tear = sign * offset;
    return uv + float2(vertical ? 0 : tear, vertical ? tear * invAr : 0); 
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    uv = Wiggle(uv,ShiftLength, ShiftGap, ShiftDistance);
    float addition = uv.x + uv.y * Dimensions.x;
    float adjust = 0;// random((float2)addition) / 78;
    float invAr = Dimensions.x / Dimensions.y;
    float2 pos = float2((0.5 - uv.x ) * (invAr + adjust), 1.5 - uv.y + adjust);
    pos = ScreenTear(pos, 0.4,0.2,false);

    //pos = ScreenTear(pos, 0.4, 0.2, true);
    //pos = ScreenTear(pos, 0.4, 0.2, false);
    float scalar = 1.5, border = 0.12, dx = pow(pos.x,2), time = Time * .2;
    float denom, dy, result, mult;
    float2 scale;
    bool foundColor = false;
    float2 bounds = float2(clamp(uv.x,0.3,0.7),uv.y);
    for(int i = 0; i<Rings; i++)
    {
        scale = scalar * ((time + ((1.0/Rings) * i)) % 1);
        dy = pow(pos.y - scale.y,2);
        denom = pow(scale,2);
        result = (dx + dy) / denom;
        if(result < 1 && result > 1 - border)
        {
            mult = 1 - pow(result,10) - pow(distance(bounds, float2(0.5,0.5 - scale.y/8)),2);
            foundColor = true;
            break;
        }
    }
    if(!foundColor)
    {
        int rings = 20;
        float midRingThickness = 0.01;
        float darken2 = 1 - distance(uv, float2(0.5,0));
        for(int i = 0; i<rings; i++)
        {
            float add = (1.0/(rings)) * i;
            scale = scalar * ((time + add) % 1);
            dy = pow(pos.y - scale.y,2);
            denom = pow(scale,2);
            result = (dx + dy) / denom;
            if(result < 1 && result > 1 - midRingThickness)
            {
                mult = 1 - distance(bounds, float2(0.5,0));
                break;
            }
        }
    }
    return float4(float3(0.5,0.5,0.5) * mult, 1);
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