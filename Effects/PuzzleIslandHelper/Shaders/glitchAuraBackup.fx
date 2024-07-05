#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Center;
uniform float Amplitude;
uniform float Size;
uniform float Random;
uniform bool Strong;
float2 Circle(float2 uv, float2 center, float size)
{
    float r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x)*invAr;
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
float rand1(in float2 uv)
{
    float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
    return abs(noise.x + noise.y) * 0.5;
}

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
    return float2(noiseX, noiseY) * 0.004;
}
static float dis=.5;
static float width=.1;
static float blur=.1;

#define PI 3.14159265358979323846
#define e  2.71828182845904523

float2x2 rotate2D(float angle){
	return float2x2(cos(angle),-sin(angle),
               sin(angle),cos(angle));
}
float4 LeftBottom(float2 p, float2 pos)
{
        float3 col;
    float3 lineColor=float3(.5,0.2,0.4);
    if((p.x<p.x*.5)&&(p.y<p.y*.5))
    {
        float2 o=p+float2(0.5*pos.x,0.5*pos.y);
        float angle=atan(o);
        float l=length(o);
        float offset=(log(l)+(angle/(2.*PI))*dis);
        float circles=fmod(offset-Time,dis);
        col=(smoothstep(circles-blur,circles,width)-smoothstep(circles,circles+blur,width))*lineColor;
    }
     float hl=smoothstep(0.01,0.03,abs(p.x-p.x*.5));
    float vl=smoothstep(0.01,0.03,abs(p.y-p.y*.5));
    //x×(1−a)+y×a.
    col=lerp(float3(1,1,0),col,hl);
    col=lerp(float3(1,1,0),col,vl);
    
    return float4(col,1.0);
}
float4 LeftTop(float2 p, float2 pos)
{
    float3 col;
    float3 lineColor=float3(.5,0.2,0.4);

        float2 o=pos;
        float angle=atan(o);
        float l=length(o);
        float offset=l+(angle/(2.*PI))*dis;
        float circles=fmod(offset-Time,dis);
        col=(smoothstep(circles-blur,circles,width)-smoothstep(circles,circles+blur,width))*lineColor;
    
     float hl=smoothstep(0.01,0.03,abs(p.x-p.x*.5));
    float vl=smoothstep(0.01,0.03,abs(p.y-p.y*.5));
    //x×(1−a)+y×a.
    col=lerp(float3(1.,1.,0),col,hl);
    col=lerp(float3(1.,1.,0),col,vl);
    
    return float4(col,1.0);
}
float4 RightBottom(float2 p, float2 pos)
{
        float3 col;
    float3 lineColor=float3(.5,0.2,0.4);
   if((p.x>=p.x*.5)&&(p.y<p.y*.5))
    {
     	float2 o=p+float2(-0.5*pos.x,0.5*pos.y);
        float angle=atan(o);
        float l=length(o);
        float offset=(log(l)/log(e*5.)+(angle/(2.*PI))*dis);
        float circles=fmod(offset-Time,dis);
        col=(smoothstep(circles-blur,circles,width)-smoothstep(circles,circles+blur,width))*lineColor;   
    }
     float hl=smoothstep(0.01,0.03,abs(p.x-p.x*.5));
    float vl=smoothstep(0.01,0.03,abs(p.y-p.y*.5));
    //x×(1−a)+y×a.
    col=lerp(float3(1,1,0),col,hl);
    col=lerp(float3(1,1,0),col,vl);
    
    return float4(col,1.0);
}
float4 RightTop(float2 p, float2 pos)
{
        float3 col;
    float3 lineColor=float3(.5,0.2,0.4);
    if((p.x>=p.x*.5)&&(p.y>p.y*.5)){
        float2 o=p+float2(-0.5*pos.x,-0.5*pos.y);
        float angle=atan(o);
        float l=length(o);
        float offset=abs(o.x)+abs(o.y)+(angle/(2.*PI))*dis;
        float circles=fmod(offset-Time,dis);
        col=(smoothstep(circles-blur,circles,width)-smoothstep(circles,circles+blur,width))*lineColor;
    }
    float hl=smoothstep(0.01,0.03,abs(p.x-p.x*.5));
    float vl=smoothstep(0.01,0.03,abs(p.y-p.y*.5));
    //x×(1−a)+y×a.
    col=lerp(float3(1,1,0),col,hl);
    col=lerp(float3(1,1,0),col,vl);
    
    return float4(col,1.0);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    
    float2 worldPos = (uv * Dimensions) + CamPos;
	float4 color = SAMPLE_TEXTURE(text, uv);
    float2 center = (float2)0.5;
    float size = 0.01;//Size; // Amplitude;
    float2 dist = Circle(uv, Center, size);
    float2 random = rand2(Time);
    float amp = sin(Time) / 3 * PI;
    float angle = atan2(uv.x,uv.y) * dist * amp;
    float distFromCenter = size - dist;  // positive when inside the circle
    if(distFromCenter >= 0)
    {
        float4 newColor = SAMPLE_TEXTURE(text, uv + angle * 10);
        if(newColor.g > 0.1)
        {
        newColor.r*=2;
        }
        return newColor;
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