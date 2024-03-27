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
float2 Circle(float2 uv, float2 center, float2 size)
{
    float2 r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x)*invAr;
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
    float amplitude = sin(Time);
    float4 color = SAMPLE_TEXTURE(text, uv);
	float2 circle = Circle(uv, Center, 0.2);
    bool condition = true;
    if(condition)
    {
        float dist = distance(circle,uv);
        float4 y = float4(0.1,0,0.1,0.4);
        return lerp(color,y, 0.7 / dist + (sin(Time) * 0.2));
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