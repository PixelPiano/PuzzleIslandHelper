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
    float2 worldPos = (uv * Dimensions) + CamPos;
	   // Calculate CRT TV distortion effect
    float2 dist   = .5 - uv;
    float4 color = SAMPLE_TEXTURE(text, dist);
    float strength  = 1; // Strength of the CRT TV effect
    float2 neww = float2(0,0);
    neww.x       = (uv.x - dist.y * dist.y * dist.x * strength/(Dimensions.x/Dimensions.y));
    neww.y       = (uv.y - dist.x * dist.x * dist.y * strength);
    
    // Chromatic aberration

    float2 chrom = float2(0,0); // Strength of the color shift
    //chrom.x = sin(Time)*2.0% 0.01;
    chrom.y = 1;
    
    float4 col = float4(0,0,0,0);
    col.r       = SAMPLE_TEXTURE(text, float2(neww.x+chrom.x, neww.y)).r;
    col.g       = SAMPLE_TEXTURE(text, float2(neww.x, neww.y)).g;
    col.b       = SAMPLE_TEXTURE(text, float2(neww.x-chrom.x, neww.y)).b;
    
    //col         = ((neww.x >= 0.0) && (neww.x <= 1.0)) && ((neww.y >= 0.0) && (neww.y <= 1.0)) ? col : float4(0.0,0,0,0);
    
    // Output to screen
    float2 effDist= float2(0.05*dist.x,0.05*dist.y);
    if(neww.x < effDist.x || neww.y < effDist.y || neww.x > 0.95-effDist.x || neww.y>0.95-effDist.y)
    {
        return float4(0,0,0,1);
    }
    color   = float4(col.r,col.g,col.b,1.0);
    
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