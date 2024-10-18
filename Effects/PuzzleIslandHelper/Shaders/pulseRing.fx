
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
uniform int Rings = 2;
float2 Circle(float2 uv, float2 center, float2 size)
{
    float2 r = size; //define the radius of the circle
	  float x = (center.x-uv.x);
	  float y = (center.y-uv.y);
    x %= 1.0 / Rings;
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
float2 Ring(float2 uv, float2 center, float2 size, float2 thickness)
{
    float2 d = Circle(uv,center, size) + thickness * 2;
    return step(thickness, d) * step(d, thickness * 2);
}
DECLARE_TEXTURE(text, 0);

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{  
  float2 size = float2(0.2 + 0.3* Amplitude,sin(0.3* Amplitude));
  float circle = Ring(uv, 0.5, size, 0.2) * (1 - Amplitude);
  return float4((float3)(circle), clamp(0, 1, circle));
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