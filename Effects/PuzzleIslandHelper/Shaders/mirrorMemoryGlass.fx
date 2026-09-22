#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float2 Dimensions;
uniform float2 RefDimensions;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float4x4 Rotation;
uniform float Angle;
uniform float AngleLength;
DECLARE_TEXTURE(text, 0);
DECLARE_TEXTURE(ref, 1);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    float4 refColor = SAMPLE_TEXTURE(ref, uv);
    float4 glassColor = SAMPLE_TEXTURE(text, uv);
	float4 color = refColor * glassColor.a;

	float2 toRotate = uv - 0.5;
	float4 rotated = mul(float4(toRotate.x * 2,toRotate.y * 2,0,0), Rotation);
	return lerp(color, float4(0,0,0,glassColor.a),saturate(rotated.z + (abs(rotated.z) * 0.5)));
}

void SpriteVertexShader(inout float4 color: COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, ViewMatrix);
	position = mul(position, TransformMatrix);
    position = mul(position, Rotation);
	position.z = 0;
}
technique Shader
{
	pass pass0
	{
		PixelShader = compile ps_3_0 SpritePixelShader();
		VertexShader = compile vs_3_0 SpriteVertexShader();
	}
}