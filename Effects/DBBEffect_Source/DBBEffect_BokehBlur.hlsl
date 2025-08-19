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
    float4 Color    : COLOR;
    float2 UV       : TEXCOORD;
};
texture blur_tex:register(t0);//用于原图像的纹理采样
texture mask_tex:register(t1);//用于掩码图像的纹理采样
CREATE_SAMPLER(sp,<blur_tex>,0);
CREATE_SAMPLER(mask_sp,<mask_tex>,1);

//散景模糊
float inner_radius=0.0;//光圈内半径
float interval=0.01;//光圈内外半径间距
int iter=8;//螺旋线条数

float4 main(pInput pin):SV_TARGET
{

    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float rotate_angle=6.2832/iter;//基于基准线条数获取基准旋转角，弧度制
    float spiral_rotate=0.524;//螺旋线旋转角，弧度制，此处为30度
    float c=cos(rotate_angle);
    float s=sin(rotate_angle);
    float2x2 base_rotation={c,-s,s,c};//基准旋转矩阵
    c=cos(spiral_rotate);
    s=sin(spiral_rotate);
    float2x2 spiral_rotation={c,-s,s,c};//螺旋线旋转矩阵
    float2 base=float2(0,1);//螺旋线的初始基准线

    float4 finalColor=tex2D(sp,pin.UV);
	float4 divisor=1.0;

    //对于每条螺旋线
    for(int i=0;i<iter;i++)
    {
        float2 spiral=base;
        //采样单条螺旋线上的点
        for(int j=0;j<6;j++)
        {
            float proption=j/6.0;
            float4 bokeh=tex2D(sp,pin.UV+spiral*(inner_radius+proption*interval));
            spiral=mul(spiral_rotation,spiral);
            finalColor+=bokeh;
            divisor+=1.0;
        }
        base=mul(base_rotation,base);
    }
    return lerp(tex2D(sp,pin.UV),finalColor/divisor,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效;
}
technique BokehBlur {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}