using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/GodLight2D")]
    [TrackedAs(typeof(DBBGeneralLight))]
    public class GodLight2D : DBBGeneralLight
    {
        //以下为参数项
        private float velocity = 0.5f;//动态变化的速度
        private float scale = 1.5f;//噪声的缩放等级

        private float base_strength = 0.5f;//光的基础强度，应该为一个大于0的值
        private float tmp_base_strength = 0.0f;
        private float dynamic_strength = 0.3f;//光的动态强度，光的强度将基于基础强度进行动态浮动

        private Vector2 emit_pos = new Vector2(1.0f, 1.0f);//光的发射位置
        private Vector2 probe_pos = new Vector2(0.5f, 0.0f);//光的探照位置，不要与emit_pos一样，至少应该有0.05的安全距离，probe_pos-emit_pos体现了光束的出射方向
        private Vector2 Emit_ParallaxProportion = Vector2.Zero;//光发射源位置的滚动系数
        private Vector2 Probe_ParallaxProportion = Vector2.Zero;//光探照位置的滚动系数

        private float light_radius = 0.5f;//光的参考强度半径，距离发射位置超过该半径的位置的光强度为0
        private int iter = 5;//迭代次数，性能损耗项，迭代次数越多，光的散射效果(光的方向在传播过程中逐渐扭曲)和整体亮度将越强，如需要不散射效果，应将此值改为1并配以较大的base_strength，此值应当大于等于1

        private float concentration_factor = 0.9f;//聚光系数，应为0.01到0.99，值越大代表光束越聚集
        private float extingction_factor = 2.0f;//消减系数，值越大代表光强随距离衰减得越快
        private float brightness_amplify = 1.0f;//Alpha通道增强

        private Vector4 color = new Vector4(0.5f, 0.6f, 0.4f, 1.0f);//光照颜色
        private Vector4 ref_color = new Vector4(0.5f, 0.6f, 0.4f, 1.0f);//光照颜色
        public bool DisableGodLight = false;//是否禁止绘制实体光
        private bool bad_light = false;//是否是坏光
        private float time = 0.0f;//计时器，应当不断变化以产生动态光

        //以下为一些临时记录内容
        private Vector2 probe_Position = Vector2.Zero;//光的探照位置
        private Vector2 tmp_emit_pos = Vector2.Zero;
        private Vector2 tmp_probe_pos = Vector2.Zero;
        private Vector2 tmp_emit_parallaxProportion = Vector2.Zero;
        private Vector2 tmp_probe_parallaxProportion = Vector2.Zero;
        private bool is_out = false;//是否正在过渡出场景
        private Vector2 ref_cameraPos_when_out = Vector2.Zero;//过渡出场景时的相机位置

        public GodLight2D(EntityData data, Vector2 offset)
        {
            //通用设置
            Position = data.Position + offset;
            velocity = data.Float("Velocity");
            scale = data.Float("Scale");
            base_strength = data.Float("BaseStrength");
            dynamic_strength = data.Float("DynamicStrength");
            light_radius = data.Float("LightRadius");
            iter = data.Int("Iter");
            concentration_factor = data.Float("ConcentrationFactor");
            extingction_factor = data.Float("ExtingctionFactor");
            color = DBBMath.ConvertColor(data.Attr("Color"));
            color.W = data.Float("Alpha");
            ref_color = color;
            brightness_amplify = data.Float("BrightnessAmplify");
            DisableGodLight = data.Bool("OnlyEnableOriginalLight");//仅启用原版光照
            //设置光源发射位置，这里是游戏内的位置进行归一化
            Vector2 temp = data.Position + offset;
            emit_pos = new Vector2(temp.X / 320.0F, temp.Y / 180.0f);
            Emit_ParallaxProportion = new Vector2(-data.Float("EmitScrollX", 0.0f), data.Float("EmitScrollY", 0.0f));
            //设置光源探照位置，这里是游戏内的位置进行归一化
            temp = data.Nodes[0] + offset;
            probe_Position = temp;
            probe_pos = new Vector2(temp.X / 320.0F, temp.Y / 180.0f);
            Probe_ParallaxProportion = new Vector2(-data.Float("ProbeScrollX", 0.0f), data.Float("ProbeScrollY", 0.0f));
            //进行光半径测试，太短的光不要留
            float test_length = (probe_pos - emit_pos).Length();
            if (test_length < 0.01f)
            {
                bad_light = true;
            }
            tmp_emit_pos = emit_pos;
            tmp_probe_pos = probe_pos;
        }
        public override void Added(Scene scene)
        {
            //调用base.Added(scene)来将该实体分别添加到原版和自定义实体管理器中
            base.Added(scene);

            //确定相对于相机的相对坐标
            Vector2 emit_position = Position - (Scene as Level).Camera.Position;
            //归一化
            emit_position = new Vector2(emit_position.X / 320.0f, emit_position.Y / 180.0f);
            //这里用的是1920*1080的纹理绘制到320*160的缓冲上，因此需要再次缩放
            tmp_emit_pos = emit_position / 6.0f;
            tmp_probe_pos = (emit_position + probe_pos - emit_pos) / 6.0f;

            //初始化tmp_emit_parallaxProportion和tmp_probe_parallaxProportion，这两个值可能会发生变化
            tmp_emit_parallaxProportion = Emit_ParallaxProportion;
            tmp_probe_parallaxProportion = Probe_ParallaxProportion;
            //初始化tmp_base_strength，这个是实际送入着色器的基础强度参数
            tmp_base_strength = base_strength;
            //指示是否正在过渡出场景
            is_out = false;

            TransitionListener handle_attribute = new TransitionListener();
            //在场景过渡进入时，将一些值进行渐进处理
            handle_attribute.OnIn = delegate (float f)
            {
                float time = (float)DBBMath.MotionMapping(f, "easeInOutSin");
                tmp_emit_parallaxProportion = DBBMath.Linear_Lerp(time, Vector2.Zero, Emit_ParallaxProportion);
                tmp_probe_parallaxProportion = DBBMath.Linear_Lerp(time, Vector2.Zero, Probe_ParallaxProportion);
                tmp_base_strength = (float)DBBMath.Linear_Lerp(time, 0.0f, base_strength);
                color.W = time * ref_color.W;
            };
            //在开始过渡进入时修正tmp_emit_parallaxProportion和tmp_probe_parallaxProportion和tmp_base_strength
            handle_attribute.OnInBegin = delegate ()
            {
                tmp_emit_parallaxProportion = Vector2.Zero;
                tmp_probe_parallaxProportion = Vector2.Zero;
                tmp_base_strength = 0.0f;
                color.W = 0.0f;
            };
            //在过渡出场景时，离开场景的实体的参考相机位置应当不再更新，同时需要标记为is_out表示正在过渡出场景
            //感谢扩展镜头大爹给我这两行代码干没用了
            //特保留以作为纪念
            handle_attribute.OnOutBegin = delegate ()
            {
                is_out = true;
                ref_cameraPos_when_out = (Scene as Level).Camera.Position;
            };
            //在过渡出场景时，将一些值进行渐退处理
            handle_attribute.OnOut = delegate (float f)
            {
                //扩展镜头：你没对我说谢谢，所以我不能让你的效果正确

                float time = (float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(f, "easeInOutSin"), 1.0f, 0.0f);
                tmp_emit_parallaxProportion = DBBMath.Linear_Lerp(time, Vector2.Zero, Emit_ParallaxProportion);
                tmp_probe_parallaxProportion = DBBMath.Linear_Lerp(time, Vector2.Zero, Probe_ParallaxProportion);
                tmp_base_strength = (float)DBBMath.Linear_Lerp(time, 0.0f, base_strength);
                color.W = time * ref_color.W;
            };
            Add(handle_attribute);
        }
        public override void Removed(Scene scene)
        {
            //调用base.Removed(scene)来将该实体分别从原版和自定义实体管理器中移除
            base.Removed(scene);
        }
        //实体的更新由原版管理器控制
        public override void Update()
        {
            base.Update();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Color debug_color = new Color(ref_color);
            //如果当前场景是过渡出场景，则使用过渡时的相机位置(感谢扩展镜头大爹给我这行代码干没用了，特保留以作为纪念)
            //Vector2 ref_cameraPos = is_out ? ref_cameraPos_when_out : (Scene as Level).Camera.Position;
            Vector2 ref_cameraPos = (Scene as Level).Camera.Position;
            //回推光源发射位置和探照位置
            Vector2 camera_viewport_WH = new Vector2((Scene as Level).Camera.Viewport.Width, (Scene as Level).Camera.Viewport.Height);
            Vector2 ref_vec = camera_viewport_WH;
            Vector2 real_relative_emit_pos = emit_pos * ref_vec;
            Vector2 real_relative_probe_pos = probe_pos * ref_vec;
            ref_vec *= 0.5f;
            Vector2 emit_offest = (Position - ref_cameraPos - ref_vec) * tmp_emit_parallaxProportion;
            Vector2 probe_offest = (probe_Position - ref_cameraPos - ref_vec) * tmp_probe_parallaxProportion;
            Vector2 real_emit_pos = Position + emit_offest;
            Vector2 real_probe_pos = Position + real_relative_probe_pos - real_relative_emit_pos + probe_offest;
            //绘制光源发射位置和探照位置
            Draw.Circle(real_emit_pos, 2.0f, debug_color, 8);
            Draw.Circle(real_probe_pos, 2.0f, debug_color, 8);
            Draw.Line(real_emit_pos, real_probe_pos, debug_color);
        }
        private void UpdateLightPos()
        {
            //扩展镜头改掉了摄像机的视口宽高
            Vector2 camera_viewport_WH = new Vector2((Scene as Level).Camera.Viewport.Width, (Scene as Level).Camera.Viewport.Height);
            Vector2 ref_center = camera_viewport_WH * 0.5f;
            //如果当前场景是过渡出场景，则使用过渡时的相机位置(感谢扩展镜头大爹给我这行代码干没用了，特保留以作为纪念)
            //Vector2 ref_cameraPos = is_out ? ref_cameraPos_when_out : (Scene as Level).Camera.Position;
            Vector2 ref_cameraPos = (Scene as Level).Camera.Position;
            //确定相对于相机的相对坐标
            Vector2 emit_offest = (Position - ref_cameraPos - ref_center) * tmp_emit_parallaxProportion;
            Vector2 probe_offest = (probe_Position - ref_cameraPos - ref_center) * tmp_probe_parallaxProportion;
            Vector2 emit_position = Position - ref_cameraPos;
            //归一化
            emit_position = new Vector2(emit_position.X / camera_viewport_WH.X, emit_position.Y / camera_viewport_WH.Y);
            emit_offest = new Vector2(emit_offest.X / camera_viewport_WH.X, emit_offest.Y / camera_viewport_WH.Y);
            probe_offest = new Vector2(probe_offest.X / camera_viewport_WH.X, probe_offest.Y / camera_viewport_WH.Y);

            float test_length = (probe_pos - emit_pos + probe_offest - emit_offest).Length();
            //如果是光照探针和光源距离太近，则视为坏光源
            bad_light = test_length < 0.01f ? true : false;
            //这里用的是DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"]绘制到GeneralLight的缓冲上，因此需要再次缩放
            float proportion = DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"].Width / camera_viewport_WH.X;
            tmp_emit_pos = (emit_position + emit_offest) / proportion;
            tmp_probe_pos = (emit_position + probe_pos - emit_pos + probe_offest) / proportion;

            time += velocity * Engine.DeltaTime;
        }
        private void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["time"].SetValue(time);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["scale"].SetValue(scale);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["base_strength"].SetValue(tmp_base_strength);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["dynamic_strength"].SetValue(dynamic_strength);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["emit_pos"].SetValue(tmp_emit_pos);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["probe_pos"].SetValue(tmp_probe_pos);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["light_radius"].SetValue(light_radius);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["iter"].SetValue(iter);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["concentration_factor"].SetValue(concentration_factor);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["extingction_factor"].SetValue(extingction_factor);
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["color"].SetValue(color);
        }

        //将光源渲染到原版光照缓冲
        public override void Render_On_Original_Light()
        {
            //如果光源不可见，则不渲染
            if (Visible == false)
            {
                return;
            }
            UpdateLightPos();
            //如果当前光源是坏光源，则不渲染
            if (bad_light == true)
            {
                return;
            }
            SetAllParameter();
            //Alpha通道增强仅仅对原版光缓冲生效
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["brightness_amplify"].SetValue(brightness_amplify);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["GodLight2D"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
        //将光源渲染到自定义光照缓冲
        public override void Render_On_Custom_Light()
        {
            //如果光源不可见，则不渲染
            if (Visible == false)
            {
                return;
            }
            //如果当前光源是坏光源，则不渲染
            if (bad_light == true)
            {
                return;
            }
            //如果只在原光照贴图上绘制，则不渲染
            if (DisableGodLight == true)
            {
                return;
            }
            SetAllParameter();
            DBBEffectSourceManager.DBBEffect["GodLight2D"].Parameters["brightness_amplify"].SetValue(1.0f);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["GodLight2D"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

    }
}
