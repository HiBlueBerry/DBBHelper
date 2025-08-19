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
Texture2D tex:register(t0);
CREATE_SAMPLER(sp,<tex>,0);//用于原图像的纹理采样

float2 RandWithUV(float2 p)
{
    float3 p3=frac(float3(p.xyx)*float3(0.1031,0.1030,0.0973));
    p3=p3+dot(p3,p3.yzx+19.19);
    return -1.0+2.0*frac((p3.xx+p3.yz)*p3.zy);
}
//简单的二维Perlin梯度噪声，scale为噪声图缩放，strength为噪声强度
//一种优化方式：Simplex噪声，生成噪声图的时间由O(2^N)转变为O(N^2)，其中N为维数
float Noise(float2 uv,float scale)
{
    //依据scale划分网格,scale越大划分的网格越密集
    float2 f=frac(uv*scale);
    float2 bottom_left=floor(uv*scale);
    float2 top_left=bottom_left+float2(0,1.0);
    float2 bottom_right=bottom_left+float2(1.0,0);
    float2 top_right=bottom_left+float2(1.0,1.0);
    //生成uv点所对应网格的四个角的随机方向向量

    float2 bottom_left_val=RandWithUV(bottom_left);
    float2 top_left_val=RandWithUV(top_left);
    float2 bottom_right_val=RandWithUV(bottom_right);
    float2 top_right_val=RandWithUV(top_right);
    //立方插值，用于平滑噪声图
    float2 final_f=f*f*(3.0-2.0*f);
    float commit1=dot(bottom_left_val,f);
    float commit2=dot(top_left_val,f-float2(0,1));
    float commit3=dot(bottom_right_val,f-float2(1,0));
    float commit4=dot(top_right_val,f-float2(1,1));
    //对uv点使用网格的四个角进行双线性插值
    float p1=commit1*(1.0-final_f.x)+commit3*final_f.x;
    float p2=commit2*(1.0-final_f.x)+commit4*final_f.x;
    return p1*(1.0-final_f.y)+p2*final_f.y;
}

float MieScatteringFunc2D(float g,float2 lightDir, float2 rayDir)
{
    float t1=(1.0f-g*g)*0.08f;
    float t2=1.0f+g*g;
    float t3=2.0f*g;
	float lightCos = dot(lightDir, -rayDir);
	return t1/pow((t2-t3 * lightCos), 1.5);
}
//Beer-Lambert法则
float ExtingctionFunc2D(float stepSize, inout float extinction,float extictionFactor)
{
	extinction+=extictionFactor*stepSize;
	return exp(-extinction);
}

//RayMarching
//light_pos光源位置
//light_radius光源最大半径
//origin待处理点的位置
//dir光在待处理点处的投射方向
//length光的最大投射距离
//step迭代步数
float RayMarching2D(float constant,float2 light_pos,float light_radius,float2 origin,float2 camera_pos,int iter,float g,float extictionFactor)
{
    //获取视线方向，即从待处理点到相机的射线
    float2 dir=camera_pos-origin;
    //获取视线的总距离和每次迭代的距离
    float ray_length=length(dir);
    float delta=ray_length/iter;
    //归一化视线为方向向量
    dir=normalize(dir);
    //开始沿着视线方向步进
    float2 light_step=dir*delta;
    float2 cur_pos=origin+light_step;
    //记录总亮度
    float total_atten=0.0f;
    //记录每次的衰减值
    float extinction=0.0f;

    //迭代
    for(int i=0;i<iter;i++)
    {
        //获取光线方向，即光源到待处理点的射线
        float2 to_light=cur_pos-light_pos;
        float2 to_camera=camera_pos-cur_pos;
		float att=dot(to_camera,to_camera);
        float atten=0.0f;

        atten=constant*smoothstep(1.0,0.0,att/(light_radius*light_radius));
        atten*=ExtingctionFunc2D(delta,extinction,extictionFactor);
        atten*=MieScatteringFunc2D(g,normalize(-to_light),dir);
        total_atten+=atten;
        cur_pos+=light_step;
    }
	return total_atten;
}

float time=0.0f;//时间，输入一个变化的值以产生变化的效果
float scale=1.5f;//噪声的缩放等级

float base_strength=0.5f;//光的基础强度，应该为一个大于0的值
float dynamic_strength=0.3f;//光的动态强度，光的强度将基于基础强度进行动态浮动

float2 emit_pos=float2(1.0,1.0);//光的发射位置
float2 probe_pos=float2(0.5,0.0);//光的探照位置，不要与emit_pos一样，至少应该有0.05的安全距离，probe_pos-emit_pos体现了光束的出射方向

float light_radius=0.5f;//光的参考强度半径，距离发射位置超过该半径的位置的光强度为0
int iter=5;//迭代次数，性能损耗项，迭代次数越多，光的散射效果(光的方向在传播过程中逐渐扭曲)和整体亮度将越强，如需要不散射效果，应将此值改为1并配以较大的base_strength，此值应当大于等于1

float concentration_factor=0.9;//聚光系数，应为0.01到0.99，值越大代表光束越聚集
float extingction_factor=2.0;//消减系数，值越大代表光强随距离衰减得越快

float4 color=float4(0.5,0.6,0.4,1.0);//光照颜色

float brightness_amplify=1.0f;//亮度增幅，用于增强或者削弱原版光照

float4 main(pInput pin):SV_TARGET
{
    float4 image=tex2D(sp,pin.UV);
    //左上角是(1,1)，右下角是(0,0)
    float4 tint_color=0;
    float total_atten=0.0;

    float2 dir=emit_pos-probe_pos;
    float origin_length=length(emit_pos-pin.UV);

    float dynamic_noise=Noise(pin.UV+dir*time,scale);
    total_atten=RayMarching2D(base_strength+dynamic_strength*dynamic_noise,probe_pos,light_radius,pin.UV,emit_pos,iter,concentration_factor,extingction_factor);
    tint_color=total_atten*color*brightness_amplify;
    return tint_color;
}
technique GodLight2D {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}
