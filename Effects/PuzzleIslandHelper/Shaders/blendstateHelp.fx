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

float4 DestinationAlpha(float4 s, float4 d)
{
	return float4(s.r * d.a, s.g * d.a, s.b * d.a, s.a * d.a);
}
float4 DestinationColor(float4 s, float4 d)
{
	return float4(s.r * d.r, s.g * d.g, s.b * d.b, s.a * d.a);
}
float4 InverseDestinationAlpha(float4 s, float4 d)
{
	float mult = 1 - d.a;
	return float4(s.r * mult, s.g * mult, s.b * mult, s.a * mult);
}
float4 InverseDestinationColor(float4 s, float4 d)
{
	return float4(s.r * (1-d.r), s.g * (1-d.g), s.b * (1-d.b), s.a * (1-d.a));
}
float4 InverseSourceColor(float4 s, float4 d)
{
	return float4(d.r * (1-s.r), d.g * (1-s.g), d.b * (1-s.b), d.a * (1-s.a));
}
float4 SourceAlpha(float4 s, float4 d)
{
	return float4(d.r * s.a, d.g * s.a, d.b * s.a, d.a * s.a);
}
float4 SourceColor(float4 s, float4 d)
{
	return float4(d.r * s.r, d.g * s.g, d.b * s.b, d.a * s.a);
}
float4 InverseSourceAlpha(float4 s, float4 d)
{
	float mult = 1 - s.a;
	return float4(d.r * mult, d.g * mult, d.b * mult, d.a * mult);
}
float4 getHelpColor(float2 uv)
{
	float4 color = float4(0,0,0,1);
	if(uv.x < 0.3 && uv.y < 0.3)
	{
		if(uv.x > 0.2 || uv.y > 0.2)
		{
			return color = float4(1,1,1,1);
		}
		else
		{
			return color = float4(0,0,0,1);
		}
	}
	return color;
}


DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{

	float2 worldPos = (uv * Dimensions) + CamPos;
	float4 origColor = SAMPLE_TEXTURE(text, uv);
    //float4 color = SAMPLE_TEXTURE(text, uv);
	float4 color = getHelpColor(uv);
	float4 buffer = float4(1,1,1,1);

	float4 src = (float4)1;//InverseSourceAlpha(color,buffer);
	float4 dest = (float4)1;//InverseDestinationAlpha(color,buffer);
	return (color * src) + (buffer * dest);
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