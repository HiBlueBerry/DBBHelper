using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Backdrops;
using System;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.BackDrops
{
    [CustomBackdrop("DBBHelper/ProgramSky")]
    public class ProgramSky:Backdrop
    {
        //控制时间变化
        private float time=0.0f;//分段插值用的时间
        private float real_time=0.0f;//实际上的时间
        //控制时间变化速度
        private float velocity=0.1f;
        //五组基础天空颜色
        // 夜间 - 深蓝色调 - 深蓝黑 暗蓝色 稍亮的夜空蓝
        Vector4[] night_color=[new Vector4(0.02f, 0.03f, 0.12f, 1.0f),new Vector4(0.05f, 0.07f, 0.18f, 1.0f),new Vector4(0.08f, 0.12f, 0.25f, 1.0f)];

        // 凌晨 - 蓝紫色调 - 深蓝紫 紫罗兰色 亮紫色
        Vector4[] dawn_color=[new Vector4(0.1f, 0.1f, 0.25f, 1.0f),new Vector4(0.25f, 0.15f, 0.35f, 1.0f),new Vector4(0.4f, 0.25f, 0.5f, 1.0f)];

        // 清晨 - 橙红色调 - 过渡色 橙红色 浅橙红
        Vector4[] morning_color=[new Vector4(0.3f, 0.2f, 0.4f, 1.0f),new Vector4(0.9f, 0.5f, 0.3f, 1.0f),new Vector4(0.98f, 0.7f, 0.5f, 1.0f)];

        // 日间 - 蓝色调 - 浅天蓝 标准天蓝 深天蓝
        Vector4[] day_color=[new Vector4(0.4f, 0.6f, 0.9f, 1.0f),new Vector4(0.2f, 0.5f, 0.8f, 1.0f),new Vector4(0.1f, 0.3f, 0.7f, 1.0f)];

        // 黄昏 - 红黄色调 - 深橙色 橙红色 紫红色
        Vector4[] dusk_color=[new Vector4(0.8f, 0.4f, 0.2f, 1.0f),new Vector4(0.9f, 0.5f, 0.3f, 1.0f),new Vector4(0.6f, 0.3f, 0.5f, 1.0f)];
        
        float stage1=0.5f;float stage2=0.6f;float stage3=0.9f;float stage4=2.2f;float stage5=2.4f;
        int stage=0;

        Vector4[] color1=[Vector4.One,Vector4.One,Vector4.One];//用于送入shader
        Vector4[] color2=[Vector4.One,Vector4.One,Vector4.One];//用于送入shader
        public ProgramSky(BinaryPacker.Element data)
        {
            stage=0;
            velocity=data.AttrFloat("Velocity");
            stage1=data.AttrFloat("NightLastTime");
            stage2=stage1+data.AttrFloat("DawnLastTime");
            stage3=stage2+data.AttrFloat("MorningLastTime");
            stage4=stage3+data.AttrFloat("DayLastTime");
            stage5=stage4+data.AttrFloat("DuskLastTime");

            string[] all_night_color=data.Attr("NightColor").Split([',']);
            string[] all_dawn_color=data.Attr("DawnColor").Split([',']);
            string[] all_morning_color=data.Attr("MorningColor").Split([',']);
            string[] all_day_color=data.Attr("DayColor").Split([',']);
            string[] all_dusk_color=data.Attr("DuskColor").Split([',']);
            for(int i=0;i<3;i++)
            {
                night_color[i]=DBBMath.ConvertColor(all_night_color[i]);
                dawn_color[i]=DBBMath.ConvertColor(all_dawn_color[i]);
                morning_color[i]=DBBMath.ConvertColor(all_morning_color[i]);
                day_color[i]=DBBMath.ConvertColor(all_day_color[i]);
                dusk_color[i]=DBBMath.ConvertColor(all_dusk_color[i]);
            }
        }
        private float SmoothStep(float start,float end, float time)
        {
            float x=Math.Clamp((time - start)/(end - start),0.0f, 1.0f); 
            return x*x*(3.0f-2.0f*x);
        }
        private void UpdateParameter()
        {
            real_time+=velocity*Engine.DeltaTime;
            if(stage==0)
            {
                for(int i=0;i<3;i++)
                {
                    color1[i]=night_color[i];
                    color2[i]=dawn_color[i];
                }
                if(real_time<=stage1)
                {
                    time=SmoothStep(0,stage1,real_time);
                }
                else
                {
                    stage=1;
                }
            }
            else if(stage==1)
            {
                for(int i=0;i<3;i++)
                {
                    color1[i]=dawn_color[i];
                    color2[i]=morning_color[i];
                }
                if(real_time<=stage2)
                {
                    time=SmoothStep(stage1,stage2,real_time);
                }
                else
                {
                    stage=2;
                }
            }
            else if(stage==2)
            {
                for(int i=0;i<3;i++)
                {
                    color1[i]=morning_color[i];
                    color2[i]=day_color[i];
                }
                if(real_time<=stage3)
                {
                    time=SmoothStep(stage2,stage3,real_time);
                }
                else
                {
                    stage=3;
                }
            }
            else if(stage==3)
            {
                for(int i=0;i<3;i++)
                {
                    color1[i]=day_color[i];
                    color2[i]=dusk_color[i];
                }
                if(real_time<=stage4)
                {
                    time=SmoothStep(stage3,stage4,real_time);
                }
                else
                {
                    stage=4;
                }
            }
            else if(stage==4)
            {
                for(int i=0;i<3;i++)
                {
                    color1[i]=dusk_color[i];
                    color2[i]=night_color[i];
                }
                if(real_time<=stage5)
                {
                    time=SmoothStep(stage4,stage5,real_time);
                }
                else
                {
                    stage=0;
                    real_time=0.0f;
                }
            }
            real_time+=velocity*Engine.DeltaTime;            
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            UpdateParameter();
    
        }
        public void SetParameter()
        {
            
            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["color1_bottom"].SetValue(color1[0]);
            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["color1_mid"].SetValue(color1[1]);
            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["color1_top"].SetValue(color1[2]);

            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["color2_bottom"].SetValue(color2[0]);
            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["color2_mid"].SetValue(color2[1]);
            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["color2_top"].SetValue(color2[2]);

            DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"].Parameters["time"].SetValue(time);


        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            SetParameter();
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBProgramSky_BaseColor"], Matrix.Identity);
            DBBEffectSourceManager.DefaultTexture.Draw(Vector2.Zero);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            
        }
    }
}