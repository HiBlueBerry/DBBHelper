using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/HDRAndContrast")]
    [DBBCustomEntity(DBBCustomEntityIndexTable.HDRAndContrast,true)]
    public class HDRAndContrast : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式

        //------------------可以更改的特效性质------------------
        private float exposure_start = 1.0f;//曝光度
        private float exposure_end = 1.0f;//曝光度
        private string exposure_control_mode = "Linear";//曝光度

        private float gamma_start = 1.0f;//Gamma值
        private float gamma_end = 1.0f;//Gamma值
        private string gamma_control_mode = "Linear";//Gamma值

        private float contrast_start = 1.0f;//对比度
        private float contrast_end = 1.0f;//对比度
        private string contrast_control_mode = "Linear";//对比度

        //------------------临时值------------------
        private float exposure_tmp = 1.0f;
        private float gamma_tmp = 1.0f;
        private float contrast_tmp = 1.0f;

        public HDRAndContrast(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");//区域控制方式

            exposure_start = data.Float("ExposureStart");//曝光度
            exposure_end = data.Float("ExposureEnd");//曝光度
            exposure_control_mode = data.Attr("ExposureControlMode");//曝光度

            gamma_start = data.Float("GammaStart");//Gamma值
            gamma_end = data.Float("GammaEnd");//Gamma值
            gamma_control_mode = data.Attr("GammaControlMode");//Gamma值

            contrast_start = data.Float("ContrastStart");//对比度
            contrast_end = data.Float("ContrastEnd");//对比度
            contrast_control_mode = data.Attr("ContrastControlMode");//对比度

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TransitionListener handle_value = new TransitionListener();
            handle_value.OnIn = delegate (float f)
            {
                UpdateParameter();
            };
            Add(handle_value);
        }
        //尝试更新参数
        private void UpdateParameter()
        {
            if (Scene == null)
            {
                return;
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return;
            }
            Vector2 offset = player.Position - Position;
            //获取X比例和Y比例，首先只有人物中心在区域之内才能有效果，其次要根据控制方式来确定是X生效还是Y生效
            float x_proportion = offset.X / area.Width;
            float y_proportion = offset.Y / area.Height;
            if (x_proportion >= 0.0f && x_proportion <= 1.0f && y_proportion >= 0.0f && y_proportion <= 1.0f)
            {
                //从左到右模式
                if (area_control_mode == "Left_to_Right")
                {
                    float time1 = DBBMath.MotionMapping(x_proportion, exposure_control_mode);
                    float time2 = DBBMath.MotionMapping(x_proportion, gamma_control_mode);
                    float time3 = DBBMath.MotionMapping(x_proportion, contrast_control_mode);
                    exposure_tmp = (float)DBBMath.Linear_Lerp(time1, exposure_start, exposure_end);
                    gamma_tmp = (float)DBBMath.Linear_Lerp(time2, gamma_start, gamma_end);
                    contrast_tmp = (float)DBBMath.Linear_Lerp(time3, contrast_start, contrast_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    float time1 = DBBMath.MotionMapping(y_proportion, exposure_control_mode);
                    float time2 = DBBMath.MotionMapping(y_proportion, gamma_control_mode);
                    float time3 = DBBMath.MotionMapping(y_proportion, contrast_control_mode);
                    exposure_tmp = (float)DBBMath.Linear_Lerp(time1, exposure_start, exposure_end);
                    gamma_tmp = (float)DBBMath.Linear_Lerp(time2, gamma_start, gamma_end);
                    contrast_tmp = (float)DBBMath.Linear_Lerp(time3, contrast_start, contrast_end);

                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    exposure_tmp = exposure_end;
                    gamma_tmp = gamma_end;
                    contrast_tmp = contrast_end;
                }
                //告诉全局设置关卡内控制器已接管参数并设置参数值
                DBBSettings.ColorCorrectionMenu.HDR_InLevelControled = true;
                DBBGlobalSettingManager.Light_Exposure = exposure_tmp;
                DBBGlobalSettingManager.Light_Gamma = gamma_tmp;
                DBBGlobalSettingManager.Light_Contrast = contrast_tmp;

            }
        }
        public override void Update()
        {
            base.Update();
            UpdateParameter();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Position, area.Width, area.Height, Color.YellowGreen);
        }
        public static void Load()
        {
            DBBEffectSourceManager.Redraw_Something_On_Gameplay += Draw_GamePlayTempA_On_GamePlay;
        }
        public static void UnLoad()
        {
            DBBEffectSourceManager.Redraw_Something_On_Gameplay -= Draw_GamePlayTempA_On_GamePlay;
        }
        private static void Draw_GamePlayTempA_On_GamePlay()
        {
            //如果该项功能被禁用了，则停止一切绘制
            if (DBBGlobalSettingManager.ColorCorrectionSwitch == false)
            {
                return;
            }
            //将GameplayTempA上的东西画到Gameplay上
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            if (DBBSettings.ColorCorrectionMenu.HDR_InLevelControled == false)
            {
                DBBGlobalSettingManager.Light_Exposure = DBBSettings.ColorCorrectionMenu.ColorCorrection_Exposure;
                DBBGlobalSettingManager.Light_Gamma = DBBSettings.ColorCorrectionMenu.ColorCorrection_Gamma;
                DBBGlobalSettingManager.Light_Contrast = DBBSettings.ColorCorrectionMenu.ColorCorrection_Contrast;
            }
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"].Parameters["exposure"].SetValue(DBBGlobalSettingManager.Light_Exposure);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"].Parameters["gamma"].SetValue(DBBGlobalSettingManager.Light_Gamma);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"].Parameters["contrast"].SetValue(DBBGlobalSettingManager.Light_Contrast);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["GameplayTempA"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}