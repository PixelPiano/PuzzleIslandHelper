#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

uniform float Amplitude = 1;
uniform float YScale;
uniform float XScale;
uniform float e = 2.7182818284590452353602874713527;
static const float PI = 3.14159265f;

float rand(float2 co){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}

DECLARE_TEXTURE(text, 0);

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float center = 0.5;
    float strength = Amplitude;
    float size = 1;
    float zoomAdd = 0.1;
    float noiseIntensity = 0.2;
    float2 scaledUV = float2(
        (uv.x - 0.5f) * (1/(1 + (zoomAdd * sin(Amplitude))) - XScale) + 0.5f,
        (uv.y - 0.5f) / ((1 + (zoomAdd * sin(Amplitude))) - YScale) + 0.5f
    );
    //CRT "Shut off" effect
    if(((uv.y< YScale / 2 || uv.y>1-YScale / 2) && (uv.y > 0.505 || uv.y < 0.495)) 
        || (uv.x > 1 - (-XScale / 2) || uv.x < -XScale / 2)) return float4(0,0,0,0);

    float2 dist   = center - scaledUV;
    float2 neww = float2(
        (scaledUV.x - dist.y * dist.y * dist.x * strength),
        (scaledUV.y - dist.x * dist.x * dist.y * strength));

	float r = rand(neww*Time) * pow(distance(neww,(float)0.5), Amplitude) * noiseIntensity;
    if(YScale >= 1)
    {
        r = 0;
    }
    // Output to screen
    float2 effDist= (float2)((1-size) * dist);
    if(neww.x < effDist.x || neww.y < effDist.y || neww.x > size-effDist.x || neww.y>size-effDist.y)
    {
        return float4(0,0,0,1);
    }
    float4 color = SAMPLE_TEXTURE(text, pow(neww,(r * 30) - (pow(Time,0.1))));
    color += (float4)r * Amplitude;
    return lerp(color, (float4)1, min(YScale,1));
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