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
float Circle(float2 uv, float2 center, float2 size)
{
    float2 r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x) * invAr;
	float y = (center.y-uv.y);
    float d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
	return size - d;
}
bool InSquareField(float2 uv, float2 spacing, float size)
{
	//spacing.y *= (Dimensions.x/Dimensions.y);
	return InSquare(uv % spacing, spacing / 2., size);
}
float Equation(float2 uv)
{
	return cos((uv.x+uv.y)/(uv.x*uv.x + uv.y * uv.y));
	//return pow(2, uv.x * 2) - pow((pow(2,uv.x)),2);
	
}
float Equation(float angle)
{
	return pow(sin(angle * 2),3) + pow(cos(angle * 0.5),3);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float pixel = 1 / Dimensions;
	float d = Circle(uv,(float2)0.5,0.5);

	float ease = (sin(Time + Equation(uv)) + 1) / 2.;
	float2 space = Spacing;
	float minSize = 6 * pixel;
	float maxSize = 12 * pixel;
	for(int i = 0; i<4; i++)
	{
		float size = (float)i / 4.0;
		float2 offset = normalize(Center - uv) * space / 2.;
		float2 pos = uv + (offset * (1 - size) * ease);

		if(InSquareField(pos, space, lerp(minSize, maxSize, size)))
		{
			float4 color = lerp(float4(0,0.1,0,1),float4(0,0.5,0,1),1-size);
			return lerp(float4(0,0,0,1),color,0.2 + (clamp(d, 0, 0.5) / 0.5 * 0.4));
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