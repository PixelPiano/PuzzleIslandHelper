#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Center;
uniform float Amplitude;
uniform float Speed = 20;
uniform float Thickness = 0.2;
uniform float Intensity = 50;
uniform float Fade = 0;
uniform bool SolidColor = false;
uniform float Size = 0.01;
float2 Circle(float2 uv, float2 center, float size)
{
    float r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x)*invAr;
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
//THIS ONE IS A REALLY COOL EFFECT
float2 rand3(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY) * 0.004;
}
#define PI 3.14159265358979323846
float spiral(float2 m, float time) {
	float r = length(m / Thickness); //radius
	float a = atan2(m.x, m.y); //angle
	float v = sin(Intensity * (sqrt(r) - (0.02 * a) - ((Speed / 100.) * time))); //above 0 if in the spiral
	if(SolidColor) return step(v, -0.4);
	v /= (Fade * 5.);
	return clamp(v, 0., 1.);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = SAMPLE_TEXTURE(text, uv);

	float2 worldPos = (uv * Dimensions) + CamPos;
    float2 dist = Circle(uv, Center, Size);
    float2 random = rand2(Time);
    float amp = Amplitude;
    float angle = atan2(uv.x,uv.y) * dist * amp;
    float d = Size - dist;  // positive when inside the circle
    if(d >= 0)
    {
		uv -= Center; //reposition the center of the spiral at "Position"
		float2 p = float2(uv.x * (Dimensions.x/Dimensions.y), uv.y); //reverse the screen scaling
		uv += Center;
		float v = spiral(p,Time) * amp; //generate spiral
		if(v > 0) 
		{
			float4 c = SAMPLE_TEXTURE(text, uv + (angle * PI * 2));
			return c;
		}
		float4 newColor = SAMPLE_TEXTURE(text, uv + angle * 10);

        return newColor;
    }
	float thresh = cos(Time) * cos(Time) / 2;
	thresh += 0.05;
	thresh += sin(Time) * sin(Time);
	if(d > -thresh)
	{
		float amount = abs(d) * 100;
		return (smoothstep(d - thresh, d, amount) - smoothstep(d, d + thresh, amount)) *float4(0,0,0,0.2);
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