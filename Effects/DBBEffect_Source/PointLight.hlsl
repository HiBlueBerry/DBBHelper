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


//圆形的SDF
//uv为输入坐标
//center球体的中心位置
//extinction衰减速度
//aspect_ratio用于根据屏幕宽高比例进行UV的调整
float sdSphere(float2 uv,float2 center,float extinction,float aspect_ratio)
{
    //校正UV坐标以考虑宽高比
    uv.x *= aspect_ratio;
    center.x *= aspect_ratio;
    //获取距离光源的距离
    float distance = length(uv - center);
    //step(y,x)：如果后者大于等于前者则返回1.0，否则返回0.0
    return exp(-extinction*distance); 
}

//求菲涅尔项
//uv为输入坐标
//center球体中心
//sphere_radius为球体半径
//edgeWidth为边缘厚度
//F0为基础反射，如果不需要菲涅尔反射，则需要把这个值设为F0
//aspect_ratio用于根据屏幕宽高比例进行UV的调整
float FakeFresnel2D(float2 uv,float2 center,float sphere_radius,float edgeWidth, float F0,float aspect_ratio) {

    //校正UV坐标以考虑宽高比
    uv.x *= aspect_ratio;
    center.x *= aspect_ratio;

    //获取投影的方向向量
    float2 dir = uv-center;
    //计算球面的Z坐标
    float dir_length=length(dir);
    float z=sqrt(max(sphere_radius*sphere_radius-dir_length*dir_length,0.0));
    //计算击中点的法向量
    float3 hit_normal=normalize(float3(dir,z));
    //计算视线方向
    float3 view_ray=normalize(float3(0.5*aspect_ratio,0.5,0.5)-float3(uv,z));
    //计算角度和菲涅尔项
    float cosTheta = abs(dot(view_ray, hit_normal));
    return (1.0-F0)*pow(1.0-cosTheta,edgeWidth);
}

float2 center=float2(0.5,0.5);//点光源中心所在的位置
float extinction=10.0;//衰减速度
float sphere_radius=0.1;//球体半径，范围为0.0到0.5，摄像机的位置设定为了虚拟的(0.5,0.5,0.5)
float edge_width=5.0;//菲涅尔边缘宽度
float F0=1.0;//菲涅尔基础反照率，如果不需要菲涅尔效果就将其置为1.0
float brightness_amplify=1.0;//亮度增幅，用于增强或者削弱原版光照
float4 color=float4(0.2,0.8,1.0,1.0);//颜色
float aspect_ratio=1.78;//屏幕宽高比

float4 main(pInput pin):SV_TARGET
{
    float4 image=tex2D(sp,pin.UV);
    float4 tint_color=0;
    float atten=sdSphere(pin.UV,center,extinction,aspect_ratio);
    float fresnel_value=FakeFresnel2D(pin.UV,center,sphere_radius,edge_width,F0,aspect_ratio);
    tint_color=color.a*(1.0+fresnel_value)*atten*color*brightness_amplify;
    return tint_color;
}

technique PointLight {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}