#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
float length(float2 pos) {
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);
	float valueRx = (sin(uv.x)/2 + 0.5 / CamPos.x) / (sin(Time)/4 + 0.3);
	float valueBx = (sin(uv.x)/2 + 0.5 / CamPos.x) / (sin(Time)/3 + 0.4);
	float valueGx = (sin(uv.x)/2 + 0.5 / CamPos.x) / (sin(Time)/2 + 0.5);
	float valueRy = (sin(uv.y)/2 + 0.5 / CamPos.y) / (sin(Time)/4 + 0.3);
	float valueBy = (sin(uv.y)/2 + 0.5 / CamPos.y) / (sin(Time)/3 + 0.4);
	float valueGy = (sin(uv.y)/2 + 0.5 / CamPos.y) / (sin(Time)/2 + 0.5);
	//color.a = (cos(Time)/2 + 0.5) / (sin(Time)/2 + 0.5);
	if((uv.x > valueRx && uv.x < CamPos.x + Dimensions.x) &&
	    uv.y > valueRy && uv.y < CamPos.y + Dimensions.y)
	{
		color.b /=0.1;
		color.g /=0.1;
	}
		if((uv.x > valueGx && uv.x < CamPos.x + Dimensions.x)&&
	    uv.y > valueGy && uv.y < CamPos.y + Dimensions.y)
	{
		color.b *=0.6;
		color.r *=0.6;
	}
		if((uv.x > valueBx && uv.x < CamPos.x + Dimensions.x)&&
	    uv.y > valueBy && uv.y < CamPos.y + Dimensions.y)
	{
		color.r *=cos(color.b);
		color.g *=sin(color.r);
	}
    return color;
}

void SpriteVertexShader(inout float4 color    : COLOR0,
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