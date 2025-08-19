float MieScatteringFunc(float3 lightDir, float3 rayDir)
{
	//MieScattering公式
	// (1 - g ^2) / (4 * pi * (1 + g ^2 - 2 * g * cosθ) ^ 1.5 )
	//_MieScatteringFactor.x = (1 - g ^ 2) / 4 * pai
	//_MieScatteringFactor.y =  1 + g ^ 2
	//_MieScatteringFactor.z =  2 * g
	float lightCos = dot(lightDir, -rayDir);
	return _MieScatteringFactor.x / pow((_MieScatteringFactor.y - _MieScatteringFactor.z * lightCos), 1.5);
}

//Beer-Lambert法则
float ExtingctionFunc(float stepSize, inout float extinction)
{
	float density = 1.0; //密度，暂且认为为1吧，可以采样3DNoise贴图
	float scattering = _ScatterFactor * stepSize * density;
	extinction += _ExtictionFactor * stepSize * density;
	return scattering * exp(-extinction);
}

float4 RayMarching(float3 rayOri, float3 rayDir, float rayLength)
{	
    //ori：相机位置
	//raydir：从相机到当前像素点对应世界坐标值的方向
	//rayLength：长度
	float delta = rayLength / RAYMARCHING_STEP_COUNT;
	float3 step = rayDir * delta;
	float3 curPos = rayOri + step;
	
	float totalAtten = 0;
	float extinction = 0;
	for(int t = 0; t < RAYMARCHING_STEP_COUNT; t++)
	{
		float3 tolight = (curPos - _VolumeLightPos.xyz);
		float atten = 1.0;
		float att = dot(tolight, tolight) * _MieScatteringFactor.w;
		atten *= tex2D(_LightTextureB0, att.rr).UNITY_ATTEN_CHANNEL;
		atten *= MieScatteringFunc(normalize(-tolight), rayDir);
		atten *= ExtingctionFunc(delta, extinction);
		
		totalAtten += atten;
		curPos += step;
	}
	
	float4 color = float4(totalAtten, totalAtten, totalAtten, totalAtten);
	return color * _TintColor;
}