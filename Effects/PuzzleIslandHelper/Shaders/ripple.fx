#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Center = float2(0.5,0.5);
uniform float Speed = 0.035;
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
float PulseRate(float mult, float intensity = 2, float offset = 0.5)
{
	return sin(Time * mult)/intensity + offset;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);


    float invAr = Dimensions.y / Dimensions.x;
		
   
    float4 texcol = float4(0,0,0,0);
			
	float x = (Center.x-uv.x);
	float y = (Center.y-uv.y) *invAr;
	
	//float r = -sqrt(x*x + y*y); //uncoment this line to symmetric ripples
	float r = -(x*x + y*y);
	float z = 1.0 + 0.5*sin((r+Time*Speed)/0.013);
	
	float4 col = SAMPLE_TEXTURE(text,  uv * z);
	float pulse = 1 + PulseRate(2,10);
	float4 realCol = float4(col.r / pulse ,col.g / pulse,col.b / pulse,0);
	return realCol * 0.5 * PulseRate(2);
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