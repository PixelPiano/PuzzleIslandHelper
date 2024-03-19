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
uniform float Size = 0.3;
uniform float Max = 8;
uniform int Rings = 8;
uniform float Mult[4] = {0,1,1,3};
uniform int Div[4] = {1,2,4,4};
uniform float Threshold = 0.6;
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

bool InCircle(float2 uv, float2 center, float size)
{
    float circle = Circle(uv, center, size);
    return circle < 0;
}
bool InRing(float2 uv, float2 center, float size, float2 thickness)
{
    float d = Circle(uv,center, size) + thickness * 2;
    return d > thickness && d < thickness * 2;
}
bool InRing(float2 uv, float amplitude, float thickness)
{
    return InRing(uv, Center, amplitude * (amplitude / Max),thickness * (amplitude / Max));
}
bool InOval(float2 uv, float width, float height)
{
    uv.x -= 0.5;
    uv.y -= 0.5;
    float x = uv.x * uv.x;
    float y = uv.y * uv.y;
    return (x/(width/2)) + (y/(height/2)) < 1;
}
bool PovPipe(float2 uv, float t)
{
    //y = uv.y
    //x = uv.x
    float a = t;
    float b = 2 * t;
    float ax = a * a;
    float result = (pow(uv.x,2)/ ax) + (pow(uv.y - b,2) / ax);
    return result < 1;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    //float2 screenPos = uv * Dimensions;
	//float2 worldPos = screenPos + CamPos;
    float amplitude = Time % (Max/4);
    float yOffset = sin(Time /Max)/4;
    //uv = mul(uv, TransformMatrix) * 100; //uncomment for cool stuff
    for(int i = 0; i<Rings; i++)
    {
        float size = amplitude;
        float added = ((Max/Rings) * i);
        float2 newuv = float2(uv.x, uv.y + yOffset);
        if(size + added > Threshold && InRing(newuv, size + added, 0.2))
        {   
            float alpha = min(1, (added + size - Threshold - 0.5/ Threshold));
            return float4(alpha,0,0, 1);
        }
    }
    return float4(0,0,0,1);

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