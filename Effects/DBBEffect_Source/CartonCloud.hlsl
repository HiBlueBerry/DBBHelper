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
texture tex:register(t0);
texture noise_tex:register(t1);
texture light_mask:register(t2);

CREATE_SAMPLER(sp,<tex>,0);//用于原图像的纹理采样
CREATE_SAMPLER(noise_sp,<noise_tex>,1);//用于原图像的纹理采样
CREATE_SAMPLER(light_sp,<light_mask>,2);//用于光照贴图的纹理采样
//纹理噪声
float texture_noise(float2 uv, float2 sample_detail){
    //网格化
    float2 f = frac(uv);
    float2 p = floor(uv);
    float2 detail = 1.0/sample_detail;
    //取单个网格四个点的值,detail控制网格密度
	float a = tex2D(noise_sp,(p+float2(0.0,0.0))*detail).x;
	float b = tex2D(noise_sp,(p+float2(1.0,0.0))*detail).x;
	float c = tex2D(noise_sp,(p+float2(0.0,1.0))*detail).x;
	float d = tex2D(noise_sp,(p+float2(1.0,1.0))*detail).x;
    //双线性插值
    float2 u = f*f*f*(f*(f*6.0-15.0)+10.0);
	return a+(b-a)*u.x+(c-a)*u.y+(a-b-c+d)*u.x*u.y;
}
//uv为输入坐标
//sample_detail为网格采样细节度
//fre为分形频率
//strength为噪声强度
//num为分形次数
float noise_fbm(float2 uv,float2 sample_detail,float fre,float strength,int num)
{
    float noise_sum=0.0;
    //这个f_amp值为1.0，设置为多少都不会对视觉产生影响
    float f_amp=1.0;
    float count=0.0;
    float f_fre=fre;
    for (int i = 0; i < num; i++) {
        //噪声累计值
        noise_sum+=f_amp*texture_noise(uv,sample_detail);
        //分形
        count+=f_amp;
        f_amp*=strength;
        uv*=fre;
    }
    //计算平均值
    return noise_sum/count;
}
//uv为输入坐标
//sample_detail为网格采样细节度
//fre为分形频率
//strength为噪声强度
//num为分形次数
//baseline_height为云层基础高度
//height_scale为云层高度起伏的缩放
//layer_color1-5为云层颜色
//layer_h1-5为不同云层的阈值高度
float4 background(
    float2 base_uv, float2 offest, float2 sample_detail, float fre, float strength, float num,
    float baseline_height, float height_scale,
    float4 layer_color1, float layer_h1,
    float4 layer_color2, float layer_h2,
    float4 layer_color3, float layer_h3,
    float4 layer_color4, float layer_h4,
    float4 layer_color5
)
{
    float2 uv=base_uv;
    uv.y=1.0 - base_uv.y;
    float h=(noise_fbm(uv+offest, sample_detail,fre,strength,num)-baseline_height)*height_scale;
    if (uv.y<h+layer_h1) return layer_color1;
    if (uv.y<h+layer_h2) return layer_color2;
    if (uv.y<h+layer_h3) return layer_color3;
    if (uv.y<h+layer_h4) return layer_color4;
    return layer_color5;
}

float2 offset=0.0f;//偏移量, 可以在外部提供速度控制
float2 sample_detail=float2(512.0,512.0);//网格采样细节度
float fre=2.0;//分形频率
float strength=0.8;//噪声强度
int num=4;//分形次数
float baseline_height=0.0;//云层基础高度
float height_scale=0.4;//云层高度起伏的缩放
//以下控制光照交互
float bloom_contrast=4.0;//辉光对比度
float bloom_strength=1.5;//辉光强度
float light_influence_coefficient=1.0;//光照影响系数
//以下控制云层颜色
float4 layer_color1=float4(0.92, 0.85, 0.82, 1.0);float layer_h1=0.01;
float4 layer_color2=float4(0.98, 0.76, 0.64, 1.0);float layer_h2=0.23;
float4 layer_color3=float4(0.95, 0.80, 0.77, 1.0);float layer_h3=0.35;
float4 layer_color4=float4(0.72, 0.85, 0.82, 1.0);float layer_h4=0.39;
float4 layer_color5=float4(0.92, 0.85, 0.82, 1.0);

float4 main(pInput pin):SV_TARGET
{
    float4 image=tex2D(sp,pin.UV);
    float4 col=background(
        pin.UV, offset, sample_detail, fre, strength, num,
        baseline_height, height_scale,
        layer_color1, layer_h1,
        layer_color2, layer_h2,
        layer_color3, layer_h3,
        layer_color4, layer_h4,
        layer_color5
    );
    //获取光照颜色
    float3 light_col=tex2D(light_sp, pin.UV).rgb;
    //计算光照贴图的图像亮度
    float light_strength = length(light_col)/1.732;
    //辉光
    float3 glowColor = light_col * pow(light_strength, bloom_contrast) * bloom_strength;
    col.rgb += glowColor;
    //应用光照对云层的消散效果
    col.a *= saturate(1.0 - light_strength * light_influence_coefficient);
    col.rgb *= col.a;
    return col;
}
technique CartonCloud {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}