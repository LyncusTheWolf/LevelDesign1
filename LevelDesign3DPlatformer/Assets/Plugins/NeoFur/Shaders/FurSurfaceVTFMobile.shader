Shader "NeoFur/FurSurfaceVTFMobile" {
	Properties {
		_RootColor("Root Color", Color)								=(0.7132353,0.359,0.04195501,1)
		_TipColor("Tip Color", Color)								=(0.6207288,0.7993236,0.9485294,1)
		[MaterialToggle] _bTipTex("Fur Texture Enable", Float)		=0
		_TipTexture("Fur Texture", 2D)								="white" {}
		_FurNoiseTex("Fur Noise Texture", 2D)						="white" {}
		_Smoothness("Smoothness", Range(0, 1))						=0.5
		_Metallic("Metallic", Range(0, 1))							=0
		_RimLightStrength("Rimlight Strength", Range(0, 1))			=0
		_RimLightExponent("Rimlight Exponent", Range(0, 4))			=2
		_FuzzCenterDarkness("Fuzz Center Darkness", Range(0, 1))	=0.5
		_FuzzBrightness("Fuzz Brightness", Range(0, 4))				=1
		_FuzzRimColor("Fuzz Rim Color", Color)						=(1,1,1)
		_AOValue("AO Value", Range(0, 1))							=0.5
		_HeightMin("Height Min", Range(0, 1))						=0.5
		_HeightMax("Height Max", Range(0, 1))						=1
		_HeightPower("Height Power", Range(0, 4))					=1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Back
		LOD 400
		
		CGPROGRAM

		//Replace FurStandard with your own lighting function if you want
		//to do some kind of special lighting.  Remember that the function
		//name needs to begin with Lighting and should have versions for
		//forward, deferred, and GI similar to ours in NeoFurLighting.cginc
		//See SampleLighting.cginc for examples
		#pragma	surface FurSurf FurStandard vertex:FurVS addshadow
		#define UNITY_SETUP_BRDF_INPUT MetallicSetup
		#pragma target 3.0

		//this is for animations that contain morph targets
		//not to be confused with morphs used for fur guide generation
		//(though you can have both)
		#pragma shader_feature MORPH_ANIMATED

		//drop a few features for mobile
		#define MOBILEFUR

		#include "CommonFunctions.cginc"
		#include "FurSetup.cginc"
		#include "NeoFurLighting.cginc"


		//samplers for extra data
		sampler2D	FurGuides;
		sampler2D	FurCPs;
		sampler2D	Bones;
		sampler2D	Morphs;

		float4	mTexSizes;	//cp/guide/bone/morph tex sizes
		int		mNumVerts;

		float4	mBlendVec;


		//convert index into uv coordinates
		//that fetch into textures
		void ConvertIndexToUV(float index, float texSize, out float2 coord)
		{
			//the divide and mod get compiled to a udiv instruction
			//with dx11, that then gets translated to a GL that stomps
			//takes the div result as the arg to the mod, which is bad
			float	y	=floor(index / texSize);
			float	x	=floor((uint)index % (uint)texSize);
				
			coord.x	=floor(x) / texSize;
			coord.y	=floor(y) / texSize;
		}


		void Morph(uint index, inout float3 vertPos,
			inout float3 norm, inout float4 tan)
		{
			float3	posDiff		=0;
			float3	normDiff	=0;
			float3	tanDiff		=0;

			//morph 0
			float4	uv		=0;
			uint	idx		=index * 3;	//3 elements per

			ConvertIndexToUV(idx, mTexSizes.z, uv.xy);
			float4	morphPos	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 1, mTexSizes.z, uv.xy);
			float4	morphNorm	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 2, mTexSizes.z, uv.xy);
			float4	morphTan	=tex2Dlod(Morphs, uv);

			posDiff		+=(morphPos.xyz * mBlendVec.x);
			normDiff	+=(morphNorm.xyz * mBlendVec.x);
			tanDiff		+=(morphTan.xyz * mBlendVec.x);

			//morph 1
			idx		=((1 * mNumVerts) + index) * 3;	//3 elements per

			ConvertIndexToUV(idx, mTexSizes.z, uv.xy);
			morphPos	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 1, mTexSizes.z, uv.xy);
			morphNorm	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 2, mTexSizes.z, uv.xy);
			morphTan	=tex2Dlod(Morphs, uv);

			posDiff		+=(morphPos.xyz * mBlendVec.y);
			normDiff	+=(morphNorm.xyz * mBlendVec.y);
			tanDiff		+=(morphTan.xyz * mBlendVec.y);

			//morph 2
			idx		=((2 * mNumVerts) + index) * 3;	//3 elements per

			ConvertIndexToUV(idx, mTexSizes.z, uv.xy);
			morphPos	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 1, mTexSizes.z, uv.xy);
			morphNorm	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 2, mTexSizes.z, uv.xy);
			morphTan	=tex2Dlod(Morphs, uv);

			posDiff		+=(morphPos.xyz * mBlendVec.z);
			normDiff	+=(morphNorm.xyz * mBlendVec.z);
			tanDiff		+=(morphTan.xyz * mBlendVec.z);

			//morph 3
			idx		=((3 * mNumVerts) + index) * 3;	//3 elements per

			ConvertIndexToUV(idx, mTexSizes.z, uv.xy);
			morphPos	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 1, mTexSizes.z, uv.xy);
			morphNorm	=tex2Dlod(Morphs, uv);

			ConvertIndexToUV(idx + 2, mTexSizes.z, uv.xy);
			morphTan	=tex2Dlod(Morphs, uv);

			posDiff		+=(morphPos.xyz * mBlendVec.w);
			normDiff	+=(morphNorm.xyz * mBlendVec.w);
			tanDiff		+=(morphTan.xyz * mBlendVec.w);


			vertPos	+=posDiff;
			norm	+=normDiff;
			tan.xyz	+=tanDiff;

			//renormalize
			norm	=normalize(norm);
			tan.xyz	=normalize(tan.xyz);
		}


		float4x4 GetSkinXForm(int4 boneIdx, float4 bnWeights)
		{
			float4x4	skinXForm		=0;
			float4x4	madTransform	=0;
			float4		coords			=0;

			//calc step in texture for vector4s
			float	boneStep	=1.0 / mTexSizes.w;

			//mul by 4 because 4x4 matrix
			boneIdx	*=4;

			ConvertIndexToUV(boneIdx.x, mTexSizes.w, coords.xy);

			float4	r0	=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			float4	r1	=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			float4	r2	=tex2Dlod(Bones, coords);
			float4	r3	=float4(0, 0, 0, 1);

			skinXForm	=float4x4(r0, r1, r2, r3);
			skinXForm	*=bnWeights.x;

			ConvertIndexToUV(boneIdx.y, mTexSizes.w, coords.xy);				

			r0			=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			r1			=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			r2			=tex2Dlod(Bones, coords);

			madTransform	=float4x4(r0, r1, r2, r3);
			skinXForm		+=madTransform * bnWeights.y;

			ConvertIndexToUV(boneIdx.z, mTexSizes.w, coords.xy);

			r0			=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			r1			=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			r2			=tex2Dlod(Bones, coords);

			madTransform	=float4x4(r0, r1, r2, r3);
			skinXForm		+=madTransform * bnWeights.z;

			ConvertIndexToUV(boneIdx.w, mTexSizes.w, coords.xy);

			r0			=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			r1			=tex2Dlod(Bones, coords);
			coords.x	+=boneStep;

			r2			=tex2Dlod(Bones, coords);

			madTransform	=float4x4(r0, r1, r2, r3);
			skinXForm		+=madTransform * bnWeights.w;

			return	skinXForm;
		}


		void FurVS(inout InputVS v, out Input o)
		{
			//on SM3 SV_Index isn't supported, so we stuff the
			//index in texCoord0.z
			float	vertIndex	=v.texcoord.z;

			float3	cpPos, guideVec, norm;
			float4	skinPos, tan	=0;
			float	SplineLength;

			//calc the texcoord for looking up guides/control points
			float4	gcpUV	=0;
			ConvertIndexToUV(vertIndex, mTexSizes.x, gcpUV.xy);

			//get extra data from textures
			cpPos			=tex2Dlod(FurCPs, gcpUV).xyz;
			guideVec		=tex2Dlod(FurGuides, gcpUV).xyz;
			SplineLength	=length(guideVec);

			skinPos	=v.vertex;
			norm	=v.normal;

#ifdef MORPH_ANIMATED
			Morph(vertIndex, skinPos.xyz, norm, tan);
#endif

			if(mTexSizes.w > 0)
			{
				float4x4	skinXForm	=GetSkinXForm(v.texcoord1, v.texcoord2);

				//skin transform vert position
				skinPos		=mul(skinXForm, skinPos);
				skinPos.xyz	/=skinPos.w;

				guideVec	=RotateByProjection(skinXForm, guideVec);
				norm		=RotateByProjection(skinXForm, v.normal);
			}

			DoFurMath(SplineLength, guideVec, cpPos, norm, skinPos, tan, v, o);
		}


		void FurSurf(Input v, inout SurfaceOutputStandard o)
		{
			FurSurfaceFunc(v, o);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
