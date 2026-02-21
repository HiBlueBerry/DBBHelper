using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using On.Celeste;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Text.RegularExpressions;
using Celeste.Mod.DBBHelper.Mechanism;
using Celeste.Mod.DBBHelper.UI.UIComponent;
using System;

namespace Celeste.Mod.DBBHelper {
    [SettingName("DBBHelper_Settings")]
    public class DBBSettings : EverestModuleSettings
    {

        //以下为素材贴图

        //以下为各种属性表
        public static Dictionary<int, string> ColorCorrectionTable = Enumerable.Range(0, 71).ToDictionary
        (
            key_item => key_item,
            key_item => (-3.5f + key_item * 0.1f).ToString("0.0")
        );//取值范围表，范围为-3.5到3.5，每次步进0.1
        public static Dictionary<int,string> SpecialLight_GeneralLightBlendState_Table=new Dictionary<int,string>()
        {
            {0,"AlphaKeep"},
            {1,"AlphaWeakened"}
        };//特殊光照层与gp层的颜色混合方式表
        public static Dictionary<int,string> SpecialLight_OnlyEnableOriginalLight_Table=new Dictionary<int,string>()
        {
            {0,"Special and original light render"},
            {1,"Using only original light render"}
        };//特殊光照层与gp层的颜色混合方式表

        public static List<string> HexTable = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };//十六进制对应表
        //以下为涉及一系列自定义的函数和类
        [SettingSubMenu]
        public class ColorCorrection
        {
            //颜色矫正开关，这里使用可以保存的设置，即重开游戏将读取上一次关游戏时的配置
            [SettingName("ModOptions_DBBHelper_ColorCorrection_Switch")]
            [SettingSubText("ModOptions_DBBHelper_ColorCorrection_Switch_Tip")]
            public bool ColorCorrectionSwitch
            {
                get { return DBBGlobalSettingManager.ColorCorrectionSwitch; }
                set
                {
                    DBBGlobalSettingManager.Last_ColorCorrectionSwitch = DBBGlobalSettingManager.ColorCorrectionSwitch;
                    DBBGlobalSettingManager.ColorCorrectionSwitch = value;
                }
            }
            public Vector4 ColorCorrection_TintColor { get; set; } = new Vector4(1, 1, 1, 1);
            public float ColorCorrection_TintStrength { get; set; } = 0.0f;
            public float ColorCorrection_Saturation { get; set; } = 1.0f;

            public float ColorCorrection_Exposure { get; set; } = 1.0f;
            public float ColorCorrection_Gamma { get; set; } = 1.0f;
            public float ColorCorrection_Contrast { get; set; } = 1.0f;

            public bool HDR_InLevelControled = false;//这个参数将传给ExtendedSlider
            public bool Tint_Saturation_InLevelControled = false;//这个参数将传给ExtendedSlider


            //每次重新打开全局设置时都会重新构建一遍组件，为此需要记录上一次选择的索引
            private int previous_tint_strength_index = 35;
            private int previous_saturation_index = 45;
            private int previous_exposure_index = 45;
            private int previous_gamma_index = 45;
            private int previous_contrast_index = 45;
            public void CreateColorCorrection_TintColorEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ColorPicker(
                    Dialog.Clean("ModOptions_DBBHelper_ColorCorrection_TintColor"),
                    Color.YellowGreen,
                    Color.OrangeRed,
                    ColorCorrection_TintColor,
                    Vector4.One,
                    () => Tint_Saturation_InLevelControled,
                    "ModOptions_DBBHelper_ColorCorrection_Controlled_Tip",
                    () => !DBBGlobalSettingManager.ColorCorrectionSwitch
                );
                tmp.Change
                (
                    delegate (Vector4 current_color)
                    {
                        ColorCorrection_TintColor = current_color;
                    }
                );
                submenu.Add(tmp);
            }
            public void CreateColorCorrection_TintStrengthEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_ColorCorrection_TintStrength"),
                    index => ColorCorrectionTable[index],
                    35,
                    45,
                    Color.YellowGreen,
                    Color.OrangeRed,
                    35,
                    previous_tint_strength_index,
                    () => Tint_Saturation_InLevelControled,
                    "ModOptions_DBBHelper_ColorCorrection_Controlled_Tip",
                    () => !DBBGlobalSettingManager.ColorCorrectionSwitch
                );
                tmp.Change
                (
                    delegate (int index)
                    {
                        ColorCorrection_TintStrength = float.Parse(ColorCorrectionTable[index]);
                        previous_tint_strength_index = index;
                    }
                );
                submenu.Add(tmp);
            }
            public void CreateColorCorrection_SaturationEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_ColorCorrection_Saturation"),
                    index => ColorCorrectionTable[index],
                    35,
                    55,
                    Color.YellowGreen,
                    Color.OrangeRed,
                    45,
                    previous_saturation_index,
                    () => Tint_Saturation_InLevelControled,
                    "ModOptions_DBBHelper_ColorCorrection_Controlled_Tip",
                    () => !DBBGlobalSettingManager.ColorCorrectionSwitch
                );
                tmp.Change
                (
                    delegate (int index)
                    {
                        ColorCorrection_Saturation = float.Parse(ColorCorrectionTable[index]);
                        previous_saturation_index = index;
                    }
                );
                submenu.Add(tmp);
            }
            public void CreateColorCorrection_ExposureEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_ColorCorrection_Exposure"),
                    index => ColorCorrectionTable[index],
                    0,
                    70,
                    Color.DarkGoldenrod,
                    Color.OrangeRed,
                    45,
                    previous_exposure_index,
                    () => HDR_InLevelControled,
                    "ModOptions_DBBHelper_ColorCorrection_Controlled_Tip",
                    () => !DBBGlobalSettingManager.ColorCorrectionSwitch
                );
                tmp.Change
                (
                    delegate (int index)
                    {
                        ColorCorrection_Exposure = float.Parse(ColorCorrectionTable[index]);
                        previous_exposure_index = index;
                    }
                );
                submenu.Add(tmp);
            }
            public void CreateColorCorrection_GammaEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_ColorCorrection_Gamma"),
                    index => ColorCorrectionTable[index],
                    35,
                    70,
                    Color.DarkGoldenrod,
                    Color.OrangeRed,
                    45,
                    previous_gamma_index,
                    () => HDR_InLevelControled,
                    "ModOptions_DBBHelper_ColorCorrection_Controlled_Tip",
                    () => !DBBGlobalSettingManager.ColorCorrectionSwitch
                );
                tmp.Change
                (
                    delegate (int index)
                    {
                        ColorCorrection_Gamma = float.Parse(ColorCorrectionTable[index]);
                        previous_gamma_index = index;
                    }
                );
                submenu.Add(tmp);
            }
            public void CreateColorCorrection_ContrastEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {

                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_ColorCorrection_Contrast"),
                    index => ColorCorrectionTable[index],
                    0,
                    70,
                    Color.DarkGoldenrod,
                    Color.OrangeRed,
                    45,
                    previous_contrast_index,
                    () => HDR_InLevelControled,
                    "ModOptions_DBBHelper_ColorCorrection_Controlled_Tip",
                    () => !DBBGlobalSettingManager.ColorCorrectionSwitch
                    );
                tmp.Change
                (
                    delegate (int index)
                    {
                        ColorCorrection_Contrast = float.Parse(ColorCorrectionTable[index]);
                        previous_contrast_index = index;
                    }
                );
                submenu.Add(tmp);
                //补一个空白占位行
                var blank_item = new DBBCustomUIComponent.PlaceholderItem(1, 0.3f);
                submenu.Add(blank_item);
            }

        }

        [SettingSubMenu]
        public class SpeicalLight
        {
            //特殊光效开关，这里使用可以保存的设置，即重开游戏将读取上一次关游戏时的配置
            [SettingName("ModOptions_DBBHelper_SpecialLight_Switch")]
            [SettingSubText("ModOptions_DBBHelper_SpecialLight_Switch_Tip")]
            [SettingInGame(false)]
            public bool SpecialLightSwitch
            {
                get { return DBBGlobalSettingManager.SpecialLightSwitch; }
                set { DBBGlobalSettingManager.Last_SpecialLightSwitch = DBBGlobalSettingManager.SpecialLightSwitch; DBBGlobalSettingManager.SpecialLightSwitch = value; }
            }

            //[SettingSubText("ModOptions_DBBHelper_SpecialLight_GeneralLightBlendState_Tip")]
            public int SpecialLight_GeneralLightBlendState { get; set; } = 0;

            //[SettingSubText("ModOptions_DBBHelper_SpecialLight_OnlyEnableOriginalLight_Tip")]
            public int SpecialLight_OnlyEnableOriginalLight { get; set; } = 0;

            public bool GeneralLightBlendState_InLevelControled = false;//这个参数将传给ExtendedSlider
            public bool OnlyEnableOriginalLight_InLevelControled = false;//这个参数将传给ExtendedSlider

            //每次重新打开全局设置时都会重新构建一遍组件，为此需要记录上一次选择的索引
            private int previous_blendstate_index = 0;
            private int previous_only_enable_original_light_index = 0;

            public void CreateSpecialLight_GeneralLightBlendStateEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_SpecialLight_GeneralLightBlendState"),
                    index => SpecialLight_GeneralLightBlendState_Table[index],
                    0,
                    1,
                    Color.DeepSkyBlue,
                    Color.OrangeRed,
                    0,
                    previous_blendstate_index,
                    () => GeneralLightBlendState_InLevelControled,
                    "ModOptions_DBBHelper_SpecialLight_Controlled_Tip",
                    () => !DBBGlobalSettingManager.SpecialLightSwitch
                );
                tmp.Change
                (
                    delegate (int index)
                    {
                        SpecialLight_GeneralLightBlendState = index;
                        previous_blendstate_index = index;
                    }
                );
                //为滑块组件添加注释
                var description_item = new DBBCustomUIComponent.ExtendedDescription(tmp, "ModOptions_DBBHelper_SpecialLight_GeneralLightBlendState_Tip", false)
                {
                    TextColor = new Color(0.416f, 0.545f, 0.686f),
                    //这个是开头空出来的高度，空出来的高度用来放图片，0.0是没有空出来的情况
                    HeightExtra = 24.0f
                };
                submenu.Add(tmp);
                submenu.Add(description_item);
                
            }
            public void CreateSpecialLight_OnlyEnableOriginalLightEntry(TextMenuExt.SubMenu submenu, bool inGame)
            {
                var tmp = new DBBCustomUIComponent.ExtendedSlider(
                    Dialog.Clean("ModOptions_DBBHelper_SpecialLight_OnlyEnableOriginalLight"),
                    index => SpecialLight_OnlyEnableOriginalLight_Table[index],
                    0,
                    1,
                    Color.DeepSkyBlue,
                    Color.OrangeRed,
                    0,
                    previous_only_enable_original_light_index,
                    () => OnlyEnableOriginalLight_InLevelControled,
                    "ModOptions_DBBHelper_SpecialLight_Controlled_Tip",
                    () => !DBBGlobalSettingManager.SpecialLightSwitch
                );
                tmp.Change
                (
                    delegate (int index)
                    {
                        SpecialLight_OnlyEnableOriginalLight = index;
                        previous_only_enable_original_light_index = index;
                    }
                );
                //为滑块组件添加注释
                var description_item = new DBBCustomUIComponent.ExtendedDescription(tmp, "ModOptions_DBBHelper_SpecialLight_OnlyEnableOriginalLight_Tip", false)
                {
                    TextColor = new Color(0.416f, 0.545f, 0.686f),
                    //这个是开头空出来的高度，空出来的高度用来放图片，0.0是没有空出来的情况
                    HeightExtra = 24.0f
                };
                //补一个空白占位行
                var blank_item = new DBBCustomUIComponent.PlaceholderItem(1, 0.3f);
                submenu.Add(tmp);
                submenu.Add(description_item);
                submenu.Add(blank_item);
            }


        }

        //以下为显示的属性实例

        //颜色校正，包含色调、饱和度、HDR与对比度，这里不使用可保存的设置，即每次重开游戏都将恢复到默认配置
        [SettingName("ModOptions_DBBHelper_ColorCorrection")]
        [SettingSubText("ModOptions_DBBHelper_ColorCorrection_Tip")]
        public static ColorCorrection ColorCorrectionMenu { get; set; } = new ColorCorrection();

        [SettingName("ModOptions_DBBHelper_SpecialLight")]
        [SettingSubText("ModOptions_DBBHelper_SpecialLight_Tip")]
        public static SpeicalLight SpecialLightMenu { get; set; } = new SpeicalLight();
        //后处理特效开关，这里使用可以保存的设置，即重开游戏将读取上一次关游戏时的配置
        [SettingName("ModOptions_DBBHelper_HDPostProcessing")]
        [SettingSubText("ModOptions_DBBHelper_HDPostProcessing_Tip")]
        [SettingInGame(false)]
        public bool HDPostProcessingSwitch { get { return DBBGlobalSettingManager.HDPostProcessingSwitch; } set { DBBGlobalSettingManager.Last_HDPostProcessingSwitch = DBBGlobalSettingManager.HDPostProcessingSwitch; DBBGlobalSettingManager.HDPostProcessingSwitch = value; } }

        //高清雾效开关，这里使用可以保存的设置，即重开游戏将读取上一次关游戏时的配置
        [SettingName("ModOptions_DBBHelper_FogEffect")]
        [SettingSubText("ModOptions_DBBHelper_FogEffect_Tip")]
        [SettingInGame(false)]
        public bool FogEffectSwitch { get { return DBBGlobalSettingManager.FogEffectSwitch; } set { DBBGlobalSettingManager.Last_FogEffectSwitch = DBBGlobalSettingManager.FogEffectSwitch; DBBGlobalSettingManager.FogEffectSwitch = value; } }
    }
}
