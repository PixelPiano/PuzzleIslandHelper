#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Center = float2(0.5,0.5);
uniform float Speed = 0.035;
uniform float Amplitude;
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
    float amplitude = 1;
    float2 playerPos = CamPos + (Dimensions * float2(0.5, 0.9));
    float range = 0.3;
    int r = 30;
    float4 color = SAMPLE_TEXTURE(text, uv);

    if(worldPos.x < playerPos.x - r || worldPos.x > playerPos.x + r ||
       worldPos.y < playerPos.y - r || worldPos.y > playerPos.y + r)
       {
            return float4(0,0,0,1);
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