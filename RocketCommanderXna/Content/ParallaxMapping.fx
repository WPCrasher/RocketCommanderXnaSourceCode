// Project: Rocket Commander, File: ParallaxMapping.fx
// Creation date: 01.11.2005 04:56
// Last modified: 01.11.2005 18:13
// Author: Benjamin Nitschke (abi@exdream.com) (c) 2005
// Note: To test this use FX Composer from NVIDIA!

string description = "Parallax normal mapping shaders for a directional light";

// Shader techniques in this file, all shaders work with vs/ps 1.1, shaders not
// working with 1.1 have names with 20 at the end:
// Specular           : Full vertex ambient+diffuse+specular lighting
// Specular20         : Same for ps20, only required for 3DS max to show shader!

// Default variables, supported by the engine (may not be used here)
// If you don't need any global variable, just comment it out, this way
// the game engine does not have to set it!
//obs: float4x4 worldViewProj         : WorldViewProjection;
float4x4 viewProj         : ViewProjection;
float4x4 world            : World;
float4x4 viewInverse      : ViewInverse;

float3 lightDir : Direction
<
	string UIName = "Light Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {-0.65f, 0.65f, -0.39f}; // Normalized by app. FxComposer still uses inverted stuff :(

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
	string UIName = "Ambient Color";
	string Space = "material";
> = {0.1f, 0.1f, 0.1f, 1.0f};
//> = {0.25f, 0.25f, 0.25f, 1.0f};

float4 diffuseColor : Diffuse
<
	string UIName = "Diffuse Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 specularColor : Specular
<
	string UIName = "Specular Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float shininess : SpecularPower
<
	string UIName = "Specular Power";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 128.0;
	float UIStep = 1.0;
> = 16.0;

float parallaxAmount
<
	string UIName = "Parallax amount";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.0001;
> = 0.033f;

// Texture and samplers
texture diffuseTexture : Diffuse
<
	string UIName = "Diffuse Texture";
	string ResourceName = "asteroid4.dds";
>;
sampler diffuseTextureSampler = sampler_state
{
	Texture = <diffuseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture normalTexture : Diffuse
<
	string UIName = "Normal Texture";
	string ResourceName = "asteroid4Normal.dds";
>;
sampler normalTextureSampler = sampler_state
{
	Texture = <normalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture heightTexture : Diffuse
<
	string UIName = "Height Texture";
	string ResourceName = "asteroid4Height.dds";
>;
sampler heightTextureSampler = sampler_state
{
	Texture = <heightTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture NormalizeCubeTexture : Environment
<
	string UIName = "Normalize Cube Map Texture";
	string ResourceType = "CUBE";
	string ResourceName = "NormalizeCubeMap.dds";
>;

samplerCUBE NormalizeCubeTextureSampler = sampler_state
{
	Texture = <NormalizeCubeTexture>;
	// Clamp isn't good for negative values we need to normalize!
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
};

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
	float3 pos      : POSITION;
	float2 texCoord : TEXCOORD0;
	float3 normal   : NORMAL;
	float3 tangent	: TANGENT;
};

//----------------------------------------------------
/*obs
// Common functions
float4 TransformPosition(float3 pos)//float4 pos)
{
	//this eats up 1 more instruction:
	return mul(float4(pos.xyz, 1), worldViewProj);
	//needs float4:
	//return mul(pos, worldViewProj);
} // TransformPosition(.)
*
float3 GetWorldPos(float3 pos)
{
	return mul(float4(pos, 1), world).xyz;
} // GetWorldPos(.)
*/
float3 GetCameraPos()
{
	return viewInverse[3].xyz;
} // GetCameraPos()

float3 CalcNormalVector(float3 nor)
{
	return normalize(mul(nor, (float3x3)world));//worldInverseTranspose));
} // CalcNormalVector(.)

// Get light direction, inverted for FX Composer, else it will just return lightDir!
float3 GetLightDir()
{
	return lightDir;
} // GetLightDir()

float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace;
	/*tst1, 3dsmax mode
	worldToTangentSpace[0] =
		mul(cross(normal, tangent), world);
	worldToTangentSpace[1] = mul(tangent, world);
	worldToTangentSpace[2] = mul(normal, world);
	//*/
	//worldToTangentSpace[0] = mul(cross(tangent, normal), world);
	//worldToTangentSpace[1] = mul(tangent, world);
	//worldToTangentSpace[2] = mul(normal, world);
	worldToTangentSpace[0] = mul(tangent, world);
	worldToTangentSpace[1] = mul(cross(tangent, normal), world);
	worldToTangentSpace[2] = mul(normal, world);
	
	return worldToTangentSpace;
} // ComputeTangentMatrix(..)

//----------------------------------------------------

// Helpers for constant instancing
//we need rotation too, just update worldMatrix, not as fast, but much easier:
//float4 objPos = {0, 0, 0, 1}; // xyz is pos, w is size
//just use diffuseColor now: float4 objColor = {1, 1, 1, 1}; // color with alpha

// vertex shader output structure (optimized for ps_1_1)
struct VertexOutput_Specular
{
	float4 pos          : POSITION;
	float2 diffTexCoord : TEXCOORD0;
	float2 normTexCoord : TEXCOORD1;
	float3 viewVec      : TEXCOORD2;
	float3 lightVec     : TEXCOORD3;
	float3 lightVecDiv3 : COLOR0;
};

// Vertex shader function
VertexOutput_Specular VS_Specular(VertexInput In)
{
	VertexOutput_Specular Out = (VertexOutput_Specular) 0;
	
	float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
	Out.pos = mul(worldVertPos, viewProj);
	//obs: Out.pos = TransformPosition(In.pos);// * objPos.w + objPos.xyz);
	
	// Duplicate texture coordinates for diffuse and normal maps
	Out.diffTexCoord = In.texCoord;
	Out.normTexCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = GetCameraPos();
	//already defined: float3 worldVertPos = GetWorldPos(In.pos);

	// Transform light vector and pass it as a color (clamped from 0 to 1)
	// For ps_2_0 we don't need to clamp form 0 to 1
	float3 lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
	Out.lightVec = 0.5 + 0.5 * lightVec;
	Out.lightVecDiv3 = 0.5 + 0.5 * lightVec / 3;
	Out.viewVec = //0.5 + 0.5 *
		mul(worldToTangentSpace, worldEyePos - worldVertPos);

	//obs: Out.color = objColor;
	
	// And pass everything to the pixel shader
	return Out;
} // VS_Specular(.)

// Techniques
technique Specular
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS_Specular();
		sampler[0] = (diffuseTextureSampler);
		sampler[1] = (normalTextureSampler);
		sampler[2] = (NormalizeCubeTextureSampler);
		PixelShaderConstant1[0] = <ambientColor>;
		PixelShaderConstant1[2] = <diffuseColor>;
		PixelShaderConstant1[3] = <specularColor>;
		PixelShader = asm
		{
			// Optimized for ps_1_1, uses all possible 8 instructions.
			ps_1_1
			// Helper to calculate fake specular power.
			def c1, 0, 0, 0, -0.25
			//def c2, 0, 0, 0, 4
			def c4, 1, 0, 0, 1
			// Sample diffuse and normal map
			tex t0
			tex t1
			// Normalize view vector (t2)
			tex t2
			// Light vector (t3)
			texcoord t3
			// v0 is lightVecDiv3!
			// Convert agb to xyz (costs 1 instuction)
			lrp r1.xyz, c4, t1.w, t1
			// Now work with r1 instead of t1
			dp3_sat r0.xyz, r1_bx2, t3_bx2
			mad r1.xyz, r1_bx2, r0, -v0_bx2
			dp3_sat r1, r1, t2_bx2
			// Increase pow(spec) effect
			//obs: mul r0.rgb, r0, v1
			mul_x2_sat r1.w, r1.w, r1.w
			//we have to skip 1 mul because we lost 1 instruction because of agb
			//mul_x2_sat r1.w, r1.w, r1.w
			mad r0.rgb, r0, c2, c0
			// Combine 2 instructions because we need 1 more to set alpha!
			+add_sat r1.w, r1.w, c1.w
			mul r0.rgb, t0, r0
			+mul_x2_sat r1.w, r1.w, r1.w
			mad r0.rgb, r1.w, c3, r0
			// Set alpha from texture to result color!
			// Also multiplied by diffuseColor.a.
			+mul r0.a, t0.a, c2.a
		};
	} // pass P0
} // Specular

//----------------------------------------

// vertex shader output structure
struct VertexOutput_Specular20
{
	float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 lightVec     : TEXCOORD1;
	float3 viewVec      : TEXCOORD2;
	float  backFaceFactor : TEXCOORD3;
};

// Vertex shader function
VertexOutput_Specular20 VS_Specular20(VertexInput In)
{
	VertexOutput_Specular20 Out = (VertexOutput_Specular20) 0;
	
	float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
	Out.pos = mul(worldVertPos, viewProj);
	//obs: Out.pos = TransformPosition(In.pos * objPos.w + objPos.xyz);
	
	// Copy texture coordinates for diffuse and normal maps
	Out.texCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = GetCameraPos();
	//already defined: float3 worldVertPos = GetWorldPos(In.pos);

	// Transform light vector and pass it as a color (clamped from 0 to 1)
	// For ps_2_0 we don't need to clamp form 0 to 1
	Out.lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
	Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

	//obs: Out.color = objColor;
	// Little helper to darken back face areas, looks more realistic on asteroids
	Out.backFaceFactor = 0.25f + 0.75f *
		saturate(dot(CalcNormalVector(In.normal), lightDir)+0.5f);

	// And pass everything to the pixel shader
	return Out;
} // VS_Specular20(.)

//--------------------------------------

// Pixel shader function
float4 PS_Specular20(VertexOutput_Specular20 In) : COLOR
{
	// Get height from normal map alpha channel!
	float2 height = tex2D(heightTextureSampler, In.texCoord);
	
	// Calculate parallax offset
	float3 viewVector = normalize(In.viewVec);
	float2 offsetTexCoord = In.texCoord +
		// Push stuff more in than pulling it out, this minimized the disortion effect.
		(height*parallaxAmount - parallaxAmount*0.5f)*viewVector;
		//(height-1)*parallaxAmount*viewVector;

	// Grab texture data
	float4 diffuseTexture = tex2D(diffuseTextureSampler, offsetTexCoord);
	float3 normalVector = (2.0 * tex2D(normalTextureSampler, offsetTexCoord).agb) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);

	// Additionally normalize the vectors
	float3 lightVector = //In.lightVec;//not needed:
		normalize(In.lightVec);
	// Compute the angle to the light
	float bump =
		dot(normalVector, lightVector);
		//saturate(dot(normalVector, lightVector));
	// Specular factor
	float3 reflect = normalize(2 * bump * normalVector - lightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);

	// Darken down bump factor on back faces
	bump = bump * In.backFaceFactor;

	float3 ambDiffColor = ambientColor + bump * diffuseColor;
	//obs: diffuseTexture = diffuseTexture * In.color;
	float4 ret;
	ret.rgb = diffuseTexture * ambDiffColor +
		// Also multiply by height, lower stuff should be more occluded
		// and not have so much shininess
		(height.x + 0.5f) * bump * spec * specularColor * diffuseTexture.a;
	// Apply color
	ret.a = diffuseTexture.a * diffuseColor.a;
	return ret;
} // PS_Specular20(.)

// Techniques
technique Specular20
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS_Specular20();
		PixelShader  = compile ps_2_0 PS_Specular20();
	} // pass P0
} // Specular20

//----------------------------------------------------
//Added more optimized diffuse normal mapping shaders (no parallax)
// for faster asteroid rendering, looks almost as good and is a lot faster!

float alphaValue = 1.0f;

// vertex shader output structure
struct VertexOutput
{
	float4 pos          : POSITION;
	float2 diffTexCoord : TEXCOORD0;
	float2 normTexCoord : TEXCOORD1;
	float3 lightVec     : COLOR0;
};

// Vertex shader function
VertexOutput VS_Diffuse(VertexInput In)
{
	VertexOutput Out = (VertexOutput) 0; 
	float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
	Out.pos = mul(worldVertPos, viewProj);
	// Duplicate texture coordinates for diffuse and normal maps
	Out.diffTexCoord = In.texCoord;
	Out.normTexCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	// Transform light vector and pass it as a color (clamped from 0 to 1)
	Out.lightVec = 0.5 + 0.5 *
		normalize(mul(worldToTangentSpace, GetLightDir()));

	// And pass everything to the pixel shader
	return Out;
} // VS_Diffuse(.)

// Techniques
technique Diffuse
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS_Diffuse();
	
		sampler[0] = (diffuseTextureSampler);
		sampler[1] = (normalTextureSampler);
		PixelShaderConstant1[0] = <ambientColor>;
		PixelShaderConstant1[1] = <diffuseColor>;
		PixelShaderConstant1[2] = <alphaValue>;		
		PixelShader = asm
		{
			// Optimized for ps_1_1, uses just 4 instructions :)
			ps_1_1
			// Helper to calculate fake specular power.
			def c2, 1, 0, 0, 1
			// Sample diffuse and normal map
			tex t0
			tex t1
			// v0 is lightVector
			// Convert agb to xyz (costs 1 instuction)
			lrp r1, c2, t1.w, t1
			// Now work with r1 instead of t1
			dp3_sat r0, r1_bx2, v0_bx2
			mad r0, r0, c1, c0
			mul r0.rgb, r0, t0
			+mul r0.a, t0.a, c2.a
		};
	} // pass P0
} // Diffuse

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_Diffuse(VertexOutput In) : COLOR
{
	// Grab texture data
	float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
	//return diffuseTexture;
	float3 normalTexture = tex2D(normalTextureSampler, In.normTexCoord).agb;
	float3 normalVector =
		(2.0 * normalTexture) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);

	// Unpack the light vector to -1 - 1
	float3 lightVector =
		(2.0 * In.lightVec) - 1.0;

	// Compute the angle to the light
	float bump = saturate(dot(normalVector, lightVector));
	
	float4 ambDiffColor = ambientColor + bump * diffuseColor;
	float4 ret;
	// Apply color and alpha
	ret.rgb = diffuseTexture * ambDiffColor;
	ret.a = diffuseTexture.a * alphaValue;
	return ret;
} // PS_Diffuse(.)

// Same for ps20 to show up in 3DS Max.
technique Diffuse20
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS_Diffuse();
		PixelShader  = compile ps_2_0 PS_Diffuse();
	} // pass P0
} // Diffuse20
