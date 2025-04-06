#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
//THIS ONE IS A REALLY COOL EFFECT
float2 rand3(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 screenPos = (uv * Dimensions);

    // Time varying pixel color

    float space = 0.04;
    float height = 0.02;
    float gap = 0.01;
    float offset = floor(uv.y / (space + height));
    float dir = -1.0 + step(1., offset % 2) * 2.;
    float r = rand2(float2(offset, offset));
    float width = 0.05 + rand3(float2(offset, offset)) * 0.06;
    float finaloffset = r * width;
    float speed = rand2(float2(offset + 4, offset + 4));
    
    
    float x = (uv.x + finaloffset + (Time * dir / 30.));
	if(x < 0)
	{
		x = 1 + x;
	}
    float blockX = x % (gap + width);
    float blockY = uv.y % (space + height);
    float totalwidth = gap + width;
    float totalheight = space + height;
    float r2 = rand2((float2)(floor(Time / 0.5)));
    float val = 
    //returns 1 if y within a block
    step(space, blockY)
    //returns 1 if x within a block
    * step(gap, blockX)
    //creates the dot effect
    * step(1.,screenPos.x % 2.) * step(1.,screenPos.y % 2.);
    val *= 0.5;

	return float4(val, val, val, 1.0);
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