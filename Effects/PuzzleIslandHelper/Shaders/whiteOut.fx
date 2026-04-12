#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
uniform float GlowMult = 1;
DECLARE_TEXTURE(text, 0);

float4 addWhiteVal(float4 orig, float2 uv, float xOffset, float yOffset, float mult)
{
	float4 color = SAMPLE_TEXTURE(text,uv + float2(xOffset,yOffset));
	float maxVal = max(color.r,max(color.g,color.b));
	return float4(maxVal * mult, maxVal * mult, maxVal * mult, 0);
}
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 pixel = float2(1/Dimensions.x, 1/Dimensions.y);
	float4 color = SAMPLE_TEXTURE(text, uv);
	float amount = Amplitude * color.a;
	float4 w = lerp(color, float4(1,1,1,color.a),amount);
	float4 light = float4(0,0,0,0);
	float a = 0.1;
	float b = 0.2;
	float c = 0.3;
	float d = 0.4;
	float e = 0.8;
	float a2 = 5 + Amplitude * 4;
	float b2 = 4 + Amplitude * 3;
	float c2 = 3 + Amplitude * 2;
	float d2 = 2 + Amplitude * 1;
	float e2 = 1 + Amplitude * 0.5;
	float f = 1;
	float f2 = 5 + Amplitude * 20;
	if(color.a == 0)
	{
    	light += addWhiteVal(w, uv, 0, pixel.y * a2, a);
		light += addWhiteVal(w, uv, 0, pixel.y * b2, b);
		light += addWhiteVal(w, uv, 0, pixel.y * c2, c);
		light += addWhiteVal(w, uv, 0, pixel.y * d2, d);
		light += addWhiteVal(w, uv, 0, pixel.y * e2, e);
		light += addWhiteVal(w, uv, 0, pixel.y * f2, f) * 0.5;
    	light += addWhiteVal(w, uv, 0, pixel.y * -a2, a);
		light += addWhiteVal(w, uv, 0, pixel.y * -b2, b);
		light += addWhiteVal(w, uv, 0, pixel.y * -c2, c);
		light += addWhiteVal(w, uv, 0, pixel.y * -d2, d);
		light += addWhiteVal(w, uv, 0, pixel.y * -e2, e);
		light += addWhiteVal(w, uv, 0, pixel.y * -f2, f) * 0.5;
		light += addWhiteVal(w, uv, pixel.x * a2, 0,a);
		light += addWhiteVal(w, uv, pixel.x * b2, 0,b);
		light += addWhiteVal(w, uv, pixel.x * c2, 0,c);
		light += addWhiteVal(w, uv, pixel.x * d2, 0,d);
		light += addWhiteVal(w, uv, pixel.x * e2, 0,e);
		light += addWhiteVal(w, uv, pixel.x * f2, 0,f) * 0.5;
    	light += addWhiteVal(w, uv, pixel.x * -a2,0, a);
		light += addWhiteVal(w, uv, pixel.x * -b2,0, b);
		light += addWhiteVal(w, uv, pixel.x * -c2,0, c);
		light += addWhiteVal(w, uv, pixel.x * -d2,0, d);
		light += addWhiteVal(w, uv, pixel.x * -e2,0, e);
		light += addWhiteVal(w, uv, pixel.x * -f2, 0,f) * 0.5;
	}

	return w + (light * GlowMult) * Amplitude;
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