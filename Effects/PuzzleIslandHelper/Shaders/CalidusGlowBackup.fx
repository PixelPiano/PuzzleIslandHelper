#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float RingSpace = 0.2;
uniform float Amplitude;
uniform float WidthMult = 1;
uniform float HeightMult = 1;
uniform float SizeAdjust;


float Circle(float2 uv, float2 center, float2 size, float dir)
{
    float2 r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x);
	float y = (center.y-uv.y);
	float s = sin(x + Time) * 0.1;
	float c = cos(y + Time) * 0.1;
	x += c * s * dir;
	y += s * -c * dir;
    float d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
	return d;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{



	float amp = 1 - Time * 0.8 % 1;
	float adjust = sin(Time) * 0.005;
	amp *= amp;
	float size = 0.25 + adjust;
	float s = sin(Time);
	float dist = Circle(uv, (float2)0.5, size, s);
	float d = size - dist;
	float amount = (sin(Time) + 1)/ 2;
	float4 color = float4(1,0,0,1);
	float alpha = 0;

	if(d >= 0)
	{
		float hD = 0.35 + (s * 0.05);
		if(d > hD)
		{
			float heart = (d - hD) / (1 - hD);
			alpha = heart;
			if( amp < heart * 2 + 0.6)
			{
				alpha *= 1.5;
			}
		}
		else
		{
			for(float i = hD - 0.05; i<=1;i+=0.1 + s * 0.02)
			{
				float e = d * 2;
				if(e >= i && e <= i + 0.1)
				{
					float am = (e - i) / 0.1;
					if(am >0.5) am = 0.5 - (am - 0.5);
					alpha = am;
					break;
				}
			}
			if(d > amp-0.1 && d < amp + 0.1)
			{
				alpha *= (1 + ((d - amp) / 0.2) * 0.7 * (1.6-d));
			}
		}
	}
	if(Time % 0.1 < 0.05)
	{
		if(((uv.y + Time) % 0.2) < 0.1)
		{
			alpha *= 0.7;
		}
	}

	return color * alpha;
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