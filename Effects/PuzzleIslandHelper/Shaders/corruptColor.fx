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
static bool state;
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
float PulseRate(float mult, float intensity = 2, float offset = 0.5)
{
	return sin(Time * mult)/intensity + offset;
}

float Circle(
    float2 uv, // uv coordinates 
    float r, // the radius of the circle
    float blur,  // the blur quantity;
    float2 p // to fixed position?
    ){

    float d = length(uv - p);
    float c = smoothstep(r, r-blur, d);
    return c;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
    float amplitude = 1;
        float4 color = SAMPLE_TEXTURE(text, uv);
    float a = uv.y;
    float b = Time;
    float x = sin(((color.r + color.b + color.g)/3) / 4);
    float offset = a*sin(b*x) * amplitude;
    color = SAMPLE_TEXTURE(text, float2(offset,offset));

    color*=Circle(uv, 4, 2, float2(0.5,0.5));
         if(color.r>0 || color.g > 0){
        color.r*=2;
        color.g = 0.1;
    }else{
        return float4(0,0,0,0);
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