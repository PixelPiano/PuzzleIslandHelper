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
uniform float Amplitude;
uniform float Radius = 0.5;
float2 Circle(float2 uv, float2 center, float size)
{
    float r = size; //define the radius of the circle
	float x = (center.x-uv.x);
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
float3x3 xrot(float angle) {
    float3x3 m;
    m[0] = float3(1.0, 0.0, 0.0);
    m[1] = float3(0.0, cos(angle), -sin(angle));
    m[2] = float3(0.0, sin(angle), cos(angle));
    return m;
}

float3x3 yrot(float angle) {
    float3x3 m;
    m[0] = float3(cos(angle), 0.0, -sin(angle));
    m[1] = float3(0.0, 1.0, 0.0);
    m[2] = float3(sin(angle), 0.0, cos(angle));
    return m;
}

float intersectSphere(float3 camera, float3 ray, float3 spherePosition, float sphereRadius) {
    float radiusSquared = sphereRadius * sphereRadius;
    float dt = dot(ray, spherePosition - camera);
    if (dt < 0.0) {
        return -1.0;
    }
    float3 tmp = camera - spherePosition;
    tmp.x = dot(tmp, tmp);
    tmp.x = tmp.x - dt * dt;
    if (tmp.x >= radiusSquared) {
        return -1.0;
    }
    float distanceFromCamera = dt - sqrt(radiusSquared - tmp.x);
    return distanceFromCamera;
}
float2 rand2(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
float2 rand3(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{    
    float4 color = float4(0,0,0,0);
    bool hit = false;

    float2 pos = uv;
	bool change = rand3(uv.yy * Time) < 0.4;
	if(change)
	{
		float pixeloffset = uv.y * 10 / Dimensions.x;
		float offset = -pixeloffset / 2 + rand3(uv.y * rand2(uv.y / Time)) * pixeloffset;
		pos = uv + float2(offset, offset * Time % 0.007);
	}
	
    uv = pos;
    uv += 0.5;
	uv.x /= Dimensions.y / Dimensions.x;
    float2 uvSave = uv;
    float3 spherePosition = float3(0,0, 0.0);
    float sphereRadius = Radius * 4;
    float3 cameraPosition = float3(0.0, 0.0, -10.0);
    uv = uv * 2.0;
    uv.y -= 1.0;
    uv.x -= (1.0 / (Dimensions.y / Dimensions.x));

	float3x3 yr = yrot(Time * 0.3);
	float3x3 yrr = yrot(-Time * 0.3);
    float3 pixelPosition = float3(uv.x / 5, uv.y/5, -9.0);
    
    float3 ray = pixelPosition - cameraPosition;  // Generate a ray
    ray = normalize(ray);
    float3 camSave = cameraPosition;
    float3 raySave = ray;
	ray = mul(ray,yr);
	cameraPosition = mul(cameraPosition, yr);
    float distance = intersectSphere(cameraPosition, ray, spherePosition, sphereRadius);
    float shading = 1 - Circle(uv, spherePosition, sphereRadius) - 0.9;
	if(distance > 11.4 - sphereRadius) return float4((float3)(shading),1);
    if (distance > 0.0)
    {
                 ray.x += rand3(uv.y) * 10;
        float3 pointOfIntersection = cameraPosition + ray * distance;
        float3 normal = normalize(pointOfIntersection - spherePosition);
        float u = 0.5 + atan2(normal.z, normal.x) / (3.1415926 * 2.0);
        float v = 0.5 - asin(normal.y) / -3.1415926;
        float x = frac(u * 18.0);
        float y = frac(v * 10.0);
        
        if (x < 0.15 || y < 0.15)
        {
           float3 a = (float3)shading;
           color = float4(a, 1);
           hit = true;
       	}
        else
        {
            ray = mul(raySave, yrr);
            cameraPosition = mul(camSave, yrr);
            distance = intersectSphere(cameraPosition, ray, spherePosition, sphereRadius);
   
            pointOfIntersection = cameraPosition + ray * distance;
            normal = normalize(pointOfIntersection - spherePosition);
            u = 0.5 + atan2(normal.z, normal.x) / (3.1415926 * 2.0);
            v = 0.5 - asin(normal.y) / -3.1415926;
            x = frac(u * 18.0);
            y = frac(v * 10.0);
              if (x < 0.15 || y < 0.15) 
            {
                float3 a = (float3)(shading / 6);
                color = float4(a,1);
                hit = true;
       	    }
            else
            {
                color = (float4)0;
            }
        }
    }
    uv = uvSave;
    if(hit)
    {
        float seed = Time / 50;
        float2 r = rand3(seed) + float2(0, -0.2);
        float2 s = rand3(seed / 2) * float2(0.7, 0.7);
        if(uv.x > r.x && uv.x < r.x + s.x && uv.y > r.y && uv.y < r.y + s.y)
        {
            color *= 1.3;
        }
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