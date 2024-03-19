#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
uniform int Rings = 4;
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}

bool PovPipe(float2 uv,float scalar, float t, float border)
{   
    float a = scalar * t;
    float b = scalar * t;
    float ax = a * a;
    float result = (pow(uv.x,2)/ ax) + (pow(uv.y - b,2) / ax);
    return result < 1 && result > 1 - border;
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float invAr = Dimensions.x / Dimensions.y;
    float2 pos = float2((0.5 - uv.x) * invAr, 1.5 - uv.y);
    float scalar = 1.5;
    float border = 0.12;
    float denom, dx, dy, result;
    float2 scale;
    //Rings = 4

    float time = Time * .5;
    for(int i = 0; i<Rings; i++)
    {
        //float add = amps[i];
        float add = (1.0/Rings) * i;
        scale = scalar * ((time + add) % 1);
        denom = pow(scale, 2);
        dx = pow(pos.x, 2);
        dy = pow(pos.y - scale.y, 2);
        result = (dx + dy) / denom;
        if(result < 1 && result > 1 - border)
        {
            float darken = pow(result,10);
            return float4(1-darken,1-darken,0,1);
        }
    }
    int rings = 20;
    float midRingThickness = 0.005;
    for(int i = 0; i<rings; i++)
    {
        //float add = amps[i];
        float add = (1.0/(rings)) * i;
        scale = scalar * ((time + add) % 1);
        denom = pow(scale, 2);
        dx = pow(pos.x, 2);
        dy = pow(pos.y - scale, 2);
        result = (dx + dy) / denom;
        if(result < 1 && result > 1 - midRingThickness)
        {
            float darken = pow(result,10);
            float darken2 = distance(uv, float2(0.5,1));
            return float4(darken2,darken2,0,1);
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