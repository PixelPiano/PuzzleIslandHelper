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

float2 rand3(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY) * 0.004;
}
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
float Circle(float2 uv, float2 center, float2 size, float dir)
{
	float ra = rand2(uv.x * Time);
	float ra2 = rand3(uv.y * Time);
    float2 r = size + (0.05 * float2(ra,ra2)); //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x);
	float y = (center.y-uv.y);
	float s = sin(x + Time) * 0.2;
	float c = cos(y + Time) * 0.2;
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
	float dist = size - Circle(uv, (float2)0.5, size, s);
	float amount = (sin(Time) + 1)/ 2;
	float4 color = float4(1,0,0,1);
	float alpha = 0;
	bool inHeart = false;
	if(dist >= 0)
	{
		float heartSize = 0.4 + (s * 0.05);
		heartSize -= amp / 8;
		float heartAlpha = (dist - heartSize) / (1 - heartSize);
		if(amp < heartAlpha * 1.5 + 0.7)
		{
			heartSize *= 0.5 + (1 - amp) * 0.5; 
		}
		heartSize += amp / 8;
		if(dist > heartSize)
		{
			inHeart = true;
			alpha = heartAlpha * 2;
			if(amp < heartAlpha * 1.5 + 0.8)
			{
				float mult =min(0.5 + (3 * (1 -amp)),2);
				heartSize *= mult;
				//heartbeat
				alpha *= mult;
			}
			else
			{
				alpha *= 0.8;
			}
		}
		else
		{
			float e = dist * 2.5;
			for(float i = heartSize - 0.05; i<=1;i+=0.20 + s * 0.02)
			{
				if(e >= i && e <= i + 0.2)
				{
					//first half of ring fade
					alpha = (e - i) / 0.2;
					//second half of ring fade
					if(alpha >0.5) alpha = 0.5 - (alpha - 0.5);
					if(alpha > 0)
					{
					}
					
				}
			}
			//ring pulse
			if(dist + 0.4 > amp - 0.1 && dist + 0.4 < amp + 0.1)
			{
				float add = ((dist + 0.2 - (amp - .1)) / -0.05) * 0.2 * (1.6 - dist);
				alpha *= (1 + add);
			}
		}
	}
	if(inHeart)
	{
		if(Time % 0.15 < 0.075)
		{
			float r = rand2(uv * floor(Time * 3));
			alpha *= 0.9 + r * 0.3;
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