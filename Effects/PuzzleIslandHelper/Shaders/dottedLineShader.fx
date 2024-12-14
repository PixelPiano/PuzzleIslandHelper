#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define E 0.0000001;
uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float4 Color;
uniform float2 Start;
uniform float Thickness;
uniform float2 End;
uniform float Interval;
uniform float Rate;
uniform float Length;
uniform float Space;
uniform float Pixel;
uniform float2 Offset;
uniform float OffsetMult;
float udSegment(float2 p,float2 a,float2 b,float filled, float gap, float offset)
{
    
    float2 ba = b-a;
    float2 pa = p-a;
    float h = clamp(dot(pa,ba)/dot(ba,ba), 0.0, 1.0 );
    
    // Here's where the magic happens
    h -= offset;
    float s = floor(h / (filled + gap)) * (filled + gap);
    h = s + clamp(h - s, gap * 0.5f, gap * 0.5f + filled);
    h += offset;
    return length(pa-h*ba) * Pixel;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 w = (uv * Dimensions) + CamPos + Offset;
    float4 color = SAMPLE_TEXTURE(text, uv);
    
    float2 p = w;
    float2 v1 = Start;
	float2 v2 = End;
    float th =  Thickness * Pixel;
    float sp = Space * Pixel;
    // Additional parameters
    float l = length(v1 - v2);
    float filled = Length / l;
    float gap = th + sp + 0.3*(0.5) / l;
    float offset = (Time * 20 / l);

	float d = udSegment(p, v1, v2, filled, gap, offset) - th;
    
    return lerp(color, Color, 1 - sign(d));
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