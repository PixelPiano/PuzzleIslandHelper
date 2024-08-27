#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude = 0.5;
uniform float2 Center = float2(0.5,0.5);
uniform float Size = 0.1;
uniform float StartFade = 0;
uniform float EndFade = 0.3;
uniform float2 OrigCam;
float2 Circle(float2 uv, float2 center, float size)
{
    float r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x);
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d - size * size;
    return d;
}
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
//THIS ONE IS A REALLY COOL EFFECT
float2 rand3(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY) * 0.004;
}
float4 FuzzColor(float2 uv, float4 color, float4 second, float amount)
{
    float randB = rand2(uv * Time);
    float mult = 1 - amount;
    if(1 - amount >= 0 && randB < (1 - amount) *  mult)
    {
        return lerp(second, float4(0.2,0.2,1,1),amount);
    }
    return lerp(color, float4(0.6,0.6,1,1),amount * 0.5);
    return color;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = SAMPLE_TEXTURE(text, uv);
    float amp = Amplitude;
    float size = Size * amp;
    float start = StartFade * amp;
    float end = EndFade * amp;
    float dist = Circle(uv, Center, size);
    float d = size - dist;  // positive when inside the circle
    if(amp > 0 && dist <= end)
    {
        float amount = (clamp(dist, start, end) -start) / (end - start);
        return FuzzColor(uv, (float4)0,color,amount * 0.9);
    }
    return (float4)0;
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