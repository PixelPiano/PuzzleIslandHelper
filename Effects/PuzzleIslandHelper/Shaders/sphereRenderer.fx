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
uniform float4 BackColor;
uniform float4 FrontColor;
uniform int Layers = 4;
uniform int MinSize = 3;
uniform int MaxSize = 6;
uniform int MinDepth;
uniform int MaxDepth;
uniform float2 Spacing = float2(0.05,0.05);
uniform float2 Center = float2(.5,.5);
// camera attributes
// cameraDirection and cameraUp MUST be normalized
// (ie. their length must be equal to 1)
const float3 cameraPosition = float3(0.0, 0.0, 10.0);
const float3 cameraDirection = float3(0.0, 0.0, -1.0);
const float3 cameraUp = float3(0.0, 1.0, 0.0);
const float S = 0.1;
// const delta floattors for normal calculation
const float3 deltax = float3(1 ,0, 0);
const float3 deltay = float3(0 ,1, 0);
const float3 deltaz = float3(0 ,0, 1);
// ray computation vars
const float PI = 3.14159265359;
const float fov = 50.0;

float distanceToNearestSurface(float3 p){
    return length(p) - 1.0;
}
float3 computeLambert(float3 hit, float3 surfaceNormal, float3 light){
    float a = dot(normalize(light-hit), surfaceNormal);
    return float3(a,a,a);
}
float3 computeSurfaceNormal(float3 hit, float s){
    float d = distanceToNearestSurface(hit);
    return normalize(float3(
        distanceToNearestSurface(hit+deltax * s)-d,
        distanceToNearestSurface(hit+deltay * s)-d,
        distanceToNearestSurface(hit+deltaz * s)-d
    ));
}
float3 createLight(float x, float y, float z, float angle)
{
    return float3(x * sin(angle), y * cos(angle), z * cos(angle));
}
float3 intersectWithWorld(float3 p, float3 dir)
{
    float dist = 0.0;
    for(int i = 0; i < 20; i++)
    {
        float nearest = distanceToNearestSurface(p + dir*dist);
        if(nearest < 0.01)
        {
            float3 hit = p+dir*dist;
            float3 light = createLight(100.,30.,50.,Time);
            return computeLambert(hit, computeSurfaceNormal(hit, S), light);
        }
        dist += nearest;
    }
    return (float3)0;
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    // generate the ray for this pixel
    const float fovx = PI * fov / 360.0;
    float fovy = fovx * Dimensions.y/Dimensions.x;
    float ulen = tan(fovx);
    float vlen = tan(fovy);    
    float2 camUV = uv*2.0 - float2(1.0, 1.0);
    float3 nright = normalize(cross(cameraUp, cameraDirection));
    float3 pixel = cameraPosition + cameraDirection + nright*camUV.x*ulen + cameraUp*camUV.y*vlen;
    float3 rayDirection = normalize(pixel - cameraPosition);
    float3 pixelColour = intersectWithWorld(cameraPosition, rayDirection);

    return float4(pixelColour, 1.0);
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