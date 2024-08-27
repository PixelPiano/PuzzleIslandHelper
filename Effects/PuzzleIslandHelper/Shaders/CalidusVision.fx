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
uniform float2 PlayerCenter;


float Circle(float2 uv, float2 center, float2 size)
{
    float2 r = size; //define the radius of the circle
	float x = (center.x-uv.x);
	float y = (center.y-uv.y);
    float d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
	return d;
}
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float4 cache = SAMPLE_TEXTURE(text, uv);
	float4 color = cache;
	float2 pixel = (1/Dimensions);
	float2 pcuv = (PlayerCenter - CamPos) / Dimensions;
    float invAr = Dimensions.x / Dimensions.y;
	float x = (pcuv.x-uv.x)*invAr;;
	float y = (pcuv.y-uv.y);
    float d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
	d = d * 2;
	float alpha = 0;
	if(0.1 - (d - 0.1) >= 0)
	{
		alpha = (d/0.1) * 0.6;
		color = smoothstep(color, (float4)1,alpha);
	}
	else
	{
		alpha = 1;
		color = (float4)1;
	}
	float add = cache.r + cache.g + cache.b;
	if(add < 1.5)
	{
		if(alpha == 1)
		else
		{
			
		}
	}
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