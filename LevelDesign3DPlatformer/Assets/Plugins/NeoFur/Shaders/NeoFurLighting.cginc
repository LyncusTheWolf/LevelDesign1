#ifndef NEO_FUR_LIGHTING_INCLUDED
#define NEO_FUR_LIGHTING_INCLUDED


void DoFurAmbientOcclusion(inout SurfaceOutputStandard s)
{
	//fur alpha stored in occlusion from FurSurf
	float	furAlpha	=s.Occlusion;

	//ambient occlusion for fur nearer the object surface
	float	AO	=lerp(furAlpha, 1, (1.0 - _AOValue));

	s.Occlusion	=AO;	//store AO back in occlusion
}


//custom lighting function for forward
//mostly copied from Unity standard shader
half4 LightingFurStandard(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
{
	s.Normal = normalize(s.Normal);

	//rim to boost light around fur edges
	float rim = min(1, max(0, dot(s.Normal, viewDir)));
	float edge = pow(1.0 - rim, _RimLightExponent) * _FuzzBrightness;
	float darkened = 1.0 - _FuzzCenterDarkness * rim;
	float4 rimLight = _FuzzRimColor * edge + darkened;

	DoFurAmbientOcclusion(s);

	//standard lighting from unity
	half oneMinusReflectivity;
	half3 specColor;
	s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	// multiply in the edge lightening and core darkening according to mix factor
	s.Albedo = lerp(s.Albedo, s.Albedo * rimLight, _RimLightStrength);

	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

	half4 c = UNITY_BRDF_PBS (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
	c.a = outputAlpha;
	return c;
}


inline half4 LightingFurStandard_Deferred (SurfaceOutputStandard s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
{
	float rim = min(1, max(0, dot(s.Normal, viewDir)));
	float edge = pow(1.0 - rim, _RimLightExponent) * _FuzzBrightness;
	float darkened = 1.0 - _FuzzCenterDarkness * rim;
	float4 rimLight = _FuzzRimColor * edge + darkened;

	DoFurAmbientOcclusion(s);

	//standard lighting from unity
	half oneMinusReflectivity;
	half3 specColor;
	s.Albedo = DiffuseAndSpecularFromMetallic (s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

	// multiply in the edge lightening and core darkening according to mix factor
	s.Albedo = lerp(s.Albedo, s.Albedo * rimLight, _RimLightStrength);


	half4 c = UNITY_BRDF_PBS (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI (s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);

	outDiffuseOcclusion = half4(s.Albedo, s.Occlusion);
	outSpecSmoothness = half4(specColor, s.Smoothness);
	outNormal = half4(s.Normal * 0.5 + 0.5, 1);
	half4 emission = half4(s.Emission + c.rgb, 1);
	return emission;
}


inline void LightingFurStandard_GI (
	SurfaceOutputStandard s,
	UnityGIInput data,
	inout UnityGI gi)
{
	DoFurAmbientOcclusion(s);

	UNITY_GI(gi, s, data);
}
#endif