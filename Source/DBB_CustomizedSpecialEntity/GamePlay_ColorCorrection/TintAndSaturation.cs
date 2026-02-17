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
    [CustomEntity("DBBHelper/TintAndSaturation")]
    [DBBCustomEntity(DBBCustomEntityIndexTable.TintAndSaturation,true)]
    public class TintAndSaturation : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式

        //------------------可以更改的特效性质------------------
        private Vector4 tintColor_start = new Vector4(1,1,1,1);//色调
        private Vector4 tintColor_end = new Vector4(1,1,1,1);//色调
        private string tintColor_control_mode = "Linear";//色调

        private float tintStrength_start = 0.0f;//色调强度
        private float tintStrength_end = 1.0f;//色调强度
        private string tintStrength_control_mode = "Linear";//色调强度

        private float saturation_start = 1.0f;//饱和度
        private float saturation_end = 1.0f;//饱和度
        private string saturation_control_mode = "Linear";//饱和度
        //------------------临时值------------------
        private Vector4 tintColor_tmp = new Vector4(1,1,1,1);
        private float tintStrength_tmp = 1.0f;
        private float saturation_tmp = 1.0f;

        public TintAndSaturation(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");//区域控制方式

            tintColor_start = DBBMath.ConvertColor(data.Attr("TintColorStart"));//色调
            tintColor_end = DBBMath.ConvertColor(data.Attr("TintColorEnd"));//色调
            tintColor_control_mode = data.Attr("TintColorControlMode");//色调

            tintStrength_start = data.Float("TintStrengthStart");//色调强度
            tintStrength_end = data.Float("TintStrengthEnd");//色调强度
            tintStrength_control_mode = data.Attr("TintStrengthControlMode");//色调强度

            saturation_start = data.Float("SaturationStart");//饱和度
            saturation_end = data.Float("SaturationEnd");//饱和度
            saturation_control_mode = data.Attr("SaturationControlMode");//饱和度

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
                    float time1 = DBBMath.MotionMapping(x_proportion, tintColor_control_mode);
                    float time2 = DBBMath.MotionMapping(x_proportion, tintStrength_control_mode);
                    float time3 = DBBMath.MotionMapping(x_proportion, saturation_control_mode);

                    tintColor_tmp = DBBMath.Linear_Lerp(time1,tintColor_start, tintColor_end);
                    tintStrength_tmp = (float)DBBMath.Linear_Lerp(time2, tintStrength_start, tintStrength_end);
                    saturation_tmp = (float)DBBMath.Linear_Lerp(time3, saturation_start, saturation_end);
                    
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    float time1 = DBBMath.MotionMapping(y_proportion, tintColor_control_mode);
                    float time2 = DBBMath.MotionMapping(y_proportion, tintStrength_control_mode);
                    float time3 = DBBMath.MotionMapping(y_proportion, saturation_control_mode);

                    tintColor_tmp = DBBMath.Linear_Lerp(time1,tintColor_start, tintColor_end);
                    tintStrength_tmp = (float)DBBMath.Linear_Lerp(time2, tintStrength_start, tintStrength_end);
                    saturation_tmp = (float)DBBMath.Linear_Lerp(time3, saturation_start, saturation_end);
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    tintColor_tmp = tintColor_end;
                    tintStrength_tmp = tintStrength_end;
                    saturation_tmp = saturation_end;
                }
                //告诉全局设置关卡内控制器已接管参数并设置参数值
                DBBSettings.ColorCorrectionMenu.Tint_Saturation_InLevelControled = true;
                DBBGlobalSettingManager.Tint_Color = tintColor_tmp;
                DBBGlobalSettingManager.Tint_Strength = tintStrength_tmp;
                DBBGlobalSettingManager.Saturation = saturation_tmp;

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
            Draw.HollowRect(Position, area.Width, area.Height, Color.LawnGreen);
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
            //按照色调、饱和度、HDR和对比度的顺序进行绘制
            //任何绘制工作交给HDRAndContrast
            if (DBBSettings.ColorCorrectionMenu.Tint_Saturation_InLevelControled == false)
            {
                DBBGlobalSettingManager.Tint_Color = DBBSettings.ColorCorrectionMenu.ColorCorrection_TintColor;
                DBBGlobalSettingManager.Tint_Strength = DBBSettings.ColorCorrectionMenu.ColorCorrection_TintStrength;
                DBBGlobalSettingManager.Saturation = DBBSettings.ColorCorrectionMenu.ColorCorrection_Saturation;
            }
            Vector3 tint_color = new Vector3(DBBGlobalSettingManager.Tint_Color.X, DBBGlobalSettingManager.Tint_Color.Y, DBBGlobalSettingManager.Tint_Color.Z);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"].Parameters["tint_color"].SetValue(tint_color);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"].Parameters["tint_strength"].SetValue(DBBGlobalSettingManager.Tint_Strength);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorCorrection"].Parameters["saturation"].SetValue(DBBGlobalSettingManager.Saturation);
            //任何绘制工作交给HDRAndContrast
        }
    }
}