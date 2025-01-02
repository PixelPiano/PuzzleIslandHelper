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
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
float PulseRate(float mult, float intensity = 2, float offset = 0.5)
{
	return sin(Time * mult)/intensity + offset;
}
float3 palette(float t){
    float3 a = float3(0.5, 0.5, 0.5);
    float3 b = float3(0.5, 0.5, 0.5);
    float3 c = float3(1.0, 1.0, 1.0);
    float3 d = float3(0.263, 0.416, 0.557);
    
    return a + b*cos(6.28318*(c*t+d) * 50);
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


float Band(
    float t,
    float start,
    float end,
    float blur
   
 ){ 
     
     float step1 = smoothstep(start-blur, start+blur, t) ;
     float step2 = smoothstep(end+blur, end-blur, t) ;
     
     return step1*step2;   
 }
 
 float Box(
     float2 uv,
     float left,
     float right,
     float bottom,
     float top, 
     float blur
 ){
     float band1 = Band(uv.x, left, right, blur);
     float band2 = Band(uv.y, bottom, top, blur);
     
     return band2*band1;
 }

DECLARE_TEXTURE(text, 1);
DECLARE_TEXTURE(noise, 0);

uniform float alpha = 1;
uniform float2 noiseDistort = float2(1,1);
uniform float direction = -0.1;
uniform float2 noiseSample = float2(1, 0.5);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float noiseEase = Time * 0.05;
    float2 pixel = float2(1/Dimensions.x, 1/Dimensions.y);
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);
    float4 noiseval = SAMPLE_TEXTURE(noise, uv * noiseSample + float2(0, noiseEase));
    float2 pos = uv + float2(noiseval.g - 0.5, noiseval.b - 0.5) * pixel * noiseDistort * 2;

    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * 0)) * 1.00f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction)) * 0.94f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 2)) * 0.88f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 3)) * 0.82f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 4)) * 0.76f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 5)) * 0.70f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 6)) * 0.64f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 7)) * 0.58f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 8)) * 0.52f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 9)) * 0.46f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 10)) * 0.40f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 11)) * 0.34f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 12)) * 0.28f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 13)) * 0.22f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 14)) * 0.16f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 15)) * 0.10f;
    color += SAMPLE_TEXTURE(text, float2(pos.x, pos.y + pixel.y * direction * 16)) * 0.04f;

    return color * noiseval.r * alpha;
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