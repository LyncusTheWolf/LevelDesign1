Shader "NeoFurDebug/DotRay"
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
			#pragma	geometry	DotRayGS
			#pragma fragment	DotRayPS
			
			#include "UnityCG.cginc"

			struct VSInput
			{
				float3	Position	: POSITION;
				float3	RayVec		: TEXCOORD0;
			};

			StructuredBuffer<VSInput>	mVerts;
			
			struct GSInput
			{
				float4	Position	: SV_POSITION;
				float4	RayVec		: TEXCOORD0;
			};

			struct PSInput
			{
				float4	Position	: SV_POSITION;
				float4	Color		: COLOR0;
			};

			//misc shader vars
			float4		DotColor;	//color of the dots
			float4		RayColor;	//color of the ray
			float		DotSize;	//size of the dots
			float4x4	mModelMat;	//fake _Object2World since it sometimes doesn't get set
			float		isDelta;	//second element pos or delta?


			GSInput	DotRayVS(uint id : SV_VertexID, uint inst : SV_InstanceID)
			{
				GSInput	o;

				float3	pos	=mVerts[id].Position;
				float3	ray	=mVerts[id].RayVec;

				float3	rayEnd	=lerp(ray, pos + ray, isDelta);

				float4	pos0	=float4(pos, 1);
				float4	pos1	=float4(rayEnd, 1);

				//need a transform to get to world space
				o.Position	=mul(mModelMat, pos0);
				o.RayVec	=mul(mModelMat, pos1);

				return	o;
			}


			[maxvertexcount(10)]
			void DotRayGS(point GSInput p[1], inout TriangleStream<PSInput> triStream)
			{
				PSInput	o	=(PSInput)0;

				float4	screenPos	=mul(UNITY_MATRIX_VP, p[0].Position);
				float4	screenVel	=mul(UNITY_MATRIX_VP, p[0].RayVec);

				float3	upVec		=float3(0, 1, 0);
				float3	rightVec	=float3(1, 0, 0);

				float4 v[6];

				//control point as a billboard facing cam
				v[0]	=float4(screenPos + DotSize * rightVec - DotSize * upVec, screenPos.w);
				v[1]	=float4(screenPos + DotSize * rightVec + DotSize * upVec, screenPos.w);
				v[2]	=float4(screenPos - DotSize * rightVec - DotSize * upVec, screenPos.w);
				v[3]	=float4(screenPos - DotSize * rightVec + DotSize * upVec, screenPos.w);

				//make control point position dot
				for(int i=0;i < 4;i++)
				{
					o.Position	=v[i];
					o.Color		=DotColor;
					triStream.Append(o);
				}				
				triStream.RestartStrip();

				v[0]	=float4(screenPos + DotSize * rightVec - DotSize * upVec, screenPos.w);
				v[1]	=float4(screenPos - DotSize * rightVec + DotSize * upVec, screenPos.w);
				v[2]	=screenVel;
				v[3]	=float4(screenPos + DotSize * rightVec + DotSize * upVec, screenPos.w);
				v[4]	=float4(screenPos - DotSize * rightVec - DotSize * upVec, screenPos.w);
				v[5]	=screenVel;

				//make ray
				for(i=0;i < 6;i++)
				{
					o.Position	=v[i];
					o.Color		=RayColor;
					triStream.Append(o);
				}				
			}


			fixed4	DotRayPS(PSInput i) : SV_Target
			{
				return	i.Color;
			}
			ENDCG
		}
	}
}
