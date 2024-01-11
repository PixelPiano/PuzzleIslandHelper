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

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
	float4 origColor = SAMPLE_TEXTURE(text, uv);
	float time = Time + 2000;
    float2 offset1 = float2(sqrt(sin(pow(time/2 + uv.y * (5.0 % uv.x),2)) * 4) * Amplitude, 0);
    float2 offset2 = float2(tan(time * 1 + uv.y * 15500000.0) * Amplitude, offset1.x * 1.5);
    float2 offset3 = float2(cos(time * offset2.x + (uv.y/uv.x)) * Amplitude,0);
    float4 color = SAMPLE_TEXTURE(text, uv + (offset1 * 1.6 + offset2)/2 - offset3 - float2(Amplitude,0));
    
    return color;
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