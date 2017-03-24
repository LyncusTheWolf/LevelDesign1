//this is largely a cut & paste of FurSurface
//Tried about 1000 ways to make this a shader feature but no such luck
Shader "NeoFur/FurSurfaceTwoSided"
{
	Properties
	{
		_RootColor("Root Color", Color)								=(0.7132353,0.359,0.04195501,1)
		[MaterialToggle] _bRootTex("Root Texture Enable", Float)	=0
		_RootTexture("Root Texture", 2D)							="white" {}
		_TipColor("Tip Color", Color)								=(0.6207288,0.7993236,0.9485294,1)
		[MaterialToggle] _bTipTex("Tip Texture Enable", Float)		=0
		_TipTexture("Tip Texture", 2D)								="white" {}
		[MaterialToggle] _bDensity("Density Enable", Float)			=1
		_DensityTex("Density Texture", 2D)							="white" {}
		_FurNoiseTex("Fur Noise Texture", 2D)						="white" {}
		_Smoothness("Smoothness", Range(0, 1))						=0.5
		_Metallic("Metallic", Range(0, 1))							=0
		_RimLightStrength("Rimlight Strength", Range(0, 1))			=0
		_RimLightExponent("Rimlight Exponent", Range(0, 4))			=2
		_FuzzCenterDarkness("Fuzz Center Darkness", Range(0, 1))	=0.5
		_FuzzBrightness("Fuzz Brightness", Range(0, 4))				=1
		_FuzzRimColor("Fuzz Rim Color", Color)						=(1,1,1)
		_AOValue("AO Value", Range(0, 1))							=0.5
		_DensityMin("Density Min", Range(0, 1))						=0
		_DensityMax("Density Max", Range(0, 1))						=1
		_HeightMin("Height Min", Range(0, 1))						=0.5
		_HeightMax("Height Max", Range(0, 1))						=1
		_HeightPower("Height Power", Range(0, 4))					=1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		//draw back side first
		Cull Front
		CGPROGRAM

		//Replace FurStandard with your own lighting function if you want
		//to do some kind of special lighting.  Remember that the function
		//name needs to begin with Lighting and should have versions for
		//forward, deferred, and GI similar to ours in NeoFurLighting.cginc
		//See SampleLighting.cginc for examples
		#pragma	surface FurSurf FurStandard vertex:FurVS addshadow
		#pragma only_renderers d3d11
		#pragma target 5.0
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup

		//this is for animations that contain morph targets
		//not to be confused with morphs used for fur guide generation
		//(though you can have both)
		#pragma shader_feature MORPH_ANIMATED

		#include "CommonFunctions.cginc"
		#include "FurSetup.cginc"
		#include "NeoFurLighting.cginc"


		void FurVS(inout InputVS v, out Input o)
		{
			float3	cpPos, guideVec, norm;
			float4	skinPos, tan;
			float	SplineLength;

			CalcVertStuff(v, cpPos, guideVec, skinPos, norm, tan, SplineLength);

#ifdef SHADER_API_D3D11
#ifdef MORPH_ANIMATED
			//morphs go first
			Morph(v.vID, NumBlends, NumVerts, mBlends, mBlendWeights, skinPos.xyz, norm, tan);
#endif
#endif
			if(mbSkinned != 0)
			{
				SkinTransformStuff(v, skinPos, norm, tan, guideVec);
			}
			DoFurMath(SplineLength, guideVec, cpPos, norm, skinPos, tan, v, o);
		}


		void FurSurf(Input v, inout SurfaceOutputStandard o)
		{
			//surface function in FurSetup.cginc
			FurSurfaceFunc(v, o);
		}
		ENDCG

		Cull Back
		CGPROGRAM

		//Replace FurStandard with your own lighting function if you want
		//to do some kind of special lighting.  Remember that the function
		//name needs to begin with Lighting and should have versions for
		//forward, deferred, and GI similar to ours in NeoFurLighting.cginc
		//See SampleLighting.cginc for examples
		#pragma	surface FurSurf FurStandard vertex:FurVS addshadow
		#pragma only_renderers d3d11
		#pragma target 5.0
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup

		//this is for animations that contain morph targets
		//not to be confused with morphs used for fur guide generation
		//(though you can have both)
		#pragma shader_feature MORPH_ANIMATED

		#include "CommonFunctions.cginc"
		#include "FurSetup.cginc"
		#include "NeoFurLighting.cginc"
		

		void FurVS(inout InputVS v, out Input o)
		{
			float3	cpPos, guideVec, norm;
			float4	skinPos, tan;
			float	SplineLength;

			CalcVertStuff(v, cpPos, guideVec, skinPos, norm, tan, SplineLength);

#ifdef SHADER_API_D3D11
#ifdef MORPH_ANIMATED
			//morphs go first
			Morph(v.vID, NumBlends, NumVerts, mBlends, mBlendWeights, skinPos.xyz, norm, tan);
#endif
#endif

			if(mbSkinned != 0)
			{
				SkinTransformStuff(v, skinPos, norm, tan, guideVec);
			}
			DoFurMath(SplineLength, guideVec, cpPos, norm, skinPos, tan, v, o);
		}


		void FurSurf(Input v, inout SurfaceOutputStandard o)
		{
			//surface function in FurSetup.cginc
			FurSurfaceFunc(v, o);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
