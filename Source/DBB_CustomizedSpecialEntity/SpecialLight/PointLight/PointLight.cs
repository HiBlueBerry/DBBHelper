using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/PointLight")]
    public class PointLight : DBBGeneralLight
    {
        //以下为参数项
        public Vector2 light_center = Vector2.Zero;//光源位置，应为归一化的数值
        public Vector4 color = new Vector4(0.2f, 0.8f, 1.0f, 1.0f);//颜色
        public float extinction = 10.0f;//衰减速度
        public float sphere_radius = 0.1f;//点光源半径，范围为0.0到0.5
        public float edge_width = 5.0f;//菲涅尔项的边缘厚度
        public float F0 = 1.0f;//菲涅尔基础反照率，如果不需要菲涅尔效果就将其置为1.0，范围应为0.0到1.0
        private float brightness_amplify = 1.0f;//原版亮度增幅，用于增强或者削弱原版光照
        private float aspect_ratio = 1.78f;//屏幕宽高比
        public float aspect_ratio_proportion = 1.0f;//屏幕宽高比的调节比例，此处用于用户自定义调节
        public float camera_z = 0.5f;//虚拟摄像机的Z轴位置

        //以下为一些临时记录内容
        public Vector4 ref_color;

        public PointLight(EntityData data, Vector2 offset)
        {
            //场景切入或切出时的光颜色变化方式
            LevelIn_Style = data.Attr("LevelInStyle", "easeInOutSin");
            LevelOut_Style = data.Attr("LevelOutStyle", "easeInOutSin");
            Position = data.Position + offset;
            //参数项的赋值
            color = DBBMath.ConvertColor(data.Attr("Color"));
            color.W = data.Float("Alpha");
            ref_color = color;
            brightness_amplify = data.Float("BrightnessAmplify", 1.0f);
            DisableEntityLight = data.Bool("OnlyEnableOriginalLight", false);//仅启用原版光照
            extinction = data.Float("Extinction", 10.0f);
            sphere_radius = data.Float("SphereRadius", 0.1f);
            edge_width = data.Float("EdgeWidth", 5.0f);
            F0 = data.Float("FresnelCoefficient", 1.0f);//菲涅尔系数
            aspect_ratio_proportion = data.Float("AspectRatioProportion", 1.0f);
            camera_z = data.Float("CameraZ", 0.5f);
            label = data.Attr("Label", "Default");//该实体的标签
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            //确定相对于相机的相对坐标
            light_center = Position - (Scene as Level).Camera.Position;
            //归一化
            light_center = new Vector2(light_center.X / 320.0F, light_center.Y / 180.0f);

            TransitionListener handle_attribute = new TransitionListener();
            //在场景过渡进入时，将一些值进行渐进处理
            handle_attribute.OnIn = delegate (float f)
            {
                //只有在非Instant模式下才进行渐变效果
                if (LevelIn_Style == "Instant")
                {
                    return;
                }
                float time = (float)DBBMath.MotionMapping(f, LevelIn_Style);
                color.W = time * ref_color.W;
            };
            handle_attribute.OnInBegin = delegate ()
            {
                if (LevelIn_Style == "Instant")
                {
                    color.W = ref_color.W;
                }
                else
                {
                    color.W = 0.0f;
                }
                
            };
            handle_attribute.OnOutBegin = delegate ()
            {
                if (LevelOut_Style == "Instant")
                {
                    color.W = 0.0f;
                }
            };
            //在过渡出场景时，将一些值进行渐退处理
            handle_attribute.OnOut = delegate (float f)
            {
                //只有在非Instant模式下才进行渐变效果
                if (LevelOut_Style == "Instant")
                {
                    return;
                }
                float time = (float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(f, LevelOut_Style), 1.0f, 0.0f);
                color.W = time * ref_color.W;
            };
            Add(handle_attribute);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void Update()
        {
            base.Update();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Color debug_color = new Color(ref_color);
            debug_color.A = 255;
            Draw.Circle(Position, 4.0f, debug_color, 8);
        }
        private void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["center"].SetValue(light_center);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["extinction"].SetValue(extinction);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["sphere_radius"].SetValue(sphere_radius);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["edge_width"].SetValue(edge_width);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["F0"].SetValue(F0);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["aspect_ratio"].SetValue(aspect_ratio * aspect_ratio_proportion);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["camera_z"].SetValue(camera_z);
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["color"].SetValue(color);
        }
        private void UpdateLightCenter()
        {
            light_center = Position - (Scene as Level).Camera.Position;
            //归一化
            light_center = new Vector2(light_center.X / GeneralLight_WH.X, light_center.Y / GeneralLight_WH.Y);
            //我也不知道GeneralLight_WH.Y为0是什么时候，反正就不管了
            if (GeneralLight_WH.Y == 0.0f)
            {
                aspect_ratio = 1.78f;
                return;
            }
            aspect_ratio = GeneralLight_WH.X / GeneralLight_WH.Y;
        }
        public override void Render_On_Original_Light()
        {
            //如果光源不可见，则不渲染
            if (Visible == false)
            {
                return;
            }
            UpdateLightCenter();
            SetAllParameter();
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["brightness_amplify"].SetValue(brightness_amplify);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["PointLight"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture320x180"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();

        }
        public override void Render_On_Custom_Light()
        {
            //如果光源不可见，则不渲染
            if (Visible == false)
            {
                return;
            }
            //如果只在原光照贴图上绘制，则不渲染
            if (DisableEntityLight == true)
            {
                return;
            }
            SetAllParameter();
            DBBEffectSourceManager.DBBEffect["PointLight"].Parameters["brightness_amplify"].SetValue(1.0f);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["PointLight"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture320x180"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}