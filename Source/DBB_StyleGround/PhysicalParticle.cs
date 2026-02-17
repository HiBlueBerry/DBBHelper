using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Backdrops;
using System;
using Celeste.Mod.DBBHelper.Mechanism;
using System.Collections.Generic;

namespace Celeste.Mod.DBBHelper.BackDrops
{
    [CustomBackdrop("DBBHelper/PhysicalParticle")]
    public class PhysicalParticle : Backdrop
    {
        private class Particle
        {
            //以下与粒子的形态有关
            public Vector2 basic_scale = Vector2.One;

            public float base_rotation = 0.0f;
            public float rotation = 0.0f;

            //以下和粒子的物理有关
            
            public Vector2 last_position = Vector2.Zero;//粒子上一次的位置
            public Vector2 current_position = Vector2.Zero;//粒子当前的位置
            public Vector2 acceleration = Vector2.Zero;//粒子的加速度
            private Vector2 velocity;//粒子的速度
            private Vector2 turbulence_acceleration=Vector2.Zero;

            public Particle(
                Vector2 init_acceleration,
                Vector2 turbulence_amplification,
                Vector2 init_velocity,
                Vector2 init_velocity_random_amp,
                Vector2 extend_support,
                Vector2 basic_scale,
                Vector2 basic_scale_random_amplify,
                bool uniform_scale_mode,
                bool generate_turbulence_when_init
            )
            {
                acceleration = init_acceleration;
                //随机生成粒子位置
                current_position = new Vector2(-32f + Calc.Random.NextFloat(384f + extend_support.X), -32f + Calc.Random.NextFloat(244f + extend_support.Y));
                //基于初始化的速度、加速度、位置来初始化上一次的位置

                Vector2 random_velocity = new Vector2((Calc.Random.NextFloat() - 0.5f) * init_velocity_random_amp.X * 2.0f, (Calc.Random.NextFloat() - 0.5f) * init_velocity_random_amp.Y * 2.0f);
                velocity = init_velocity + random_velocity;
                last_position = current_position - velocity * Engine.RawDeltaTime + 0.5f * init_acceleration * Engine.RawDeltaTime * Engine.RawDeltaTime;

                if (generate_turbulence_when_init==true)
                {
                    turbulence_acceleration = new Vector2((Calc.Random.NextFloat() - 0.5f) * turbulence_amplification.X * 2.0f, (Calc.Random.NextFloat() - 0.5f) * turbulence_amplification.Y * 2.0f);
                }
                //以下与粒子的形态有关
                if (uniform_scale_mode == true)
                {
                    //等比例缩放
                    float uniform_scale = (Calc.Random.NextFloat() - 0.5f) * basic_scale_random_amplify.Length() * 2.0f;
                    this.basic_scale = basic_scale + new Vector2(uniform_scale, uniform_scale);
                }
                else
                {
                    this.basic_scale = basic_scale + new Vector2((Calc.Random.NextFloat() - 0.5f) * basic_scale_random_amplify.X * 2.0f, (Calc.Random.NextFloat() - 0.5f) * basic_scale_random_amplify.Y * 2.0f);
                }
                base_rotation = Calc.Random.Range(-0.05f, 0.05f);
                rotation = base_rotation;
            }
            //更新粒子加速度
            private void Update_Acceleration(Vector2 acceleration, float damping, bool need_change_turbulence, Vector2 turbulence_amplification)
            {
                //如果需要，则产生一次噪声加速度
                if (need_change_turbulence == true)
                {
                    //产生满足二维均匀分布，每一维度满足U(-turbulence_amplification, turbulence_amplification)
                    turbulence_acceleration = new Vector2((Calc.Random.NextFloat() - 0.5f) * turbulence_amplification.X * 2.0f, (Calc.Random.NextFloat() - 0.5f) * turbulence_amplification.Y * 2.0f);
                }
                //产生基于速度的阻尼
                Vector2 drag_acceleration = -damping * velocity;
                this.acceleration = acceleration + turbulence_acceleration + drag_acceleration;
            }
            //更新粒子位置
            private void Update_Position()
            {
                //Verlet积分法更新粒子位置
                Vector2 next_position = DBBMath.Verlet_Integrate(current_position, last_position, Engine.DeltaTime, acceleration);
                last_position = current_position;
                current_position = next_position;
            }

            //松弛操作，用于约束粒子最大速度
            private void Relaxation(float max_velocity)
            {
                //计算粒子的速度
                Vector2 velocity = (current_position - last_position) / Engine.DeltaTime;
                float v = velocity.Length();
                if (v > max_velocity)
                {
                    current_position = last_position + velocity * (max_velocity / v) * Engine.DeltaTime;
                }
                //更新参考速度
                this.velocity = velocity;
            }
            private void Update_Shape()
            {
                //计算粒子缩放
                //delta_scale = (abs(velocity) - min_velocity_for_dynamic_scale) * 0.03;
                //float dynamic_scale_x = Math.Max(Math.Abs(velocity.X) - min_velocity_for_dynamic_scale.X, 0.0f) * 0.005f;
                //float dynamic_scale_y = Math.Max(Math.Abs(velocity.Y) - min_velocity_for_dynamic_scale.Y, 0.0f) * 0.005f;
                //scale = basic_scale * new Vector2(1.0f + dynamic_scale_x, 1.0f + dynamic_scale_y);
                //计算粒子角度
                if (velocity.Length() < 1.0f)
                {
                    return;
                }
                rotation = velocity.Angle();
            }
            //更新粒子位置
            public void Update(Vector2 acceleration, float damping, float max_velocity, bool need_change_turbulence, Vector2 turbulence_amplification)
            {
                //如果时间步长为0，则直接返回
                if (Engine.DeltaTime == 0.0f)
                {
                    return;
                }
                //更新粒子加速度
                Update_Acceleration(acceleration, damping, need_change_turbulence, turbulence_amplification);
                //更新粒子位置
                Update_Position();
                //松弛，约束最大速度
                Relaxation(max_velocity);
                //调整粒子形态
                Update_Shape();
                
            }
            //渲染粒子
            public void Render(Camera camera, MTexture texture, Color color, Vector2 scroll, Vector2 extend_support)
            {
                Vector2 render_position = new Vector2(DBBMath.Mod(current_position.X - camera.X * scroll.X, 320f + extend_support.X), DBBMath.Mod(current_position.Y - camera.Y * scroll.Y, 180f + extend_support.Y));
                if (texture == null)
                {
                    Draw.Pixel.DrawCentered(render_position, color, basic_scale, rotation);
                }
                else
                {
                    texture.DrawCentered(render_position, color, basic_scale, rotation);
                }
                
            }
        }
        private List<Particle> particles = new List<Particle>();

        //以下为通用设置
        public int amount = 180;//粒子数目
        public MTexture texture=null;//粒子的贴图
        public float alpha = 1.0f;//粒子的整体不透明度
        public Vector4 color = Vector4.One;//粒子的颜色
        public Vector2 scroll = Vector2.Zero;//粒子的视差
        public Vector2 extend_support = Vector2.Zero;//额外扩展的X轴值和Y轴值
        public Vector2 basic_scale = Vector2.One;//粒子的基础缩放
        public Vector2 basic_scale_random_amplify=Vector2.Zero;//粒子的基础缩放扰动范围
        public bool uniform_scale_mode = false;//粒子是否启用等比缩放模式

        //以下与粒子的物理有关
        public Vector2 init_velocity = Vector2.Zero;//粒子初始速度
        public Vector2 init_velocity_random_amp=Vector2.Zero;//粒子初始速度的随机扰动范围
        public Vector2 constant_acceleration = Vector2.Zero;//施加在粒子上的常量加速度
        public Vector2 turbulence_amplification = Vector2.Zero;//均匀噪声场的幅度
        public float turbulence_change_interval = 0.5f;//均匀噪声场经过多久时变更一次
        public bool generate_turbulence_when_init = true;//在初始化时是否产生噪声场
        public Vector2 wind_amplify_coeficient = Vector2.One;//粒子受到风速影响的系数
        public float max_velocity = 360.0f;//粒子最大速度
        public float damping = 0.0f;//阻尼系数

        private float turbulence_timer = 0.0f;//噪声场变更计时器
        public PhysicalParticle(BinaryPacker.Element data)
        {
            //以下是一些基本参数
            amount = data.AttrInt("Amount", 240);
            string particle_path = data.Attr("ParticleTexture");
            if (GFX.Game.Has(particle_path) == true)
            {
                texture = GFX.Game[data.Attr("ParticleTexture")];
            }
            color = DBBMath.ConvertColor(data.Attr("Color", "FFFFFF"));
            alpha = data.AttrFloat("Alpha", 1.0f);
            scroll = new Vector2(data.AttrFloat("ScrollX", 0.0f), data.AttrFloat("ScrollY", 0.0f));
            extend_support = new Vector2(data.AttrFloat("ExtX", 0.0f), data.AttrFloat("ExtY", 0.0f));

            basic_scale = new Vector2(data.AttrFloat("BasicScaleX", 1.0f), data.AttrFloat("BasicScaleY", 1.0f));
            basic_scale_random_amplify = new Vector2(data.AttrFloat("BasicScaleRandomAmpX", 0.0f), data.AttrFloat("BasicScaleRandomAmpY", 0.0f));
            uniform_scale_mode = data.AttrBool("UniformScaleMode", false);

            //以下与粒子的物理有关
            init_velocity = new Vector2(data.AttrFloat("InitVelocityX", 0.0f), data.AttrFloat("InitVelocityY", 0.0f));
            init_velocity_random_amp=new Vector2(data.AttrFloat("InitVelocityRandomAmpX", 0.0f), data.AttrFloat("InitVelocityRandomAmpY", 0.0f));
            constant_acceleration = new Vector2(data.AttrFloat("ConstantAccelerationX", 0.0f), data.AttrFloat("ConstantAccelerationY", 980.0f));
            turbulence_amplification = new Vector2(data.AttrFloat("TurbulenceAmplificationX", 0.0f), data.AttrFloat("TurbulenceAmplificationY", 0.0f));
            turbulence_change_interval = data.AttrFloat("TurbulenceChangeInterval", 0.5f);
            generate_turbulence_when_init= data.AttrBool("GenerateTurbulenceWhenInit", true);
            wind_amplify_coeficient = new Vector2(data.AttrFloat("WindAmplifyCoeficientX", 1.0f), data.AttrFloat("WindAmplifyCoeficientY", 1.0f));
            max_velocity = data.AttrFloat("MaxVelocity", 360.0f);
            damping = data.AttrFloat("Damping", 0.0f);
            //初始化粒子
            for (int i = 0; i < amount; i++)
            {
                particles.Add(
                    new Particle(
                        constant_acceleration,
                        turbulence_amplification,
                        init_velocity,
                        init_velocity_random_amp,
                        extend_support,
                        basic_scale,
                        basic_scale_random_amplify,
                        uniform_scale_mode,
                        generate_turbulence_when_init
                        )
                );
            }
        }
        private void UpdateAcceleration(Scene scene, Vector2 turbulence_amplification)
        {

            //更新均匀噪声场
            bool need_change_turbulence = false;
            if (turbulence_timer < turbulence_change_interval)
            {
                turbulence_timer += Engine.DeltaTime;
            }
            else
            {
                need_change_turbulence = true;
                turbulence_timer = 0.0f;
            }
            Vector2 acceleration = constant_acceleration + (scene as Level).Wind;
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Update(acceleration, damping, max_velocity, need_change_turbulence, turbulence_amplification);
            }
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            UpdateAcceleration(scene, turbulence_amplification);    
        }

        public override void Render(Scene scene)
        {
            //如不可见，则直接返回
            if (Visible == false)
            {
                return;
            }
            //获取相机位置，来做视差
            Camera camera = (scene as Level).Camera;
            Color tmp_color = new Color(color * alpha);
            //对于每个粒子
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Render(camera, texture, tmp_color, scroll, extend_support);
            }
        }
    }
    
}