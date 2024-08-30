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
float CircleLerp(float dist, float size)
{
	return max((size - (dist - size)) / size,0);
}
bool InCircle(float dist, float size)
{
	return ((size) - (dist - size / 2)) >= 0;
}
float4 Monochrome(float rgbAdded, float limit)
{
	if(rgbAdded < limit)
	{
		return float4(0,0,0,1);
	}
	else
	{
		return float4(1,1,1,1);
	}
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float4 cache = SAMPLE_TEXTURE(text, uv);
	float add = cache.r + cache.g + cache.b;
	float4 color = cache;
	float2 pcuv = (PlayerCenter - CamPos) / Dimensions;
    float invAr = Dimensions.x / Dimensions.y;
	float x = (pcuv.x-uv.x)*invAr;;
	float y = (pcuv.y-uv.y);
	float s = sin(uv + Time);
	float c = cos(uv + Time);
	x += (s * c) * 0.01;
	y += (c * -s) * 0.01;
    float d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
	d = d * 2;
	float alpha = 0;
	float4 outside;
	float fadeRadius = 0.1;
	float safeRadius = 0.01;

	float smallLrp = CircleLerp(d, safeRadius);
	float maxLrp = CircleLerp(d, fadeRadius);
	float lrp = 1;
	float4 from = color;
	float4 to = color;
	if(smallLrp > 0)
	{
		lrp = 0.7 - smallLrp * 0.6; //DON'T CHANGE
		//to = float4(1,0,0,1); //change to monochrome
		to = Monochrome(add, 0.297);
	}
	else if(maxLrp > 0)
	{
		//fade out monochrome
		lrp = 0.7 - maxLrp;
		float r2 = rand2(uv * Time / 2);
		from = Monochrome(add, 0.297 + (r2 * 0.65));
		to = float4(0,0,0,1);
	}
	else 
	{
		 float r = rand2(uv * Time);
		 if(r < 0.0125)
		 {
			float ss = (sin(uv + Time / 3.) + 1) / 2.;
			from = Monochrome(add, 0.297);
			from.rgb *= 0.75;
			from.rgb *= ss * 0.38;
		 	if(r < 0.0065) lrp = 0;
		 }
		 else return float4(0,0,0,1);
	}

	return lerp(from, to, lrp);
}
void safe2()
{
		/*if(smallLrp > 0)
	{
		lrp = 1 - smallLrp * 0.6; //DON'T CHANGE
		to = float4(1,0,0,1); //change to monochrome
		
	}
	else if(medLrp> 0)
	{
		//monochrome
		to = float4(0,0,1,1); //change to monochrome
	}
	else if(maxLrp > 0)
	{
		//fade out monochrome
		lrp = 1 - maxLrp;
		from = float4(1,0,0,1); //change to monochrome from medLrp
		to = float4(0,1,1,1); //change to black
	}
	else
	{
		//black
		return float4(0,0,0,1);
	}*/
}
void safe()
{
	
	//if(InCircle(d, safeRadius))
	//{
	//	alpha = (d/safeRadius) * 0.6;
	//}
	//else
	//{
	//	//return float4(1,0,0,1);
	//	alpha = (1-d) / maxRadius;
	//	if(add < 0.295)
	//	{
	//		//alpha = 1;
	//		outside = float4(0,0,0,1);
	//	}
	//	else
	//	{
	//		outside = lerp(float4(0,0,0,1),float4(1,1,1,1),alpha);
	//	}
	//}
	//float overAlpha = 1;
	//if((0.5-d) < 0)
	//{
	//	return float4(0,0,0,1);
	//}
	//return lerp(color, outside, alpha) * alpha * overAlpha;
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