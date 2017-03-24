// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef FUR_SETUP_INCLUDED
#define FUR_SETUP_INCLUDED

#include "UnityPBSLighting.cginc"


//these must be named in the usual unity way
struct InputVS
{
	float4	vertex				: POSITION;
	float3	normal				: NORMAL;
	float4	tangent				: TANGENT;
	float4	texcoord			: TEXCOORD0;	//standard texture uvs
	float4	texcoord1			: TEXCOORD1;	//for unity's lightmap stuff
	float4	texcoord2			: TEXCOORD2;	//for unity's lightmap stuff
#ifdef SHADER_API_D3D11
	uint	vID					: SV_VertexID;	//for indexing
#endif
};

//this MUST be named Input
//if it is named anything else expect completely
//incomprehensible error messages at line numbers
//beyond the end of your file, plus hours of torment
//deleting whole sections trying to isolate the error
struct Input
{
	float2	texcoord;
	float2	customThickenAlpha;
	float3	viewDir;
	float3	worldPos;
	float3	worldNormal;
//	float3	tanVec;
//	float3	biTangent;
};

//for morphs
struct BlendVertex
{
	float3	Position;
	float3	Normal;
	float3	Tangent;
};

struct ControlPoint
{
	float3	Position;
	float3	Velocity;
};

struct WeightGuide
{
	int4	BoneIdx;
	float4	BoneWeights;
	float3	GuideVec;
};

//fur material stuff
half	_AOValue;
half	_RimLightStrength;
half	_RimLightExponent;
half	_FuzzCenterDarkness;
half	_FuzzBrightness;
uniform float4 _FuzzRimColor;
half	_Metallic;
half	_Smoothness;

//new params
uniform float _DensityMin;
uniform float _DensityMax;
uniform float _HeightMin;
uniform float _HeightMax;
uniform float _HeightPower;

//shell related stuff
uniform int		In_ShellCount;
uniform int		In_TotalShellCount;
uniform float	In_CurShell;		//should be an int, but no matpropblock setint
uniform float	In_ShellDistance;
uniform float	In_ShellFade;
uniform half	BendExponent;
uniform half	In_VisibleLengthScale;

//skinning
int	mbSkinned;
int	mQuality;	//1 for 1Bone, 2 for 2Bone, 4 for 4Bone

//texture stuff
uniform float4		_RootColor;
uniform float4		_TipColor;
uniform fixed		_bRootTex;
uniform fixed		_bTipTex;
uniform sampler2D	_RootTexture;
uniform float4		_RootTexture_ST;
uniform sampler2D	_TipTexture;
uniform float4		_TipTexture_ST;
uniform sampler2D	_DensityTex;
uniform float4		_DensityTex_ST;
uniform sampler2D	_FurNoiseTex;
uniform float4		_FurNoiseTex_ST;
uniform fixed		_bDensity;

#ifdef SHADER_API_D3D11
StructuredBuffer<float4x4>		mBones;
StructuredBuffer<ControlPoint>	mFurCPs;
StructuredBuffer<WeightGuide>	mFurWGs;

#ifdef MORPH_ANIMATED
		int	NumBlends;
		int	NumVerts;
		StructuredBuffer<BlendVertex>	mBlends;
		StructuredBuffer<float>			mBlendWeights;
#endif
#endif

//A few functions here to make maintenance easier.
//Ultimately it would be cool if all the variants could go in a single
//shader, but having trouble with two-sided and shadows.

//initial grab of vertex data from compute buffer stuff
void CalcVertStuff(InputVS v, out float3 cpPos, out float3 guideVec, out float4 skinPos,
	out float3 norm, out float4 tan, out float splineLen)
{
#ifdef SHADER_API_D3D11
	cpPos			=mFurCPs[v.vID].Position;
	guideVec		=mFurWGs[v.vID].GuideVec;
#else
	//shouldn't really get here but needed for compilation
	cpPos			=v.vertex.xyz;
	guideVec		=v.normal;
#endif
	splineLen	=length(guideVec);

	skinPos	=v.vertex;
	norm	=v.normal;
	tan		=v.tangent;
}


//not sure if I'm wasting better instructions, but had some
//annoyances with mul on GL
//Remember Abrash's black book? :)
float3 RotateByProjection(float4x4 mat, float3 vec)
{
	float3	ret;

	ret.x	=dot(mat[0], vec);
	ret.y	=dot(mat[1], vec);
	ret.z	=dot(mat[2], vec);

	return	ret;
}


#ifdef SHADER_API_D3D11
//the usual 4 influences per vert setting
float4x4 GetSkinXForm4(int4 bnIdxs, float4 bnWeights, StructuredBuffer<float4x4> bones)
{
	float4x4 skinTransform	=bones[bnIdxs.x] * bnWeights.x;
				
	skinTransform	+=bones[bnIdxs.y] * bnWeights.y;
	skinTransform	+=bones[bnIdxs.z] * bnWeights.z;
	skinTransform	+=bones[bnIdxs.w] * bnWeights.w;
			
	return	skinTransform;
}

//2 bone setting
float4x4 GetSkinXForm2(int4 bnIdxs, float4 bnWeights, StructuredBuffer<float4x4> bones)
{
	float	totalWeight	=bnWeights.x + bnWeights.y;

	float4x4 skinTransform	=bones[bnIdxs.x] * (bnWeights.x / totalWeight);
				
	skinTransform	+=bones[bnIdxs.y] * (bnWeights.y / totalWeight);
			
	return	skinTransform;
}

//single bone setting
float4x4 GetSkinXForm1(int4 bnIdxs, float4 bnWeights, StructuredBuffer<float4x4> bones)
{
	return	bones[bnIdxs.x];
}
#endif


void SkinTransformStuff(InputVS v, inout float4 skinPos,
	inout float3 norm, inout float4 tan, inout float3 guideVec)
{
#ifdef SHADER_API_D3D11
	float4x4	skinXForm	=0;
	if(mQuality == 0 || mQuality == 4)
	{
		skinXForm	=GetSkinXForm4(mFurWGs[v.vID].BoneIdx, mFurWGs[v.vID].BoneWeights, mBones);
	}
	else if(mQuality == 2)
	{
		skinXForm	=GetSkinXForm2(mFurWGs[v.vID].BoneIdx, mFurWGs[v.vID].BoneWeights, mBones);
	}
	else
	{
		skinXForm	=GetSkinXForm1(mFurWGs[v.vID].BoneIdx, mFurWGs[v.vID].BoneWeights, mBones);
	}
#else
	float4x4	skinXForm	=0;
	//just set to identity
	skinXForm[0]	=float4(1, 0, 0, 0);
	skinXForm[1]	=float4(0, 1, 0, 0);
	skinXForm[2]	=float4(0, 0, 1, 0);
	skinXForm[3]	=float4(0, 0, 0, 1);
#endif

	//skin transform vert position
	skinPos		=mul(skinXForm, skinPos);
	skinPos.xyz	/=skinPos.w;

	guideVec	=RotateByProjection(skinXForm, guideVec);
	norm		=RotateByProjection(skinXForm, v.normal);
	tan.xyz		=RotateByProjection(skinXForm, v.tangent.xyz);
}


void FurAlphaSetup(float shellCount, float totalShellCount, float curShell,
	out float curAlpha, out float thicken)
{
	// Artificial thickness increase for lower-res LODs.
	thicken	=1.0f - (float(shellCount) / float(totalShellCount));
	thicken	=clamp(thicken, 0.0f, 1.0f);
	thicken	=1.0f + thicken * 5.0f;
				
	float	furDelta	=1.0f / (shellCount - 1);
				
	//a small offset to prevent z fighting on the innermost shell
	float	firstOffs	=furDelta / 2.0f;
				
	//step in shell alpha, 1 to stepAmount (never goes 0)
	float	alphaDelta	=1.0f / (shellCount + 1);
	float	curFur		=(furDelta * curShell) + firstOffs;
	
	curAlpha	=alphaDelta * (curShell + 1);
}


//controls the curve from base to tip
float3 GetPositionForShell(float shellAlpha, float3 skinPos, float3 guideVec, float3 cpPos,
	float bendExponent, float shellFade, float shellDist)
{
	float	blendPointAlpha	=1.0 - pow(1.0 - shellAlpha, bendExponent);
	float3	blendPoint		=(1.0 - blendPointAlpha) * (skinPos + guideVec) +
							blendPointAlpha * cpPos;

	float	posAlpha	=shellAlpha * shellFade * In_VisibleLengthScale;

	float	cpDist	=length(cpPos - skinPos);

	//trying something new
	//do this ratio in "guide normalized space"
	//where the control point rest pos is near the guide endpoint
	float3	furPos=
		(1.0 - posAlpha) * skinPos +
		posAlpha * blendPoint;

	//normalize and rescale to the guide and cp length
	float3	newVec	=normalize(furPos - skinPos);
	newVec			*=shellDist * posAlpha * cpDist;

	return	skinPos + newVec;
}


void DoFurMath(float SplineLength, float3 guideVec, float3 cpPos, float3 norm,
	inout float4 skinPos, inout float4 tan, inout InputVS v, inout Input o)
{
	//this value needs to be a bit longer than the value done
	//on the physics side, as inaccuracy can result from guides
	//being passed through textures or using 16 bit floats.
	if(SplineLength >= 0.0005)
	{
		float	curAlpha, thickenAmount;
		FurAlphaSetup(In_ShellCount, In_TotalShellCount, In_CurShell,
						curAlpha, thickenAmount);

		skinPos.xyz	=GetPositionForShell(curAlpha, skinPos, guideVec, cpPos,
			BendExponent, In_ShellFade, In_ShellDistance);

		//world transform computed position
		float4	wPos	=mul(unity_ObjectToWorld, skinPos);
		wPos.xyz		/=wPos.w;

		o.customThickenAlpha.x	=pow(curAlpha, thickenAmount);
		o.customThickenAlpha.y	=curAlpha;
		o.worldPos				=wPos.xyz;

		//output vertex should not be world transformed for surface shaders
		v.vertex	=skinPos;

		//should this be rotated?  Surf shader might do it for us
		//TODO: visualize
		tan.xyz	=RotateByProjection(unity_ObjectToWorld, tan.xyz);

		o.viewDir		=UnityWorldSpaceViewDir(wPos);
	}
	else
	{
		//Doesn't matter what happens here much since this will get clipped
		float4	wPos	=mul(unity_ObjectToWorld, skinPos);
		wPos.xyz		/=wPos.w;

		o.customThickenAlpha.xy	=-1.0;

		//output vertex should not be world transformed for surface shaders
		v.vertex	=skinPos;

		o.viewDir	=UnityWorldSpaceViewDir(wPos);
		o.worldPos	=wPos.xyz;
	}
	o.texcoord		=v.texcoord.xy;
	v.normal		=normalize(norm.xyz);
	v.tangent.xyz	=normalize(tan.xyz);
	o.worldNormal	=RotateByProjection(unity_ObjectToWorld, norm);
//	o.biTangent		=normalize(cross(norm, tan.xyz) * tan.w);
//	o.tanVec		=normalize(tan.xyz);
}


//Newer fancier alpha from Carlos
float MakeCarlosAlpha(float noiseVal, float densityVal,	//from textures
	float heightMin, float heightMax, float heightPower,	//from material
	float bDensity, float densityMin, float densityMax,		//from material
	float shellAlpha, out float furAlpha)
{
	float	density		=lerp(1, densityVal, bDensity);
	furAlpha			=noiseVal * lerp(densityMin, densityMax, density);

	float	clippy			=furAlpha - shellAlpha;
	float	heightScaled	=lerp(heightMin, heightMax, clippy);

	return	(pow(heightScaled, heightPower) - 0.5);
}


#ifdef SHADER_API_D3D11
//apply morph deltas to vert data
void Morph(int idx, int numBlends, int numVerts,
	StructuredBuffer<BlendVertex> blends,
	StructuredBuffer<float> blendWeights,
	inout float3 vertPos, inout float3 norm, inout float4 tan)
{
	if(numBlends <= 0)
	{
		return;
	}

	float3	posDiff		=0;
	float3	normDiff	=0;
	float3	tanDiff		=0;

	for(int blend=0;blend < numBlends;blend++)
	{
		float	weight	=blendWeights[blend];

		float3	morphPos	=blends[idx + (blend * numVerts)].Position;
		float3	morphNorm	=blends[idx + (blend * numVerts)].Normal;
		float3	morphTan	=blends[idx + (blend * numVerts)].Tangent;

		posDiff		+=(morphPos * weight);
		normDiff	+=(morphNorm * weight);
		tanDiff		+=(morphTan * weight);
	}

	vertPos	+=posDiff;
	norm	+=normDiff;
	tan.xyz	+=tanDiff;

	//renormalize
	norm	=normalize(norm);
	tan.xyz	=normalize(tan.xyz);
}
#endif


//had no luck with functionalizing individual bits of this, but doing
//the whole thing seems fine
void FurSurfaceFunc(Input v, inout SurfaceOutputStandard o)
{
	//clip on zero length guides
	clip(v.customThickenAlpha.x);

	float3	norm	=normalize(v.worldNormal);
//	float3	biTan	=v.biTangent;
//	float3	tan		=v.tanVec;

//	float3x3	tangentTransform	=float3x3(tan, biTan, norm);

	float4	furNoise	=tex2D(_FurNoiseTex, TRANSFORM_TEX(v.texcoord.xy, _FurNoiseTex));
	float4	density		=tex2D(_DensityTex, TRANSFORM_TEX(v.texcoord.xy, _DensityTex));

	float	furAlpha;	//out parameter
	float	clippy	=MakeCarlosAlpha(furNoise.x, density.x,
						_HeightMin, _HeightMax, _HeightPower,
						_bDensity, _DensityMin, _DensityMax,
						v.customThickenAlpha.x, furAlpha);

	clip(clippy);

	float4	furRoot	=_RootColor;
	float4	furTip	=_TipColor;

	if(_bRootTex)
	{
		furRoot	*=tex2D(_RootTexture, TRANSFORM_TEX(v.texcoord.xy, _RootTexture));
	}
	if(_bTipTex)
	{
		furTip	*=tex2D(_TipTexture, TRANSFORM_TEX(v.texcoord.xy, _TipTexture));
	}
			
	float3	colorBlend	=lerp(furRoot.xyz, furTip.xyz, v.customThickenAlpha.y);

	o.Albedo		=colorBlend;
	o.Metallic		=_Metallic;
	o.Smoothness	=_Smoothness;
	o.Occlusion		=furAlpha;
}
#endif