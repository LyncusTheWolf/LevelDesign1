#ifndef KENS_COMMON_FUNCTIONS_INCLUDED
#define KENS_COMMON_FUNCTIONS_INCLUDED


float3 ComputeCheapSpecular(float3 wpos, float3 lightDir, float3 pnorm,
	float3 lightVal, float3 fillLight, float specPower, float specColor)
{
	float3	eyeVec	=normalize(_WorldSpaceCameraPos - wpos);
	float3	halfVec	=normalize(eyeVec + lightDir);
	float	ndotv	=saturate(dot(eyeVec, pnorm));
	float	ndoth	=saturate(dot(halfVec, pnorm));

	float	normalizationTerm	=(specPower + 2.0f) / 8.0f;
	float	blinnPhong			=pow(ndoth, specPower);
	float	specTerm			=normalizationTerm * blinnPhong;
	
	float3	specular	=specTerm * lightVal * fillLight;

	return	specular;
}


float3 ComputeGoodSpecular(float3 wpos, float3 lightDir, float3 pnorm,
	float3 lightVal, float3 fillLight, float specPower, float3 specColor)
{
	float3	eyeVec	=normalize(_WorldSpaceCameraPos - wpos);
	float3	halfVec	=normalize(eyeVec + lightDir);
	float	ndotv	=saturate(dot(eyeVec, pnorm));
	float	ndoth	=saturate(dot(halfVec, pnorm));

	float	normalizationTerm	=(specPower + 2.0f) / 8.0f;
	float	blinnPhong			=pow(ndoth, specPower);
	float	specTerm			=normalizationTerm * blinnPhong;
	
	//fresnel stuff
	float	base		=1.0f - dot(halfVec, lightDir);
	float	exponential	=pow(base, 5.0f);
	float3	fresTerm	=specColor + (1.0f - specColor) * exponential;

	//vis stuff
	float	alpha	=1.0f / (sqrt((UNITY_PI / 4.0) * specPower + (UNITY_PI / 2.0)));
	float	visTerm	=(lightVal * (1.0f - alpha) + alpha) *
				(ndotv * (1.0f - alpha) + alpha);

	visTerm	=1.0f / visTerm;

	float3	specular	=specTerm * lightVal * fresTerm * visTerm * fillLight;

	return	specular;
}

//compute the 3 light effects on the vert
//see http://home.comcast.net/~tom_forsyth/blog.wiki.html
float3 ComputeTrilight(float3 normal, float3 lightDir, float3 c0, float3 c1, float3 c2)
{
    float3	totalLight;
	float	LdotN	=dot(normal, lightDir);
	
	//trilight
	totalLight	=(c0 * max(0, LdotN))
		+ (c1 * (1 - abs(LdotN)))
		+ (c2 * max(0, -LdotN));
		
	return	totalLight;
}


//fur noise AO
float	FurNoiseAO(float aoPattern, float patternVal)
{
	float	oneMinusAO	=1.0 - aoPattern;

	float	pat1	=lerp(1.0, oneMinusAO, patternVal);

	return	lerp(patternVal, 1.0, pat1);
}


//Fur Shell AO
//thicken is pow(ShellAlpha, ThickenAmount) from the compute shader
float	FurShellAO(float aoShell, float growthVal, float patternVal, float thicken)
{
	float	shellVal	=(3.0 * aoShell) * growthVal * patternVal;

	return	lerp(1.0, thicken, shellVal);
}


//blend fur color with shell-depth color and noise for depth - except in the growth mask
//thicken is pow(ShellAlpha, ThickenAmount) from the compute shader
float3	FurColorStuff(float4 colorBase, float4 colorTip, float growth, float pattern,
	float thicken, float aoPat, float aoShell)
{
	float	growthClamped	=clamp(growth, 0, 1);

	float4	color	=lerp(colorBase, colorTip, thicken);

	float	ao	=FurNoiseAO(aoPat, pattern) *
					FurShellAO(aoShell, growthClamped, pattern, thicken);

	color	*=ao;

	return	color.xyz;
}

//create alpha
//shellStuff value here from the compute shader is:
//x is pow(ShellAlpha, ThickenAmount);
//y is ShellAlpha or float(k) / float(In_ShellCount - 1) where k is current shell index
//returns the mask used to clip() the fur
float CreateAlpha(float2 shellStuff, float growthVal, float patternVal)
{
	float	growthClamped	=clamp(growthVal, 0, 1);

	float	thicken	=1.0 - shellStuff.x;

	float	flooredThicken	=1.0 - floor(thicken);

	float	twiceThicken	=thicken * 2.0;

	float	growthPattern	=growthClamped * patternVal;

	float	thickGrowPat	=twiceThicken * growthPattern;

	return	thickGrowPat * flooredThicken;
}


//boost light for near edge on surfaces
float3	AnisoShade(float3 norm, float3 baseColor, float edgeBright, float centerDark, float3 camVec)
{
	camVec	=normalize(camVec);

	float	camDot	=dot(camVec, norm);

	camDot	=clamp(camDot, 0, 1);

	float	edgeFactor	=(1.0 - camDot) * edgeBright;

	return	(baseColor * centerDark) + (baseColor * edgeFactor * edgeFactor);
}
#endif