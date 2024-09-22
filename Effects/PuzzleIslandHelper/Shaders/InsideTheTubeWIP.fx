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
uniform float4 BackColor;
uniform float4 FrontColor;
uniform int Layers;
uniform int MinSize;
uniform int MaxSize;
uniform int MinDepth;
uniform int MaxDepth;
uniform float2 Spacing;
uniform float2 Center = float2(.5,.5);
bool InSquare(float2 uv, float2 center, float size)
{
	float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x) * invAr;
	float y = (center.y-uv.y);
	return max(abs(x), abs(y)) - (size / 2.0) <= 0;
}
bool InSquareField(float2 uv, float2 spacing, float size)
{
	spacing.y *= (Dimensions.x/Dimensions.y);
	return InSquare(uv % spacing, spacing / 2., size);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float pixel = 1 / Dimensions;
	float sine = 1;//(sin(Time) + 1) / 2.;
	float minSize = 8 * pixel;
	float maxSize = 12 * pixel;
	for(int i = 0; i<4; i++)
	{
		float amount = (float)i / 4.0;
		float2 offset = normalize(Center - uv);
		float2 pos = uv + (offset * (1 - amount) * sine);

		if(InSquareField(pos, Spacing, lerp(minSize, maxSize, amount)))
		{
			return lerp(float4(1,0,0,1),float4(1,1,1,1),1-amount);
		}
	}
	
    return float4(0,0,0,0);
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