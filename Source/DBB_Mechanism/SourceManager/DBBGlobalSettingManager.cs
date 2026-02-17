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

        //用于控制颜色矫正的开关
        public static bool ColorCorrectionSwitch { get; set; } = true;
        public static bool Last_ColorCorrectionSwitch { get; set; } = true;
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
        /// 当结束关卡时调整一些全局配置的值
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
        /// 用于Adjust_EffectSettings_WhenBegin收集一些设置的启用和禁用的信息，改变一些实体的钩子读取状态
        /// </summary>
        private static void Collect_OnOff_Information(bool attr_switch, bool last_attr_switch, int entity_index, string tag, string str_when_on, string str_when_off)
        {
            if (last_attr_switch == false && attr_switch == true)
            {
                Need_To_Adjust = true;
                Ignore.Remove(entity_index);
                Logger.Log(LogLevel.Warn, tag, str_when_on);
            }
            else if (last_attr_switch == true && attr_switch == false)
            {
                Need_To_Adjust = true;
                Ignore.Add(entity_index);
                Logger.Log(LogLevel.Warn, tag, str_when_off);
            }
        }
        /// <summary>
        /// 当开始关卡时调整一些全局配置的值，并根据此进行特效和光效的加载，该函数仅用于只能在山体界面设置的选项
        /// </summary>
        private static void Adjust_EffectSettings_WhenBegin(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (isFromLoader == true)
            {
                Need_To_Adjust = false;
                //以下为汇集一些设置的启用和禁用的信息
                //关于特殊光效
                Collect_OnOff_Information(SpecialLightSwitch, Last_SpecialLightSwitch, DBBCustomEntityIndexTable.SpecialLight, "DBBHelper/DBBGlobalSettingManager", "SpecialLight has been Enabled.", "SpecialLight has been Disabled.");
                //关于后处理特效
                Collect_OnOff_Information(HDPostProcessingSwitch, Last_HDPostProcessingSwitch, DBBCustomEntityIndexTable.HDPostProcessing, "DBBHelper/DBBGlobalSettingManager", "HDPostProcessing has been Enabled.", "HDPostProcessing has been Disabled.");
                //关于雾效
                Collect_OnOff_Information(FogEffectSwitch, Last_FogEffectSwitch, DBBCustomEntityIndexTable.FogEffect, "DBBHelper/DBBGlobalSettingManager", "FogEffect has been Enabled.", "FogEffect has been Disabled.");
                
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