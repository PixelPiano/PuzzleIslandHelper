#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)
static float Lerp = 0;
static bool LerpUp = true;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

DECLARE_TEXTURE(text, 0);

float visibility()
{
	if(Lerp >= 1){
		LerpUp = false;
	}if(Lerp <= 0){
		LerpUp = true;
	}
	Lerp += LerpUp ? 0.1 : -0.1;
	return Lerp;
}

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);
    color.b /= uv.y + length(worldPos) % 1;
	color.r=0;
	color.g=0;

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