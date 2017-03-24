Shader "NeoFurDebug/CPUDotRayNoGeom"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex		DotRayVS
			#pragma fragment	DotRayPS
			
			#include "UnityCG.cginc"

			struct VSInput
			{
				float3	Position	: POSITION;
				float4	Color		: COLOR0;
			};

			struct PSInput
			{
				float4	Position	: SV_POSITION;
				float4	Color		: COLOR0;
			};

			//misc shader vars
			float4x4	mModelMat;	//fake _Object2World since it sometimes doesn't get set


			PSInput	DotRayVS(VSInput v)
			{
				PSInput	o;

				float4	pos	=mul(mModelMat, float4(v.Position, 1));

				o.Position	=mul(UNITY_MATRIX_VP, pos);
				o.Color		=v.Color;

				return	o;
			}


			fixed4	DotRayPS(PSInput i) : SV_Target
			{
				return	i.Color;
			}
			ENDCG
		}
	}
}
