#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Spectrum[512];
uniform int WindowSize;
uniform float2 Position;
uniform float Width;
uniform float Height;

bool InRect(float2 check, float2 topLeft, float width, float height)
{
    return check.x > topLeft.x && check.x < topLeft.x + width
       && check.y > topLeft.y && check.y < topLeft.y + height;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
    float amplitude = 1;
    float4 color = SAMPLE_TEXTURE(text, uv);
    if(InRect(worldPos, Position, Width, Height))
    {
        int ybase = Position.y + Height;
        int xpos = (int)(worldPos.x - Position.x);
        if(worldPos.y > ybase - Spectrum[xpos])
        {
            return float4(1,1,1,1);
        }
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