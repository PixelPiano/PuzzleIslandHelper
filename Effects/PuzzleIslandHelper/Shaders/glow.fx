#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new float2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 UP = float2(0,-1);
uniform float2 DOWN = float2(0,1);
uniform float2 LEFT = float2(-1,0);
uniform float2 RIGHT = float2(1,0);
uniform float2 UPRIGHT = float2(1,-1);
uniform float2 UPLEFT = float2(-1,-1);
uniform float2 DOWNRIGHT = float2(1,1);
uniform float2 DOWNLEFT = float2(-1,1);
uniform float Amplitude = 1;


float rand(float2 co){
    return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
}

// How to achieve and control a simple distance glow effect based on several Shadertoy examples
// For 3D see https://www.shadertoy.com/view/7stGWj
// Things to try:
//  * Make the radius and intensity pulse in time or to input
//	* Time varying colour
//  * Animate several points and add the glow values for each to create metaballs

void mainImage( out float4 fragColor, in float2 fragCoord )
{
    
    
}
DECLARE_TEXTURE(text, 0);

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 addColor = float4(0,0,0,0);
    float4 color = SAMPLE_TEXTURE(text, uv);
    float glowPower = 0.;
    float minDist = 500;
    // Convert hex to RGB
    
    // Glow effect
    if((color.r + color.g + color.b) / 3 == 0)
    {
        // Always show glow when fill is enabled
        int size = 5;
        float2 pixel = 1 / Dimensions;
        float4 glowColor = SAMPLE_TEXTURE(text, uv);
        float glow = glowPower;
        float3 c = float3(0,0,0);
        for(int i = 1; i<size + 1; i++)
        {
            float2 p = pixel * i;
            float3 c2 = float3(0,0,0);
            glow = pow(i / minDist, glowPower);
            c2 += SAMPLE_TEXTURE(text, uv + p * UP) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * LEFT) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * RIGHT) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * DOWN) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * UPRIGHT) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * UPLEFT) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * DOWNRIGHT) * glow;
            c2 += SAMPLE_TEXTURE(text, uv + p * DOWNLEFT) * glow;
            c += c2 / 8;
        }
        addColor = float4(c,1);
    }
    float avg = (addColor.r + addColor.g + addColor.b) / 3;
    return color + float4(0,avg,0,avg);
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