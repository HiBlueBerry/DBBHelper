using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
namespace Celeste.Mod.DBBHelper.Entities {
    [CustomEntity("DBBHelper/AdvancedInvisibleLight")]
    public class InvisibleLightAdvanced : Entity
    {
        private VertexLight m_light;//光源
        private BloomPoint m_bloom;//泛光点
        private Vector4 color1;//第一种颜色
        private Vector4 color2;//第二种颜色

        private Vector4 current_color = new Vector4(0, 0, 0, 1);//当前颜色
        private float time=0.0f;//启用相关的计时器
        private float disable_time=0.0f;//禁用相关的计时器
        private float history_time=0.0f;//禁用时需要依据此时间来调整time的值
        private float speed=1.0f;//颜色变化速度
        private float acceleration=1.0f;//启动和禁用时的颜色变化速度
        private bool go=true;//是前往终点还是返回原点
        private string label;//是否启用该光源
        private bool revert=false;
        private bool disable=false;//是否被禁用

        public InvisibleLightAdvanced(EntityData data,Vector2 offset)
        {
            Position=data.Position+offset;

            float bloomradius=data.Float("bloomRadius");
            int light_startfade=data.Int("startFade");
            int light_endfade=data.Int("endFade");

            color1=DBBMath.ConvertColor(data.Attr("lightColor1"));
            color2=DBBMath.ConvertColor(data.Attr("lightColor2"));
            speed =data.Float("speed");
            acceleration=data.Float("acceleration");
            label=data.Attr("label");
            revert=data.Bool("revert");

            m_light=new VertexLight(Color.Black,1.0f,light_startfade,light_endfade);
            m_bloom=new BloomPoint(1.0f,bloomradius);
            TransitionListener handle_alpha=new TransitionListener();
            handle_alpha.OnOut=delegate(float f)
            {
                float time=(float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(f,"easeInOutSin"),1.0f,0.0f);

                m_light.Color = new Color(time * current_color);
            };
            Add(handle_alpha);
            Add(m_light);
            Add(m_bloom);
        }
        private void ChangeColor(float t, Vector4 color1, Vector4 color2)
        {
            Vector4 tmp_color = color1 * (1.0f - t) + color2 * t;
            m_light.Color = new Color(tmp_color);
        }
        public override void Update()
        {
            base.Update();
            if(revert==false)
            {
                disable=(Scene as Level).Session.GetFlag(label);
            }
            else
            {
                disable=!(Scene as Level).Session.GetFlag(label);
            }
            //如果未被禁用
            if(!disable)
            {   //初始化disable_time
                disable_time=0.0f;
                //此时处于慢启用阶段
                if(time<1.0f)
                {
                    time+=Engine.DeltaTime*acceleration;
                    go=true;
                    if(time>1.0f)
                    {
                        time=1.0f;
                    }
                    ChangeColor(time, new Vector4(0, 0, 0, 1), color1);
                }
                //此时处于运行状态
                else
                {
                    if(go==true)
                    {
                        time+=Engine.DeltaTime*speed;
                        if(time>2.0f)
                        {
                            time=2.0f;
                            go=false;
                        }
                    }
                    else
                    {
                        time-=Engine.DeltaTime*speed;
                        if(time<1.0f)
                        {
                            time=1.0f;
                            go=true;
                        }
                    }
                    ChangeColor(time-1.0f,color1,color2);
                }
                current_color = new Vector4(m_light.Color.R / 255.0f, m_light.Color.G / 255.0f, m_light.Color.B / 255.0f, m_light.Color.A / 255.0f);
                history_time =time;
            }
            //如果被禁用
            else
            {   
                //由当前颜色过渡到黑色
                disable_time+=Engine.DeltaTime*acceleration;
                if(disable_time>1.0f)
                {
                    disable_time=1.0f;
                }
                ChangeColor(disable_time, current_color, new Vector4(0, 0, 0, 1));
                //禁用的时候掉time的进度，最终掉到0
                time=history_time*(1.0f-disable_time);
            }
        }
//------------------------原理图------------------------//

//       Black------------color1-----------color2      //       
//time:   0.0              1.0              2.0        //
//               ——>                <——>               //
//          enable_phase1      enable_phase2           //
//                        <————                        //
//                     disable_phase                   //
//tips:
//  1.禁用阶段对于time的改变只有一个方向
//  2.慢启动阶段对于time的改变只有一个方向
//  3.运行阶段对于time的改变有两个方向
    }

}