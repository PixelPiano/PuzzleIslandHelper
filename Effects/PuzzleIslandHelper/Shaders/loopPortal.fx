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
uniform float3x3 Rotation;
uniform float2 Spacing = float2(0.05,0.05);
uniform float2 Center = float2(.5,.5);
// camera attributes
// cameraDirection and cameraUp MUST be normalized
// (ie. their length must be equal to 1)
//const float S = 0.000000059604643;
const float S = 0.01;
// const delta floattors for normal calculation
const float3 deltax = float3(1 ,0, 0);
const float3 deltay = float3(0 ,1, 0);
const float3 deltaz = float3(0 ,0, 1);
// ray computation vars
const float PI = 3.14159265359;
const float fov = 50.0;
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
float cube(float3 p, float3 o, float size){
    float3 d = abs(p + o) - (float3)size;
    return min(max(d.x, max(d.y,d.z)), 0.0)
        + length(max(d,0.0));
}

float sphere(float3 p){
    return length((float3)1.0 - p) - 1.0;
}
float combine(float3 shapeA, float3 shapeB)
{
    return min(shapeA,shapeB);
}
float extract(float3 shapeA, float3 shapeB)
{
    return max(shapeA,shapeB);
}
float sdOctahedron( float3 p, float s )
{
  p = abs(p);
  float m = p.x+p.y+p.z-s;
  float3 q;
       if( 3.0*p.x < m ) q = p.xyz;
  else if( 3.0*p.y < m ) q = p.yzx;
  else if( 3.0*p.z < m ) q = p.zxy;
  else return m*0.57735027;
    
  float k = clamp(0.5*(q.z-q.y+s),0.0,s); 
  return length(float3(q.x,q.y-s+k,q.z-k)); 
}
float distanceToNearestSurface(float3 p)
{
    return sdOctahedron(p, 1);
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
    float3 light = createLight(100.,30.,50.,Time);
    for(int i = 0; i < 20; i++)
    {
        float nearest = distanceToNearestSurface(p + dir*dist);
        if(nearest < 0.01)
        {
            float3 hit = p+dir*dist;
            return computeLambert(hit, computeSurfaceNormal(hit, S), light);
        }
        dist += nearest;
    }
    return float3(1,0,0);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    // generate the ray for this pixel
    float t = Time;
    float cameraDistance = 10.0;
    float3 cameraPosition = float3(10.0 * sin(t), 0.0, 10.0 * cos(t));
    float3 cameraDirection = float3(-1.0 * sin(t), 0.0, -1.0 * cos(t));
    float3 cameraUp = float3(0.0, 1.0, 0.0);
    float fovx = PI * fov / 360.0;
    float fovy = fovx * Dimensions.y/Dimensions.x;
    float ulen = tan(fovx);
    float vlen = tan(fovy);
    float3 nright = normalize(cross(cameraUp, cameraDirection));    
    float2 camUV = uv*2.0 - float2(1.0, 1.0);
    float3 pixel = cameraPosition + cameraDirection + nright*camUV.x*ulen + cameraUp*camUV.y*vlen;
    float3 rayDirection = normalize(pixel - cameraPosition);
    float3 pixelColour = intersectWithWorld(cameraPosition, rayDirection);
    if(pixelColour.r == 1 && pixelColour.g + pixelColour.b == 0)
    {
        return SAMPLE_TEXTURE(text,uv);
    }
    return float4(pixelColour, 1);
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