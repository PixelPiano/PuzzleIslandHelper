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
DECLARE_TEXTURE(text, 0);

float3 hash3(float2 p){
    	float3 q = float3(dot(p,float2(127.1,311.7)), 
			   dot(p,float2(269.5,183.3)), 
			   dot(p,float2(419.2,371.9)));
	return frac(sin(q)*43758.5453);
}
float2 hash2(float2 p){
    	float2 q = float2(dot(p,float2(127.1,311.7)), 
			   dot(p,float2(269.5,183.3)));
	return frac(sin(q)*43758.5453);
}
float hash(float2 p){
    	float q = dot(p,float2(127.1,311.7));
	return frac(sin(q)*43758.5453);
}
float length(float2 pos) {
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 worldPos = (uv * Dimensions) + CamPos;
	float sinX = sin(Time - uv.x * 50 * (1 - Amplitude)) * (1 - Amplitude) * 0.05;
	float ampBoost = step(distance(uv.x,sinX),0.05) * sinX;
	uv.x += sin(Time - uv.y * (50 * (1 - Amplitude))) * (1 - Amplitude) * 0.05;
	uv.y += sinX;
    float4 color = SAMPLE_TEXTURE(text, uv);
	color *= step(hash((uv + 1) * Time),Amplitude) * Amplitude;
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