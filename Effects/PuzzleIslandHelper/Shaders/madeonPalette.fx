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
    
    return a + b*cos(6.28318*(c*t+d));
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


DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float2 worldPos = (uv * Dimensions) + CamPos;
    float4 color = SAMPLE_TEXTURE(text, uv);

    uv -= .5;
    uv.x *= Dimensions.x/Dimensions.y;
    float r = 0.4;// radius of circle 
    
    float3 f1 = palette(length(uv)-(Time/100));
    color *= pow(float4(f1, 1.0) * 0.5, f1.z);
    



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