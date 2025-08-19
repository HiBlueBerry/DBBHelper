using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.DBBHelper.Mechanism
{
    /// <summary>
    /// 存储那些可以被关卡内覆盖的全局配置，用于在离开关卡时更改游戏内覆盖的配置为默认配置
    /// </summary> 
    public class DBBGlobalSettingManager
    {
        //调整特效相关
        private static bool Need_To_Adjust = false;//指示是否需要调整特效的开关
        private static List<int> Ignore = new List<int>();//指示加载特效实体的钩子时需要忽略哪些特效实体

        //颜色矫正
        public static Vector4 Tint_Color = new Vector4(1, 1, 1, 1);//色调
        public static float Tint_Strength = 1.0f;//色调强度
        public static float Saturation = 1.0f;//饱和度
        public static float Light_Exposure = 1.0f;//曝光度
        public static float Light_Gamma = 1.0f;//Gamma矫正
        public static float Light_Contrast = 1.0f;//对比度

        //用于控制后处理特效的开关
        public static bool HDPostProcessingSwitch { get; set; } = true;
        public static bool Last_HDPostProcessingSwitch { get; set; } = true;

        //用于控制雾效的开关
        public static bool FogEffectSwitch { get; set; } = true;
        public static bool Last_FogEffectSwitch { get; set; } = true;

        //用于控制特殊光效的开关
        public static bool SpecialLightSwitch { get; set; } = true;
        public static bool Last_SpecialLightSwitch { get; set; } = true;

        /// <summary>
        /// 当结束关卡时调整色彩校正全局配置的值
        /// </summary>
        private static void Adjust_ColorCorrection_GlobalSettings_WhenEnd(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);
            //调整色调与饱和度设置
            Tint_Color = DBBSettings.ColorCorrectionMenu.ColorCorrection_TintColor;
            Tint_Strength = DBBSettings.ColorCorrectionMenu.ColorCorrection_TintStrength;
            Saturation = DBBSettings.ColorCorrectionMenu.ColorCorrection_Saturation;
            //调整HDR与对比度设置
            Light_Exposure = DBBSettings.ColorCorrectionMenu.ColorCorrection_Exposure;
            Light_Gamma = DBBSettings.ColorCorrectionMenu.ColorCorrection_Gamma;
            Light_Contrast = DBBSettings.ColorCorrectionMenu.ColorCorrection_Contrast;
            //告知菜单这些设置不再受关卡内参数控制
            DBBSettings.ColorCorrectionMenu.HDR_InLevelControled = false;
            DBBSettings.ColorCorrectionMenu.Tint_Saturation_InLevelControled = false;
        }
        /// <summary>
        /// 当开始关卡时调整HDPostProcessing_Switch全局配置的值，并根据此进行后处理特效的加载
        /// </summary>
        private static void Adjust_EffectSettings_WhenBegin(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (isFromLoader == true)
            {
                Need_To_Adjust = false;
                //以下为汇集一些设置的启用和禁用的信息

                //关于特殊光效
                if (Last_SpecialLightSwitch == false && SpecialLightSwitch == true)
                {
                    Need_To_Adjust = true;
                    Ignore.Remove(0);
                    Logger.Log(LogLevel.Warn, "DBBHelper/DBBGlobalSettingManager", "SpecialLight has been Enabled.");
                }
                else if (Last_SpecialLightSwitch == true && SpecialLightSwitch == false)
                {
                    Need_To_Adjust = true;
                    Ignore.Add(0);
                    Logger.Log(LogLevel.Warn, "DBBHelper/DBBGlobalSettingManager", "SpecialLight has been Disabled.");
                }

                //关于后处理特效
                if (Last_HDPostProcessingSwitch == false && HDPostProcessingSwitch == true)
                {
                    Need_To_Adjust = true;
                    Ignore.Remove(3);
                    Logger.Log(LogLevel.Warn, "DBBHelper/DBBGlobalSettingManager", "HDPostProcessing has been Enabled.");
                }
                else if (Last_HDPostProcessingSwitch == true && HDPostProcessingSwitch == false)
                {
                    Need_To_Adjust = true;
                    Ignore.Add(3);
                    Logger.Log(LogLevel.Warn, "DBBHelper/DBBGlobalSettingManager", "HDPostProcessing has been Disabled.");
                }

                //关于雾效
                if (Last_FogEffectSwitch == false && FogEffectSwitch == true)
                {
                    Need_To_Adjust = true;
                    Ignore.Remove(4);
                    Logger.Log(LogLevel.Warn, "DBBHelper/DBBGlobalSettingManager", "FogEffect has been Enabled.");
                }
                else if (Last_FogEffectSwitch == true && FogEffectSwitch == false)
                {
                    Need_To_Adjust = true;
                    Ignore.Add(4);
                    Logger.Log(LogLevel.Warn, "DBBHelper/DBBGlobalSettingManager", "FogEffect has been Disabled.");
                }


                //按需重新加载所有特效实体的钩子
                if (Need_To_Adjust == true)
                {
                    DBBCustomEntityManager.UnLoadInLevel();
                    DBBCustomEntityManager.LoadInLevel(Ignore);
                }

                //更新状态值
                Last_SpecialLightSwitch = SpecialLightSwitch;
                Last_HDPostProcessingSwitch = HDPostProcessingSwitch;
                Last_FogEffectSwitch = FogEffectSwitch;
            }
            orig(self, playerIntro, isFromLoader);
        }
        /// <summary>
        /// 加载钩子，用于根据设置动态修改要加载的特效
        /// </summary>
        public static void Load()
        {
            On.Celeste.Level.LoadLevel += new On.Celeste.Level.hook_LoadLevel(Adjust_EffectSettings_WhenBegin);
            On.Celeste.Level.End += new On.Celeste.Level.hook_End(Adjust_ColorCorrection_GlobalSettings_WhenEnd);
        }
        /// <summary>
        /// 卸载钩子
        /// </summary> 
        public static void UnLoad()
        {
            On.Celeste.Level.End -= new On.Celeste.Level.hook_End(Adjust_ColorCorrection_GlobalSettings_WhenEnd);
            On.Celeste.Level.LoadLevel -= new On.Celeste.Level.hook_LoadLevel(Adjust_EffectSettings_WhenBegin);
        }
    }
}