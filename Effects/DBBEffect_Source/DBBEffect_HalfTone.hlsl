#define CREATE_SAMPLER(name, texture,index) \
sampler name:register(s##index)= sampler_state \
{ \
    Texture = texture; \
    MinFilter = Linear; \
    MagFilter = Linear; \
    MipFilter = Linear; \
    AddressU = Wrap; \
    AddressV = Wrap; \
}  
struct pInput
{
    vector position : SV_POSITION;
    float2 UV       : TEXCOORD0;
};

texture effect_tex:register(t0);//用于原图像的纹理采样
CREATE_SAMPLER(sp,<effect_tex>,0);

float circle_mask(float2 uv, float2 center, float radius,float width)
{
    //anti-alising
    return smoothstep(radius, radius+width, length(uv-center));
}

float dot_width=0.01;//单个区域的宽度，每个区域内产生一个圆点，进而反映了单个圆点的平均宽度
float2 dot_center=float2(0.5,0.5);//圆点的局部圆心位置
float grey_threshold_start=0.05;//检测灰度的起始阈值，圆点的大小会随着灰度的大小而动态变化，以形成不规则圆点的效果
float grey_threshold_end=0.3;//检测灰度的终止阈值，圆点的大小会随着灰度的大小而动态变化，以形成不规则圆点的效果
float dot_detail=0.2;//圆点的细节度，它可以反映smoothstep的变化速度，绝对值越大时smoothstep越平滑的变化，为0时smoothstep成为阶跃函数

//半调效果
float4 main(pInput pin):SV_TARGET
{
    // normalized pixel coordinates (from 0 to 1)
    float4 texture_col=tex2D(sp,pin.UV);

    // circle mask
    float grey=dot(texture_col.rgb,float3(0.3,0.6,0.1));
    float2 uv=float2(fmod(pin.UV.x, dot_width)/dot_width,fmod(pin.UV.y, dot_width)/dot_width);
    float mask=circle_mask(uv, dot_center, lerp(grey_threshold_start,grey_threshold_end,grey), dot_detail);
    //color
    return float4(texture_col.rgb*(1.0-mask), texture_col.a);
}
technique HalfTone{
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}