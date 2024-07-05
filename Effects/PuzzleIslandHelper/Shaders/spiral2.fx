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
uniform float3 Color = float3(1,0,0);
uniform bool SolidColor = false;

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
	uv -= Center; //reposition the center of the spiral at "Position"
	float2 p = float2(uv.x * (Dimensions.x/Dimensions.y), uv.y); //reverse the screen scaling
	float v = spiral(p,Time); //generate spiral
	if(v > 0) return float4(lerp((float3)v, Color,v),v);
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