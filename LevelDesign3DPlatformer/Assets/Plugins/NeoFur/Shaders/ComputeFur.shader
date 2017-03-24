//This is for platforms that have compute shaders but don't support dx11
//which I think right now is just Linux.  Lots of work still to do here,
//mainly it needs extra light passes and shadowing code
Shader "NeoFur/ComputeFur"
{
	Properties
	{
		Color_Base("Color Base", 2D) = "white" {}
		Color_Tip("Color Tip", 2D) = "white" {}
		Growth("Growth", 2D) = "white" {}
		Pattern("Pattern", 2D) = "white" {}
		MaskClip("Opacity Mask Clip", Range(0.001, 1)) = 0.333
		TriColor1("Trilight Color 1", Color) = (0.4, 0.4, 0.4, 1)
		TriColor2("Trilight Color 2", Color) = (0.3, 0.3, 0.3, 1)
		mSpecColor("Specular Color", Color) = (1, 1, 1, 1)
		mSpecPower("Specular Power", Range(0.1, 25)) = 3
		AO_Pattern("AO Pattern", Range(0.01, 3)) = 0.5
		AO_Shell("AO Shell", Range(0.01, 3)) = 0.5
		AnisoEdgeBrighten("Edge Brighten", Range(0.1, 5)) = 2
		AnisoCenterDarken("Center Darken", Range(0.01, 3)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 600

		Pass
		{
			CGPROGRAM
			#pragma	target		5.0		//if compute shaders are around, can assume 5
			#pragma	vertex		FurVS
			#pragma	fragment	FurPS
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "CommonFunctions.cginc"

			struct VSInput
			{
				float3	Position	: POSITION;
				float3	Normal		: NORMAL;
				float4	Tangent		: TANGENT;
				float4	UV0_TA		: TEXCOORD0;
			};

			StructuredBuffer<VSInput>	mVerts;

			struct PSInput
			{
				float4	Position	: SV_POSITION;
				float3	Normal		: NORMAL;		//normal
				float4	Tangent		: TANGENT;		//tan
				float3	WPos		: TEXCOORD0;	//world position for specular
				float4	UV0_TA		: TEXCOORD1;	//uv in xy, thickenalpha zw
			};

			//textures
			sampler2D	Color_Base;
			float4		Color_Base_ST;
			sampler2D	Color_Tip;
			float4		Color_Tip_ST;
			sampler2D	Growth;
			float4		Growth_ST;
			sampler2D	Pattern;
			float4		Pattern_ST;

			//misc shader vars
			float		MaskClip;
			float4		TriColor1;		//trilight stuff
			float4		TriColor2;		//trilight stuff
			float4		mSpecColor;		//specular color
			float		mSpecPower;		//specular power
			float4x4	mObjectToWorld;	//fake _Object2World since it sometimes doesn't get set

			//fur material stuff
			float	AO_Pattern;
			float	AO_Shell;
			float	AnisoEdgeBrighten;
			float	AnisoCenterDarken;
			
			PSInput	FurVS(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				PSInput	o;

				float4	pos	=float4(mVerts[id].Position, 1);

				//skin to world space position
				float4	wPos	=mul(mObjectToWorld, pos);

				wPos.xyz	/=wPos.w;

				o.Position		=mul(UNITY_MATRIX_VP, wPos);	//world to view proj
				o.WPos			=wPos.xyz;						//world position
				o.UV0_TA.xy		=mVerts[id].UV0_TA.xy;			//copy UV
				o.UV0_TA.zw		=mVerts[id].UV0_TA.zw;			//copy alpha/thickness

				//Skinned meshes in unity bake the model matrix
				//in to the root bone, so the normal and tangent
				//are already transformed into worldspace.  I check
				//for this in code and just send identity for skinned,
				//so for a skinned mesh this will do nothing
				float3	norm	=mul(mObjectToWorld, mVerts[id].Normal);
				float3	tan		=mul(mObjectToWorld, mVerts[id].Tangent.xyz);

				o.Normal	=normalize(norm);
				o.Tangent	=float4(normalize(tan), mVerts[id].Tangent.w);

				return	o;
			}
			
			
			fixed4	FurPS(PSInput i) : SV_Target
			{
				//zero length fur?
				clip(i.UV0_TA.z);

				float2	uvPattern	=TRANSFORM_TEX(i.UV0_TA.xy, Pattern);
				float2	uvBase		=TRANSFORM_TEX(i.UV0_TA.xy, Color_Base);
				float2	uvTip		=TRANSFORM_TEX(i.UV0_TA.xy, Color_Tip);
				float2	uvGrowth	=TRANSFORM_TEX(i.UV0_TA.xy, Growth);

				//sample growth
				float4	growth	=tex2D(Growth, uvGrowth);

				//sample pattern
				float4	pattern	=tex2D(Pattern, uvPattern);

				float mask	=CreateAlpha(i.UV0_TA.zw, growth.x, pattern.x);

				//bail if below the mask threshold
				float	clippo	=mask - MaskClip;
				clip(clippo);

				//do tangent space
				//based on built in unity stuff from UnityCG.cginc
				float3	biNormal	=cross(normalize(i.Normal),
					normalize(i.Tangent.xyz)) * i.Tangent.w;

				float3x3	rot	=float3x3(i.Tangent.xyz, biNormal, i.Normal);

				rot	=transpose(rot);

				//sample colour base
				float4	base	=tex2D(Color_Base, uvBase);

				//tip
				float4	tip		=tex2D(Color_Tip, uvTip);


				float3	furColor	=FurColorStuff(base, tip,
					growth.x, pattern.x, i.UV0_TA.z,
					AO_Pattern, AO_Shell);

				furColor	=AnisoShade(i.Normal, furColor,
					AnisoEdgeBrighten, AnisoCenterDarken,
					UnityWorldSpaceViewDir(i.WPos));

				//calc light
				float3	light	=ComputeTrilight(i.Normal,
					_WorldSpaceLightPos0.xyz, _LightColor0.xyz,	//built in values
					TriColor1.xyz, TriColor2.xyz);				//material values

				float3	specular	=ComputeGoodSpecular(i.WPos,
					_WorldSpaceLightPos0.xyz,	//todo: check for directional/point
					i.Normal, light, TriColor2.xyz,
					mSpecPower, mSpecColor);

				furColor	*=light;

				furColor	=saturate(specular + furColor);

				return	fixed4(furColor, 1);
			}
			ENDCG
		}
	}
}
