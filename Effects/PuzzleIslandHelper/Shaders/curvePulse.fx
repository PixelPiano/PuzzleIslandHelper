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

uniform float MaxWidth = 50; //In pixels (max = 320)
uniform float MaxHeight = 50; //In pixels (max = 180)
uniform float2 Center;
uniform bool On;
uniform float Alpha = 0.04;
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
	float4 color = SAMPLE_TEXTURE(text, uv);
	if(!On) return color;
	if(uv.y > Center.y) return color; //In in bottom half
	float width =  (MaxWidth / Dimensions.x);
	float height = (MaxHeight / Dimensions.y);
	float px = pow(uv.x - Center.x,2);
	float py = pow(uv.y - Center.y,2);
	float pw = pow(Amplitude * width,2);
	float ph = pow(Amplitude * height,2);
	float x = px/pw- Center.x;
	float y = py/ph - Center.y;
	if(x + y <= 0.6 || x + y > 1) return color; //If not in half circle

	float clipWidth = pw * 2;
	float clipHeight = ph * 1.7;
	float cx = px / clipWidth;
	float cy = py / clipHeight;
	if(cx + cy > 0.6 && cx + cy < 1) return color; //If in clipped area

	float clipWidth2 = pw * (Amplitude*2.5);
	float clipHeight2 = ph * (Amplitude*2.5);
	float cx2 = px / clipWidth2;
	float cy2 = py / clipHeight2;
	if(cx2 + cy2 > 0.6 && cx2 + cy2 < 1) return color; //If in second clipped area

	float4 newColor = SAMPLE_TEXTURE(text, uv + float2(x*pw,y*ph) * 2);
	newColor.a = Alpha;
	return newColor;
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