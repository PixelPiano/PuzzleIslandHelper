#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new Vector2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Area = 0.02;

// unsigned round box
float udRoundBox( float3 p, float3 b, float r )
{
  return length(max(abs(p)-b,0.0))-r;
}

// substracts shape d1 from shape d2
float opS( float d1, float d2 )
{
    return max(-d1,d2);
}

// to get the border of a udRoundBox, simply substract a smaller udRoundBox !
float udRoundBoxBorder( float3 p, float3 b, float r, float borderFactor )
{
  return opS(udRoundBox(p, b*borderFactor, r), udRoundBox(p, b, r));
}
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
	float radius = 40;
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);
	float2 center = float2(0.5,0.5);


	// get screen pos in [0-1]
    // we want a 1:1 aspect ratio, this is why we use iResolution.yy
	
    // box setup
    float3 boxPosition = float3(0.5,0.5,0.0);
    float3 boxSize = float3(0.45,0.47,0.0);
    float boxRounding = 0.05;
    
    // render the box

	float a = 0.02;
	float offsetX = uv.x < 0.5 ? a : -a;
	float offsetY = uv.y < 0.5 ? -a : a;
    float3 curPosition = float3(uv.x + offsetX,uv.y + offsetY, 0.0);    
    float dist = udRoundBoxBorder(curPosition - boxPosition, boxSize, boxRounding, 0.9);    
    float THRESHOLD = 0.02;
    if (dist <= THRESHOLD){
		color.rgb = float3(0, 0, 0);
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