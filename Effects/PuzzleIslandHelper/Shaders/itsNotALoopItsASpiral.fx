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

#define PI 3.1415

#define SPIRAL_A 0.05
#define SPIRAL_B 0.05

#define SPIRAL_WIDTH 0.5
#define MAX_RADIUS 0.1

#define SPIN_RATE 4.0

//https://en.wikipedia.org/wiki/Archimedean_spiral
//If on spiral, return number of degrees (theta).
//If not on spiral, return -1.0
float checkOnSpiral(float2 pc, float a, float b){
    
    //Solve for spiral theta, given the distance of the
    //polar coordinate (r = a+b*theta)
    float theta = (pc[1]-a)/b;
    //If polar coordinate angle is aligned with theta,
    //it is on the spiral
  	if(abs(pc[0]-fmod(theta,2.0*PI)) < SPIRAL_WIDTH)
        return theta;
   	else
        return -1.0;
}

static float decay = 0;
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
 //(-1,1)
	uv -= .5;
	uv.x = (Dimensions.x/Dimensions.y) * uv.x;
    
    //polar coordinates
    float2 pc=float2(
        atan2(uv.x, uv.y),
        length(uv)
    );
    
    //Time to hypnotize the viewer B)
    pc[0]+= SPIN_RATE*Time;
    pc[0]=fmod(pc[0], 2*PI);
	float timeStart = 1 / 3000.;
    float3 col = float3(0,0,0);
    float spiral_degrees = checkOnSpiral(pc, SPIRAL_A, SPIRAL_B - (timeStart + (1 / 5000.) * Time));
    if(spiral_degrees>=0.0){
            col = float3(1.0,1.0,1.0);
    }

    // Output to screen
    return float4(col,1.0);
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