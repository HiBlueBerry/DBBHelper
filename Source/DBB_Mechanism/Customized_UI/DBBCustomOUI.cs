using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Celeste.Mod.UI;
using Monocle;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.UI.OUI
{

    public class DBB_ColorPicker_OUI : Oui, OuiModOptions.ISubmenu
    {
        //ColorPicker分为三部分，右侧的十六进制颜色值，左下侧的拾色器和右下侧的RGB和HSV值显示，例如：
        //Label:         ff00000
        //拾色器区域  R:255 H:0
        //拾色器区域  G:0   S:100
        //拾色器区域  B:0   V:100

        //以下为整体相关属性
        private float MAX_CHAR_WIDTH = 0;//单个字符的最大宽度
        private Action Exit;//一个用于控制离开该OUI的事件
        private float timer;//一个通用的计时器，文字的抖动效果等都可以用这个计时器
        private float ease;//一个通用的用于反映界面渐入和渐出的过程值，以渐入为例子，当刚开始进入时ease为0，在完全进入后ease为1
        private MTexture General_square;//一个通用的400*400的纯白色图片
        public string HexColor_For_Show { get { return final_HexColor_for_show; } }//最后需要展示的颜色十六进制的值，用于向主菜单的UI组件传值
        private string final_HexColor_for_show = "FFFFFF";//最后需要展示的颜色十六进制的值，用于向主菜单的UI组件传值，这里控制它的属性访问为外界只读
        private int selected_component = 0;//指示当前选中的是哪个内部组件，0为十六进制颜色值，1为拾色区域，2为色相条，3为RGB和HSV值显示
        private int need_synchronize_value = -1;//指示是否需要同步各个组件的值，-1为不需要同步，0为HEX颜色值组件发起同步，1为拾色区域发起同步，2为色相条发起同步，3为RGBHSV发起同步

        //以下为HEX颜色值组件相关属性
        private string HexColor_label = "";//HEX颜色值组件在文本文件中对应的标签，文本文件中该标签所对应的内容将会被展示
        private bool HexColor_focus = false;//指示当前HEX颜色值组件是否被聚焦，被聚焦后将允许用户更改HEX颜色值的数字
        private int[] HexColor_digits = new int[6];//所有HEX颜色值的数字
        private int selected_HexColor_digit;//存储选中的HEX颜色值的数字
        private Color unselected_HexColor_color = Color.White;//HEX颜色值未选中时的数字颜色
        private Color[] selected_HexColor_color = new Color[2];//HEX颜色值选中时的数字颜色
        private Wiggler[] HexColor_wiggler_digits;//Hex颜色值数字抖动效果

        //以下为色相条组件相关属性
        private bool HueBar_focus = false;//色相条是否被聚焦
        private float HueBar_mouseSensitivity = 1.0f;//色相条的鼠标敏感性，此值用于拖拽滑块用
        private float HueBar_currentHue = 0.0f;//当前存储的色相值，同时是纵坐标值
        private Rectangle HueBar_barRectangle = new Rectangle(0, 0, 30, 200);//色相条的左上角位置和大小
        private Color HueBar_knobInnerColor = Color.White;//色相条滑块的内部颜色
        private Color HueBar_knobBorderColor = Color.White;//拾色区域滑块的边界颜色
        private Vector2 HueBar_knob_leftup_corner = new Vector2(-5.0f, -5.0f);//色相条滑块在最左侧时其左上角的位置
        private Rectangle HueBar_knobInnerRectangle = new Rectangle(-5, -5, 40, 10);//色相条滑块内侧的当前的左上角位置和滑块大小
        private Rectangle HueBar_knobOuterRectangle = new Rectangle(-10, -10, 50, 20);//色相条滑块外侧的当前的左上角位置和滑块大小
        private int HueBar_knobBorderThickness = 5;//色相条的滑块的边框大小
        private bool HueBar_isDragging = false;//用于指示现在色相条滑块是否处于滑动状态

        //以下为拾色区域组件的相关属性
        private bool ColorPickerArea_focus = false;//拾色区域是否被聚焦
        private float ColorPickerArea_mouseSensitivity = 1.0f;//拾色区域的鼠标敏感性，此值用于拖拽滑块用
        private Vector2 ColorPickerArea_currentValue = Vector2.One;//拾色区域当前坐标值
        private Color ColorPickerArea_currentColor = Color.White;//拾色区域当前颜色，同时是滑块的内部颜色
        private Color ColorPickerArea_knobBorderColor = Color.White;//拾色区域滑块的边界颜色
        private Vector2 ColorPickerArea_knob_leftup_corner = new Vector2(-10.0f, -10.0f);//拾色区域的滑块在最左侧时其左上角的位置
        private Rectangle ColorPickerArea_knobInnerRectangle = new Rectangle(-10, -10, 20, 20);//拾色区域滑块内侧的当前的左上角位置和滑块大小
        private Rectangle ColorPickerArea_knobOuterRectangle = new Rectangle(-15, -15, 30, 30);//拾色区域滑块外侧的当前的左上角位置和滑块大小
        private int ColorPickerArea_knobBorderThickness = 5;//拾色区域的滑块的边框大小
        private bool ColorPickerArea_isDragging = false;//用于指示拾色区域的滑块现在是否处于滑动状态
        private Rectangle ColorPickerArea_pickerRectangle = new Rectangle(0, 0, 200, 200);//拾色区域组件的左上角位置和大小

        //以下为RGB和HSV组件相关属性，HSV范围从0到100，意为从0到百分之100
        private bool RGB_and_HSV_focus = false;//指示当前RGB和HSV组件是否被聚焦，被聚焦后将允许用户更改RGB和HSV的值
        private int RGB_and_HSV_assignment = 0;//赋值情况，0为未进行赋值，-1为进行了赋值且赋值失败，1为进行了赋值且赋值成功
        private bool RGB_and_HSV_invalidInput_Draw = false;//是否进行赋值失败提示框的绘制
        private bool RGB_and_HSV_resetIndex = true;//在进入拾色器界面时是否重置RGBHSV数组索引
        private int[] RGB_HSV = new int[6];//依次存储RGB和HSV的值
        private string[] RGB_HSV_name = new string[6];//存储RGBHSV六个字母
        private Color[] selected_RGBHSV_color = new Color[2];//RGBHSV选中时的数字颜色
        private int selected_RGBHSV_row = 0;//当前选中的行
        private int selected_RGBHSV_column = 0;//当前选中的列
        private int selected_RGBHSV_index = 0;//当前选中的索引
        private Coroutine RGB_and_HSV_invalidInput_tipEvent = null;//记录RGBHSV输入提示事件，用于在离开界面时终止运行中的RGBHSV事件
        private string[] RGB_and_HSV_invalidInput_label = new string[2];//记录输入RGB和HSV时遇到非法值时的显示标签
        private float RGB_and_HSV_invalidInput_tipEase = 0.0f;//用于控制RGBHSV输入提示的渐入和渐出

        //以下为组件提示框属性
        private string[] Tip_labels = new string[8];

        //以下为鼠标相关属性
        private Vector2 Mouse_previous_position = Vector2.Zero;
        private Vector2 Mouse_current_position = Vector2.Zero;
        private bool Mouse_left_pressed = false;

        //以下为轮廓线属性
        private Color selected_OutLine_color = Color.White;//组件选中时轮廓线的颜色
        private Color focused_OutLine_color = Calc.HexToColor("FCFF59");//组件被聚焦时轮廓线的颜色


        public DBB_ColorPicker_OUI()
        {
            //初始化通用值
            //测量一下字符最大宽度
            string test_string = "0123456789ABCDEF";
            foreach (var test_char in test_string)
            {
                MAX_CHAR_WIDTH = Math.Max(MAX_CHAR_WIDTH, ActiveFont.Measure(test_char).X);
            }
            Exit = null;
            timer = 0.0f;
            ease = 0.0f;
            General_square = GFX.Game["objects/DBB_Items/UI/general"];
            final_HexColor_for_show = "FFFFFF";
            selected_component = 0;
            need_synchronize_value = -1;

            //初始化HEX颜色值组件
            HexColor_label = "";
            HexColor_focus = false;
            HexColor_digits = [1, 1, 1, 1, 1, 1];
            selected_HexColor_digit = 0;
            unselected_HexColor_color = Color.White;
            selected_HexColor_color = [Calc.HexToColor("84FF54"), Calc.HexToColor("FCFF59")];
            //为每个显示数字设置一个摆动器
            HexColor_wiggler_digits = new Wiggler[HexColor_digits.Length];
            for (int i = 0; i < HexColor_digits.Length; i++)
            {
                HexColor_wiggler_digits[i] = Wiggler.Create(0.25f, 4f);
            }

            //初始化拾色区域组件
            ColorPickerArea_focus = false;
            ColorPickerArea_mouseSensitivity = 1.0f;
            ColorPickerArea_currentValue = Vector2.Zero;
            ColorPickerArea_currentColor = Color.White;
            ColorPickerArea_knobBorderColor = Color.White;
            ColorPickerArea_knob_leftup_corner = new Vector2(-10.0f, -10.0f);
            ColorPickerArea_knobInnerRectangle = new Rectangle(-10, -10, 20, 20);
            ColorPickerArea_knobOuterRectangle = new Rectangle(-15, -15, 30, 30);
            ColorPickerArea_knobBorderThickness = 5;
            ColorPickerArea_isDragging = false;
            ColorPickerArea_pickerRectangle = new Rectangle(0, 0, 200, 200);

            //初始化色相条组件
            HueBar_focus = false;
            HueBar_mouseSensitivity = 1.0f;
            HueBar_currentHue = 0.0f;
            HueBar_barRectangle = new Rectangle(0, 0, 30, 200);
            HueBar_knobInnerColor = Color.White;
            HueBar_knobBorderColor = Color.White;
            HueBar_knob_leftup_corner = new Vector2(-5.0f, -5.0f);
            HueBar_knobInnerRectangle = new Rectangle(-5, -5, 40, 10);
            HueBar_knobOuterRectangle = new Rectangle(-10, -10, 50, 20);
            HueBar_knobBorderThickness = 5;
            HueBar_isDragging = false;

            //初始化RGB_HSV组件
            RGB_and_HSV_focus = false;
            RGB_and_HSV_assignment = 0;
            RGB_and_HSV_invalidInput_Draw = false;
            RGB_and_HSV_resetIndex = true;
            RGB_HSV = [255, 255, 255, 0, 0, 100];
            RGB_HSV_name = ["R", "G", "B", "H", "S", "V"];
            selected_RGBHSV_color = [Calc.HexToColor("84FF54"), Calc.HexToColor("FCFF59")];
            selected_RGBHSV_row = 0;
            selected_RGBHSV_column = 0;
            selected_RGBHSV_index = 0;
            RGB_and_HSV_invalidInput_tipEase = 0.0f;

            //初始化组件提示框
            Tip_labels =
            [
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_HexColor_InputTip_DefaultMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_ColorPickerArea_InputTip_DefaultMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_HueBar_InputTip_DefaultMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InputTip_DefaultMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_HexColor_InputTip_EditMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_ColorPickerArea_InputTip_EditMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_HueBar_InputTip_EditMode_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InputTip_EditMode_Label"
            ];
            RGB_and_HSV_invalidInput_label =
            [
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InvalidInputTip_RGB_Label",
                "ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InvalidInputTip_HSV_Label"
            ];

            //初始化OUI时OUI的左上角位置
            Position = new Vector2(0f, 1080f);
            Visible = false;
        }

        //从别的OUI进入该OUI时应该做的事情
        public override IEnumerator<float> Enter(Oui from)
        {

            Visible = true;
            //修改HEX组件的设置，从别的界面进入拾色器时应该仅修改状态变量
            HexColor_focus = false;
            selected_HexColor_digit = 0;

            //修改拾色区域组件的设置，从别的界面进入拾色器时应该修改状态变量
            ColorPickerArea_focus = false;
            ColorPickerArea_isDragging = false;

            //修改色相条组件的设置，从别的界面进入拾色器时应该修改状态变量
            HueBar_focus = false;
            HueBar_isDragging = false;

            //修改色相条组件的设置，从别的界面进入拾色器时应该修改状态变量
            //数组索引是否重置需要根据从哪个界面而来进行不同处理，若是由拾色器打开数字输入键盘后再转回来则不需要重置索引
            RGB_and_HSV_focus = false;
            RGB_and_HSV_invalidInput_Draw = false;
            RGB_and_HSV_invalidInput_tipEase = 0.0f;
            if (RGB_and_HSV_resetIndex == true)
            {
                selected_RGBHSV_row = 0;
                selected_RGBHSV_column = 0;
                selected_RGBHSV_index = 0;
            }


            //修改通用属性，selected_component和need_synchronize_value不要在更改，要手动用Init函数在外界触发
            //修改OUI的位置来进行OUI的渐入，这里是OUI从画面底部往上进入
            Vector2 posFrom = Position;
            Vector2 posTo = Vector2.Zero;
            for (float t = 0.0f; t < 1.0f; t += Engine.DeltaTime * 2.0f)
            {
                ease = Ease.CubeIn(t);
                Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
                yield return 0.0f;
            }
            ease = 1.0f;
            //等一下再聚焦到此OUI
            yield return 0.2f;
            Focused = true;
            //记录一次鼠标状态
            Mouse_current_position = MInput.Mouse.Position;
            Mouse_previous_position = Mouse_current_position;
            Mouse_left_pressed = MInput.Mouse.CheckLeftButton;
        }

        //从该OUI进入到别的OUI时应该做的事情
        public override IEnumerator<float> Leave(Oui next)
        {
            //此时该OUI失焦，它不再与玩家交互
            Focused = false;
            //进行OUI的渐出，这里是OUI逐渐往下退出
            Vector2 posFrom = Position;
            Vector2 posTo = new Vector2(0f, 1080f);
            for (float t = 0.0f; t < 1.0f; t += Engine.DeltaTime * 2.0f)
            {
                ease = 1.0f - Ease.CubeIn(t);
                Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
                yield return 0.0f;
            }
            ease = 0.0f;
            //此时可能还有来自RGBHSV的提示协程运行，这里删除协程，并且初始化与之相关的参数
            if (RGB_and_HSV_invalidInput_tipEvent != null)
            {
                RGB_and_HSV_invalidInput_tipEvent.Cancel();
                RGB_and_HSV_invalidInput_tipEvent.RemoveSelf();
                RGB_and_HSV_invalidInput_tipEvent = null;
                RGB_and_HSV_invalidInput_tipEase = 0.0f;
                RGB_and_HSV_invalidInput_Draw = false;
            }
            //此时OUI不再可见
            Visible = false;
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
        }

        public override void Update()
        {
            //如果该OUI已被选中且被聚焦
            if (Selected && Focused)
            {
                //确定当前是哪个组件被激活
                Component_Active_Update();
                //根据组件状态依次更新各个组件
                HexColor_Update();
                ColorPickerArea_Update();
                HueBar_Update();
                RGB_and_HSV_Update();
                //处理由组件发出的值同步请求
                ColorValue_Synchronize();
                //获取最新的鼠标状态
                Mouse_Update();
                //如果协程已初始化且未完成，手动调用其 Update
                if (RGB_and_HSV_invalidInput_tipEvent != null && !RGB_and_HSV_invalidInput_tipEvent.Finished)
                {
                    RGB_and_HSV_invalidInput_tipEvent.Update(); // 驱动协程执行
                }
                //协程完成后清理
                else if (RGB_and_HSV_invalidInput_tipEvent != null && RGB_and_HSV_invalidInput_tipEvent.Finished)
                {
                    RGB_and_HSV_invalidInput_tipEvent = null;
                }
            }
            timer += Engine.DeltaTime;
            //更新每个Hex数字的摆动器
            for (int i = 0; i < HexColor_digits.Length; i++)
            {
                HexColor_wiggler_digits[i].Update();
            }
        }
        //用于结束拾色器界面用
        private void Finish()
        {
            Focused = false;
            Exit?.Invoke();
            Audio.Play("event:/ui/main/rename_entry_accept");
        }
        //用于组件激活状态的更新
        private void Component_Active_Update()
        {
            //Hex组件
            if (selected_component == 0)
            {
                //在Hex组件没有被聚焦的情况下
                if (HexColor_focus == false)
                {
                    //向下可以移动到拾色区域组件，按下返回键可以退出拾色器的OUI
                    if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        Finish();
                    }
                    else if (Input.MenuDown.Pressed)
                    {
                        selected_component = 1;
                        Audio.Play("event:/ui/game/chatoptions_roll_up");
                    }
                }
            }
            //拾色区域组件
            else if (selected_component == 1)
            {
                if (ColorPickerArea_focus == false)
                {
                    //向上可以移动到HEX组件，向右可以移动到色相条组件，按下返回键可以退出拾色器的OUI
                    if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        Finish();
                    }
                    else if (Input.MenuUp.Pressed)
                    {
                        selected_component = 0;
                        Audio.Play("event:/ui/game/chatoptions_roll_up");
                    }
                    else if (Input.MenuRight.Pressed)
                    {
                        selected_component = 2;
                        Audio.Play("event:/ui/game/chatoptions_roll_down");
                    }
                }
            }
            //色相条组件
            else if (selected_component == 2)
            {
                if (HueBar_focus == false)
                {
                    //向上可以移动到HEX组件，向左可以移动到拾色区域组件，向右可以移动到RGBHSV组件，按下返回键可以退出拾色器的OUI
                    if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        Finish();
                    }
                    else if (Input.MenuUp.Pressed)
                    {
                        selected_component = 0;
                        Audio.Play("event:/ui/game/chatoptions_roll_up");
                    }
                    else if (Input.MenuLeft.Pressed)
                    {
                        selected_component = 1;
                        Audio.Play("event:/ui/game/chatoptions_roll_down");
                    }
                    else if (Input.MenuRight.Pressed)
                    {
                        selected_component = 3;
                        Audio.Play("event:/ui/game/chatoptions_roll_down");
                    }
                }
            }
            //RGBHSV组件
            else if (selected_component == 3)
            {
                if (RGB_and_HSV_focus == false)
                {
                    //向上可以移动到HEX组件，向左可以移动到色相条组件，按下返回键可以退出拾色器的OUI
                    if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                    {
                        Finish();
                    }
                    else if (Input.MenuUp.Pressed)
                    {
                        selected_component = 0;
                        Audio.Play("event:/ui/game/chatoptions_roll_up");
                    }
                    else if (Input.MenuLeft.Pressed)
                    {
                        selected_component = 2;
                        Audio.Play("event:/ui/game/chatoptions_roll_down");
                    }
                }
            }
        }
        //更新Hex颜色值组件的数字
        private void HexColor_Update()
        {
            if (selected_component != 0)
            {
                HexColor_focus = false;
                selected_HexColor_digit = 0;
                return;
            }
            //Hex组件还没有聚焦时可以经由确认键进入聚焦状态
            if (!HexColor_focus)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    HexColor_focus = true;
                    selected_HexColor_digit = 0;
                    Audio.Play("event:/ui/game/chatoptions_appear");
                }
            }
            else if (HexColor_focus)
            {
                //使用各种退出键来退出聚焦状态
                if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                {
                    HexColor_focus = false;
                    Audio.Play("event:/ui/main/button_back");
                }
                //在聚焦状态下再次按确认键则更新值，同时退出聚焦状态
                else if (Input.MenuConfirm.Pressed)
                {
                    need_synchronize_value = 0;//Hex颜色值组件发起同步
                    HexColor_focus = false;
                    Audio.Play("event:/ui/main/button_select");
                }
                //通过上下键修改当前数字的值
                else if (Input.MenuDown.Pressed)
                {
                    
                    HexColor_digits[selected_HexColor_digit] = HexColor_digits[selected_HexColor_digit] - 1;
                    if (HexColor_digits[selected_HexColor_digit] < 0)
                    {
                        HexColor_digits[selected_HexColor_digit] = HexColor_digits[selected_HexColor_digit] + 16;
                    }
                    HexColor_wiggler_digits[selected_HexColor_digit].Start();//对应数字开始抖动
                    Audio.Play("event:/ui/game/chatoptions_roll_down");
                }
                else if (Input.MenuUp.Pressed)
                {
                    HexColor_digits[selected_HexColor_digit] = (HexColor_digits[selected_HexColor_digit] + 1) % 16;
                    HexColor_wiggler_digits[selected_HexColor_digit].Start();//对应数字开始抖动
                    Audio.Play("event:/ui/game/chatoptions_roll_up");
                }
                //通过左右键修改选中的数字
                else if (Input.MenuRight.Pressed)
                {
                    selected_HexColor_digit = (selected_HexColor_digit + 1) % 6;
                    Audio.Play("event:/ui/main/rollover_down");
                }
                else if (Input.MenuLeft.Pressed)
                {
                    selected_HexColor_digit = (selected_HexColor_digit - 1) % 6;
                    Audio.Play("event:/ui/main/rollover_up");
                }

            }
        }
        //更新拾色区域
        private void ColorPickerArea_Update()
        {
            if (selected_component != 1)
            {
                ColorPickerArea_focus = false;
                ColorPickerArea_isDragging = false;
                return;
            }
            if (!ColorPickerArea_focus)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    ColorPickerArea_focus = true;
                    Audio.Play("event:/ui/game/chatoptions_appear");
                }
            }
            else if (ColorPickerArea_focus)
            {
                //使用各种退出键来退出聚焦状态
                if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                {
                    ColorPickerArea_focus = false;
                    ColorPickerArea_isDragging = false;
                    Audio.Play("event:/ui/main/button_back");
                    //不要再继续执行了
                    return;
                }
                //如果鼠标处于按压状态，则激活拖动状态
                if (Mouse_left_pressed == true)
                {
                    ColorPickerArea_isDragging = true;
                }
                //现在拾色区域滑块正在被拖拽
                if (ColorPickerArea_isDragging)
                {
                    //如果鼠标左键松开了，代表脱离了拖拽状态
                    if (Mouse_left_pressed == false)
                    {
                        ColorPickerArea_isDragging = false;
                    }
                    else
                    {
                        //计算像素并调整滑块位置
                        float relativeX = (Mouse_current_position.X - Mouse_previous_position.X) * ColorPickerArea_mouseSensitivity;
                        float relativeY = (Mouse_current_position.Y - Mouse_previous_position.Y) * ColorPickerArea_mouseSensitivity;
                        ColorPickerArea_currentValue.X = MathHelper.Clamp(ColorPickerArea_currentValue.X + relativeX / ColorPickerArea_pickerRectangle.Width, 0.0f, 1.0f);
                        ColorPickerArea_currentValue.Y = MathHelper.Clamp(ColorPickerArea_currentValue.Y + relativeY / ColorPickerArea_pickerRectangle.Height, 0.0f, 1.0f);
                        Vector2 current_knob_left_corner = ColorPickerArea_knob_leftup_corner + new Vector2(ColorPickerArea_currentValue.X * ColorPickerArea_pickerRectangle.Width, ColorPickerArea_currentValue.Y * ColorPickerArea_pickerRectangle.Height);
                        ColorPickerArea_knobInnerRectangle.X = (int)current_knob_left_corner.X;
                        ColorPickerArea_knobInnerRectangle.Y = (int)current_knob_left_corner.Y;
                        ColorPickerArea_knobOuterRectangle.X = (int)current_knob_left_corner.X - ColorPickerArea_knobBorderThickness;
                        ColorPickerArea_knobOuterRectangle.Y = (int)current_knob_left_corner.Y - ColorPickerArea_knobBorderThickness;

                    }
                }
                //发起同步
                need_synchronize_value = 1;
            }

        }
        //更新色相条
        private void HueBar_Update()
        {
            if (selected_component != 2)
            {
                HueBar_focus = false;
                HueBar_isDragging = false;
                return;
            }
            //色相条组件还没有聚焦时可以经由确认键进入聚焦状态，此时才可以进行色相值的修改
            if (!HueBar_focus)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    HueBar_focus = true;
                    Audio.Play("event:/ui/game/chatoptions_appear");
                }
            }
            else if (HueBar_focus)
            {
                //使用各种退出键来退出聚焦状态
                if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                {
                    HueBar_focus = false;
                    HueBar_isDragging = false;
                    Audio.Play("event:/ui/main/button_back");
                    //不要再继续执行了
                    return;
                }
                //如果鼠标处于按压状态，则激活拖动状态
                if (Mouse_left_pressed == true)
                {
                    HueBar_isDragging = true;
                }
                //现在色相条滑块正在被拖拽
                if (HueBar_isDragging)
                {
                    //如果鼠标左键松开了，代表脱离了拖拽状态
                    if (Mouse_left_pressed == false)
                    {
                        HueBar_isDragging = false;
                    }
                    else
                    {
                        //计算并调整滑块位置，同时更新色相值(纵轴坐标)
                        float relativeY = (Mouse_current_position.Y - Mouse_previous_position.Y) * HueBar_mouseSensitivity;
                        HueBar_currentHue = MathHelper.Clamp(HueBar_currentHue + relativeY / HueBar_barRectangle.Height, 0.0f, 1.0f);
                        float current_knob_left_corner_Y = HueBar_knob_leftup_corner.Y + HueBar_currentHue * HueBar_barRectangle.Height;
                        HueBar_knobInnerRectangle.Y = (int)current_knob_left_corner_Y;
                        HueBar_knobOuterRectangle.Y = (int)current_knob_left_corner_Y - HueBar_knobBorderThickness;
                    }
                }
                //发起同步
                need_synchronize_value = 2;
            }

        }
        //更新RGB和HSV显示组件
        private void RGB_and_HSV_Update()
        {
            if (selected_component != 3)
            {
                RGB_and_HSV_focus = false;
                RGB_and_HSV_assignment = 0;
                return;
            }
            //RGBHLS组件还没有聚焦时可以经由确认键进入聚焦状态，此时才可以进行值的修改
            if (!RGB_and_HSV_focus)
            {
                if (Input.MenuConfirm.Pressed)
                {
                    RGB_and_HSV_focus = true;
                    Audio.Play("event:/ui/game/chatoptions_appear");
                }
            }
            else if (RGB_and_HSV_focus)
            {
                //在聚焦状态下按下确认键，则进入修改值的界面，如果修改成功则需要同步至值
                if (Input.MenuConfirm.Pressed)
                {
                    RGB_and_HSV_focus = false;
                    //接下来前往数字键盘输入界面
                    Audio.Play("event:/ui/main/savefile_rename_start");
                    SceneAs<Overworld>().Goto<OuiNumberEntry>().Init<DBB_ColorPicker_OUI>(RGB_HSV[selected_RGBHSV_index],
                    (float value) =>
                    {
                        //在数字输入键盘中，每次变动值(确认后返回并不负责变动值，而是在数字键盘进行输入一个数字就相当于变动一次值)都触发一次这里
                        need_synchronize_value = 3;//此时发起同步
                        RGB_and_HSV_resetIndex = false;//此时不重置数组索引
                        int test_value = (int)value;
                        if (test_value >= 0 && test_value <= 255)
                        {
                            RGB_HSV[selected_RGBHSV_index] = test_value;
                            //如果是RGB中的一个被更新，则更新HSV
                            if (selected_RGBHSV_index < 3)
                            {
                                Vector3 hsv = 100.0f * DBBMath.RGBtoHSV(RGB_HSV[0] / 255.0f, RGB_HSV[1] / 255.0f, RGB_HSV[2] / 255.0f);
                                RGB_HSV[3] = (int)hsv.X;
                                RGB_HSV[4] = (int)hsv.Y;
                                RGB_HSV[5] = (int)hsv.Z;
                            }
                            //如果是HSV中的一个被更新，则更新RGB
                            else
                            {
                                Vector3 rgb = 255.0f * DBBMath.HSVtoRGB_Normalized(RGB_HSV[3] / 100.0f, RGB_HSV[4] / 100.0f, RGB_HSV[5] / 100.0f);
                                RGB_HSV[0] = (int)Math.Round(rgb.X);
                                RGB_HSV[1] = (int)Math.Round(rgb.Y);
                                RGB_HSV[2] = (int)Math.Round(rgb.Z);
                            }
                            //直接在这里更新所有值
                            int R = RGB_HSV[0];
                            int G = RGB_HSV[1];
                            int B = RGB_HSV[2];
                            int H = RGB_HSV[3];
                            int S = RGB_HSV[4];
                            int V = RGB_HSV[5];
                            //更新Hex值
                            HexColor_digits[0] = R / 16; HexColor_digits[1] = R % 16;
                            HexColor_digits[2] = G / 16; HexColor_digits[3] = G % 16;
                            HexColor_digits[4] = B / 16; HexColor_digits[5] = B % 16;
                            //求HSV
                            Vector3 tmp_hsv = new Vector3(H / 100.0f, S / 100.0f, V / 100.0f);
                            //更新色相条和拾色区域
                            HueBar_currentHue = tmp_hsv.X;
                            HueBar_knobInnerColor = DBBMath.HSVtoRGB(tmp_hsv.X, 1.0f, 1.0f);
                            float current_knob_left_corner_Y = HueBar_knob_leftup_corner.Y + HueBar_currentHue * HueBar_barRectangle.Height;
                            HueBar_knobInnerRectangle.Y = (int)current_knob_left_corner_Y;
                            HueBar_knobOuterRectangle.Y = (int)current_knob_left_corner_Y - HueBar_knobBorderThickness;

                            ColorPickerArea_currentValue = new Vector2(tmp_hsv.Y, 1.0f - tmp_hsv.Z);
                            ColorPickerArea_currentColor = DBBMath.HSVtoRGB(HueBar_currentHue, tmp_hsv.Y, tmp_hsv.Z);
                            Vector2 current_knob_left_corner = ColorPickerArea_knob_leftup_corner + new Vector2(ColorPickerArea_currentValue.X * ColorPickerArea_pickerRectangle.Width, ColorPickerArea_currentValue.Y * ColorPickerArea_pickerRectangle.Height);
                            ColorPickerArea_knobInnerRectangle.X = (int)current_knob_left_corner.X;
                            ColorPickerArea_knobInnerRectangle.Y = (int)current_knob_left_corner.Y;
                            ColorPickerArea_knobOuterRectangle.X = (int)current_knob_left_corner.X - ColorPickerArea_knobBorderThickness;
                            ColorPickerArea_knobOuterRectangle.Y = (int)current_knob_left_corner.Y - ColorPickerArea_knobBorderThickness;

                            RGB_and_HSV_assignment = 1;//赋值成功
                        }
                        else
                        {
                            RGB_and_HSV_assignment = -1;//赋值失败
                        }    
                    },
                    3, false, false);
                }
                //在聚焦状态下按下返回键则退出聚焦状态
                else if (Input.MenuCancel.Pressed || Input.Pause.Pressed || Input.ESC.Pressed)
                {
                    RGB_and_HSV_focus = false;
                    Audio.Play("event:/ui/main/button_back");
                }
                //通过上下左右键修改选中的属性
                else if (Input.MenuDown.Pressed)
                {
                    selected_RGBHSV_row = (selected_RGBHSV_row + 1) % 3;
                    selected_RGBHSV_index = selected_RGBHSV_column * 3 + selected_RGBHSV_row;
                    Audio.Play("event:/ui/game/chatoptions_roll_down");
                }
                else if (Input.MenuUp.Pressed)
                {
                    selected_RGBHSV_row = selected_RGBHSV_row - 1;
                    if (selected_RGBHSV_row < 0)
                    {
                        selected_RGBHSV_row += 3;
                    }
                    
                    selected_RGBHSV_index = selected_RGBHSV_column * 3 + selected_RGBHSV_row;
                    Audio.Play("event:/ui/game/chatoptions_roll_up");
                }
                //通过左右键修改选中的数字
                else if (Input.MenuRight.Pressed)
                {
                    selected_RGBHSV_column = (selected_RGBHSV_column + 1) % 2;
                    selected_RGBHSV_index = selected_RGBHSV_column * 3 + selected_RGBHSV_row;
                    Audio.Play("event:/ui/main/rollover_down");
                }
                else if (Input.MenuLeft.Pressed)
                {
                    selected_RGBHSV_column = selected_RGBHSV_column - 1;
                    if (selected_RGBHSV_column < 0)
                    {
                        selected_RGBHSV_column += 2;
                    }
                    selected_RGBHSV_index = selected_RGBHSV_column * 3 + selected_RGBHSV_row;
                    Audio.Play("event:/ui/main/rollover_up");
                }
            }
        }
        //用于处理各个组件的值同步
        private void ColorValue_Synchronize()
        {
            //不需要进行同步
            if (need_synchronize_value == -1)
            {
                return;
            }
            //同步由Hex组件发起
            else if (need_synchronize_value == 0)
            {
                int R = HexColor_digits[0] * 16 + HexColor_digits[1];
                int G = HexColor_digits[2] * 16 + HexColor_digits[3];
                int B = HexColor_digits[4] * 16 + HexColor_digits[5];
                //求HSV
                Vector3 hsv = DBBMath.RGBtoHSV(R / 255.0f, G / 255.0f, B / 255.0f);
                //更新色相条和拾色区域
                HueBar_currentHue = hsv.X;
                HueBar_knobInnerColor = DBBMath.HSVtoRGB(hsv.X, 1.0f, 1.0f);
                float current_knob_left_corner_Y = HueBar_knob_leftup_corner.Y + HueBar_currentHue * HueBar_barRectangle.Height;
                HueBar_knobInnerRectangle.Y = (int)current_knob_left_corner_Y;
                HueBar_knobOuterRectangle.Y = (int)current_knob_left_corner_Y - HueBar_knobBorderThickness;

                ColorPickerArea_currentValue = new Vector2(hsv.Y, 1.0f - hsv.Z);
                ColorPickerArea_currentColor = DBBMath.HSVtoRGB(HueBar_currentHue, hsv.Y, hsv.Z);
                Vector2 current_knob_left_corner = ColorPickerArea_knob_leftup_corner + new Vector2(ColorPickerArea_currentValue.X * ColorPickerArea_pickerRectangle.Width, ColorPickerArea_currentValue.Y * ColorPickerArea_pickerRectangle.Height);
                ColorPickerArea_knobInnerRectangle.X = (int)current_knob_left_corner.X;
                ColorPickerArea_knobInnerRectangle.Y = (int)current_knob_left_corner.Y;
                ColorPickerArea_knobOuterRectangle.X = (int)current_knob_left_corner.X - ColorPickerArea_knobBorderThickness;
                ColorPickerArea_knobOuterRectangle.Y = (int)current_knob_left_corner.Y - ColorPickerArea_knobBorderThickness;
                //更新RGBHSV
                hsv *= 100.0f;
                int H = (int)Math.Round(hsv.X);
                int S = (int)Math.Round(hsv.Y);
                int V = (int)Math.Round(hsv.Z);
                RGB_HSV[0] = R;
                RGB_HSV[1] = G;
                RGB_HSV[2] = B;
                RGB_HSV[3] = H;
                RGB_HSV[4] = S;
                RGB_HSV[5] = V;
            }
            //同步由拾色区域或色相条组件发起
            else if (need_synchronize_value == 1 || need_synchronize_value == 2)
            {
                //由色相条发起需要更新拾色区域滑块的颜色，而由拾色区域发起不需要更新色相条滑块的颜色
                ColorPickerArea_currentColor = DBBMath.HSVtoRGB(HueBar_currentHue, ColorPickerArea_currentValue.X, 1.0f - ColorPickerArea_currentValue.Y );
                HueBar_knobInnerColor = DBBMath.HSVtoRGB(HueBar_currentHue, 1.0f, 1.0f);
                Vector3 RGB = 255.0f * ColorPickerArea_currentColor.ToVector3();
                //更新RGBHSV
                int R = (int)RGB.X;
                int G = (int)RGB.Y;
                int B = (int)RGB.Z;
                int H = (int)Math.Round(100.0f * HueBar_currentHue);
                int S = (int)Math.Round(100.0f * ColorPickerArea_currentValue.X);
                int V = (int)Math.Round(100.0f * (1.0f - ColorPickerArea_currentValue.Y));
                RGB_HSV[0] = R;
                RGB_HSV[1] = G;
                RGB_HSV[2] = B;
                RGB_HSV[3] = H;
                RGB_HSV[4] = S;
                RGB_HSV[5] = V;
                //更新Hex值
                HexColor_digits[0] = R / 16; HexColor_digits[1] = R % 16;
                HexColor_digits[2] = G / 16; HexColor_digits[3] = G % 16;
                HexColor_digits[4] = B / 16; HexColor_digits[5] = B % 16;
            }
            //同步由RGBHSV组件发起
            else if (need_synchronize_value == 3)
            {
                //如果赋值失败
                if (RGB_and_HSV_assignment == -1)
                {
                    //在上一个协程执行完之后添加另一个协程，否则进行等待
                    if (RGB_and_HSV_invalidInput_tipEvent == null)
                    {
                        RGB_and_HSV_invalidInput_Draw = true;
                        RGB_and_HSV_invalidInput_tipEvent = new Coroutine(RGB_and_HSV_invalidValue_tipEvent());
                        RGB_and_HSV_assignment = 0;
                    }
                }

            }
            //录入字符串，例如，对于应显示为ABCDEF的字符串，final_HexColor_for_show[0]为F
            final_HexColor_for_show = "";
            for (int i = 0; i < 6; i++)
            {
                final_HexColor_for_show += DBBSettings.HexTable[HexColor_digits[i]];
            }
            //处理完成，取消同步请求
            need_synchronize_value = -1;
        }
        //记录最新的鼠标状态
        private void Mouse_Update()
        {
            Mouse_previous_position = Mouse_current_position;
            Mouse_current_position = MInput.Mouse.Position;
            Mouse_left_pressed = MInput.Mouse.CheckLeftButton;
        }

        private IEnumerator<float> RGB_and_HSV_invalidValue_tipEvent()
        {
            for (float t = 0.0f; t < 1.0f; t += Engine.RawDeltaTime * 1.0f)
            {

                RGB_and_HSV_invalidInput_tipEase = DBBMath.MotionMapping(t, "easeInOutSin");
                yield return 0.0f;
            }
            RGB_and_HSV_invalidInput_tipEase = 1.0f;
            yield return 2.0f;
            for (float t = 1.0f; t > 0.0f; t -= Engine.RawDeltaTime * 1.0f)
                {
                    RGB_and_HSV_invalidInput_tipEase = DBBMath.MotionMapping(t, "easeInOutSin");
                    yield return 0.0f;
                }
            RGB_and_HSV_invalidInput_tipEase = 0.0f;
            RGB_and_HSV_invalidInput_Draw = false;
            yield return 0.0f;
        }


        /// <summary>
        /// 设置拾色器界面，需要外界手动调用
        /// <para>HexColor_label：HEX颜色值组件在文本文件中对应的标签，文本文件中该标签所对应的内容将会被展示</para>
        /// <para>initial_HexColor：初始的颜色值，即当一开始进入拾色器界面时应该显示的颜色</para>
        /// <para>HexColor_InputTip_Label：Hex颜色值组件的输入提示，第一项为默认模式下的提示，第二项为编辑模式下的提示</para>
        /// <para>ColorPickerArea_InputTip_Label：拾色区域组件的输入提示，第一项为默认模式下的提示，第二项为编辑模式下的提示</para>
        /// <para>HueBar_InputTip_Label：色相条组件的输入提示，第一项为默认模式下的提示，第二项为编辑模式下的提示</para>
        /// <para>RGB_and_HSV_InputTip_Label：RGB和HSV组件的输入提示，第一项为默认模式下的提示，第二项为编辑模式下的提示</para>
        /// <para>RGB_and_HSV_InvalidInputTip_Label：RGB和HSV组件的非法值输入提示，第一项为RGB的非法值输入提示，第二项为HSV的非法值输入提示</para>
        /// <para>MouseSensitivity：控制滑块的灵敏度，第一项为拾色区域滑块的灵敏度，第二项为色相条滑块的灵敏度</para>
        /// <para>do_something_before_OuiLeave：在离开拾色器界面前应当做的事情</para>
        /// <para>T：从拾色器界面离开时应该进入到哪个界面</para>
        /// </summary>
        /// <returns>拾色器界面对象自身</returns>
        public DBB_ColorPicker_OUI Init<T>(
            string HexColor_Label,
            string initial_HexColor,
            Tuple<string, string> HexColor_InputTip_Label,
            Tuple<string, string> ColorPickerArea_InputTip_Label,
            Tuple<string, string> HueBar_InputTip_Label,
            Tuple<string, string> RGB_and_HSV_InputTip_Label,
            Tuple<string, string> RGB_and_HSV_InvalidInputTip_Label,
            Tuple<float, float> MouseSensitivity,
            Action do_something_when_OuiLeave = null
        ) where T : Oui
        {
            //设置一系列组件的标题、提示内容的标签
            HexColor_label = HexColor_Label;
            Tip_labels[0] = HexColor_InputTip_Label.Item1;
            Tip_labels[1] = ColorPickerArea_InputTip_Label.Item1;
            Tip_labels[2] = HueBar_InputTip_Label.Item1;
            Tip_labels[3] = RGB_and_HSV_InputTip_Label.Item1;
            Tip_labels[4] = HexColor_InputTip_Label.Item2;
            Tip_labels[5] = ColorPickerArea_InputTip_Label.Item2;
            Tip_labels[6] = HueBar_InputTip_Label.Item2;
            Tip_labels[7] = RGB_and_HSV_InputTip_Label.Item2;

            RGB_and_HSV_invalidInput_label[0] = RGB_and_HSV_InvalidInputTip_Label.Item1;
            RGB_and_HSV_invalidInput_label[1] = RGB_and_HSV_InvalidInputTip_Label.Item2;

            //设置一系列组件的非颜色属性
            //鼠标灵敏度
            ColorPickerArea_mouseSensitivity = MouseSensitivity.Item1;
            HueBar_mouseSensitivity = MouseSensitivity.Item2;


            //根据提供的initial_HexColor来确定一系列初始颜色值
            //获取合法的颜色值
            Vector4 valid_color = DBBMath.ConvertColor(initial_HexColor);
            Vector3 hsv = DBBMath.RGBtoHSV(valid_color.X, valid_color.Y, valid_color.Z);
            valid_color *= 255.0f;
            int R = (int)valid_color.X;
            int G = (int)valid_color.Y;
            int B = (int)valid_color.Z;
            //更新Hex组件值
            HexColor_digits[0] = R / 16; HexColor_digits[1] = R % 16;
            HexColor_digits[2] = G / 16; HexColor_digits[3] = G % 16;
            HexColor_digits[4] = B / 16; HexColor_digits[5] = B % 16;
            //调整拾色区域和色相条的值
            HueBar_currentHue = hsv.X;
            HueBar_knobInnerColor = DBBMath.HSVtoRGB(hsv.X, 1.0f, 1.0f);
            float current_knob_left_corner_Y = HueBar_knob_leftup_corner.Y + HueBar_currentHue * HueBar_barRectangle.Height;
            HueBar_knobInnerRectangle.Y = (int)current_knob_left_corner_Y;
            HueBar_knobOuterRectangle.Y = (int)current_knob_left_corner_Y - HueBar_knobBorderThickness;

            ColorPickerArea_currentValue = new Vector2(hsv.Y, 1.0f - hsv.Z);
            ColorPickerArea_currentColor = DBBMath.HSVtoRGB(HueBar_currentHue, hsv.Y, hsv.Z);
            Vector2 current_knob_left_corner = ColorPickerArea_knob_leftup_corner + new Vector2(ColorPickerArea_currentValue.X * ColorPickerArea_pickerRectangle.Width, ColorPickerArea_currentValue.Y * ColorPickerArea_pickerRectangle.Height);
            ColorPickerArea_knobInnerRectangle.X = (int)current_knob_left_corner.X;
            ColorPickerArea_knobInnerRectangle.Y = (int)current_knob_left_corner.Y;
            ColorPickerArea_knobOuterRectangle.X = (int)current_knob_left_corner.X - ColorPickerArea_knobBorderThickness;
            ColorPickerArea_knobOuterRectangle.Y = (int)current_knob_left_corner.Y - ColorPickerArea_knobBorderThickness;

            //调整RGBHSV组件的值
            hsv *= 100.0f;
            int H = (int)Math.Round(hsv.X);
            int S = (int)Math.Round(hsv.Y);
            int V = (int)Math.Round(hsv.Z);
            RGB_HSV[0] = R; RGB_HSV[1] = G; RGB_HSV[2] = B;
            RGB_HSV[3] = H; RGB_HSV[4] = S; RGB_HSV[5] = V;
            //录入字符串，这里是倒着录入的，例如，对于应显示为ABCDEF的字符串，final_HexColor_for_show[0]为F
            final_HexColor_for_show = "";
            for (int i = 0; i < 6; i++)
            {
                final_HexColor_for_show += DBBSettings.HexTable[HexColor_digits[i]];
            }
            //记录一次鼠标状态
            Mouse_current_position = MInput.Mouse.Position;
            Mouse_previous_position = Mouse_current_position;
            Mouse_left_pressed = MInput.Mouse.CheckLeftButton;
            //更新Exit事件
            Exit = delegate
            {
                //执行一下用户自定义的do_something_when_OuiLeave事件
                do_something_when_OuiLeave?.Invoke();
                base.Overworld.Goto<T>();
            };
            //初始化状态值
            selected_component = 0;
            need_synchronize_value = -1;

            HexColor_focus = false;
            selected_HexColor_digit = 0;

            ColorPickerArea_focus = false;
            ColorPickerArea_isDragging = false;

            HueBar_focus = false;
            HueBar_isDragging = false;

            RGB_and_HSV_focus = false;
            RGB_and_HSV_assignment = 0;
            RGB_and_HSV_resetIndex = true;
            selected_RGBHSV_row = 0;
            selected_RGBHSV_column = 0;
            selected_RGBHSV_index = 0;
            if (RGB_and_HSV_invalidInput_tipEvent != null)
            {
                RGB_and_HSV_invalidInput_tipEvent.Cancel();
                RGB_and_HSV_invalidInput_tipEvent.RemoveSelf();
                RGB_and_HSV_invalidInput_tipEvent = null;
                RGB_and_HSV_invalidInput_tipEase = 0.0f;
                RGB_and_HSV_invalidInput_Draw = false;
            }

            return this;
        }

        /// <summary>
        /// 绘制整个拾色器界面
        /// </summary>
        public override void Render()
        {

            if (Visible == false)
            {
                return;
            }
            //绘制一层半透明的黑色底色
            if (ease > 0f)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * ease * 0.4f);
            }
            //Hex颜色值
            Vector2 HexColor_offset = new Vector2(350, 200);
            float HexColor_width = 1200.0f;
            float HexColor_height=HexColor_Render(Position, HexColor_offset, HexColor_width);

            //拾色区域
            Vector2 ColorPickerArea_offset = HexColor_offset + new Vector2(0.0f, HexColor_height + 40.0f);
            Vector2 ColorPickerArea_size = ColorPickerArea_Render(Position, ColorPickerArea_offset);

            //色相条
            Vector2 HueBar_offset = ColorPickerArea_offset + new Vector2(ColorPickerArea_size.X + 40.0f, 0.0f);
            Vector2 HueBar_size = HueBar_Render(Position, HueBar_offset);

            //RGBHSV
            Vector2 RGBHSV_offset = ColorPickerArea_offset;
            Vector2 RGBHSV_size = RGB_and_HSV_Render(Position, RGBHSV_offset, HexColor_width);

            //轮廓线
            bool component_focused = HexColor_focus | HueBar_focus | RGB_and_HSV_focus | ColorPickerArea_focus;//是否有组件被聚焦
            int outline_thickness = 5;
            Vector2 outline_lefttop_pos = Position;
            Vector2 outline_width_height = Vector2.Zero;
            if (selected_component == 0)
            {
                outline_lefttop_pos += HexColor_offset;
                outline_width_height += new Vector2(HexColor_width, HexColor_height);
            }
            else if (selected_component == 1)
            {
                outline_lefttop_pos += ColorPickerArea_offset - 0.5f * new Vector2(ColorPickerArea_knobOuterRectangle.Width, ColorPickerArea_knobOuterRectangle.Height);
                outline_width_height += ColorPickerArea_size;
            }
            else if (selected_component == 2)
            {
                outline_lefttop_pos += HueBar_offset - 0.5f * new Vector2(HueBar_knobOuterRectangle.Width, HueBar_knobOuterRectangle.Height);
                outline_width_height += HueBar_size;
            }
            else if (selected_component == 3)
            {
                outline_lefttop_pos += RGBHSV_offset + new Vector2(HexColor_width - RGBHSV_size.X, 0.0f);
                outline_width_height += RGBHSV_size;
            }
            Rectangle outline_area = new Rectangle((int)outline_lefttop_pos.X, (int)outline_lefttop_pos.Y, (int)outline_width_height.X, (int)outline_width_height.Y);
            OutLine_Render(outline_area, outline_thickness, component_focused);
            //组件提示框
            float cofficient_for_textColor = ease;
            float cofficient_for_outlineColor = DBBMath.MotionMapping(ease, "easeInOutSin");
            Vector2 Tip_offset = Position + ColorPickerArea_offset + new Vector2(0.0f, 300.0f);
            int Tip_labels_index = component_focused ? selected_component + 4 : selected_component;
            ActiveFont.DrawOutline(Dialog.Clean(Tip_labels[Tip_labels_index]), Tip_offset, Vector2.Zero, Vector2.One, Color.White * cofficient_for_textColor, 2.0f, Color.Black * cofficient_for_outlineColor);

        }
        //绘制Hex颜色值组件
        //canvas_lefttop_pos为画布左上角的位置
        //offest为该组件左上角位置相对于画布左上角位置的偏移量
        //ref_width为该组件的参考宽度，该组件将会在左侧绘制label名称，在右侧绘制当前颜色的16进制代码串
        //返回该组件所有字体所占用的最大高度
        private float HexColor_Render(Vector2 canvas_lefttop_pos, Vector2 offset, float ref_width)
        {
            //计算真正的左上角位置，绘制将基于该点的位置开始
            Vector2 real_left_top = canvas_lefttop_pos + offset;
            //计算真正的右上角位置
            Vector2 real_right_top = real_left_top;
            real_right_top.X += ref_width;
            //计算颜色和轮廓颜色的系数，这些值用于界面渐入渐出时文字的颜色渐入渐出
            float cofficient_for_textColor = ease;
            float cofficient_for_outlineColor = DBBMath.MotionMapping(ease, "easeInOutSin");
            //左对齐绘制label，这里ease用于界面渐入和渐出时文字的颜色渐强和渐弱处理
            //Vector2.UnitX为右对齐，Vector2.Zero为左对齐
            //为什么绘制UI还要进行用屏幕矩阵进行缩放.jpg
            //这里Draw.SpriteBatch.Begin();将矩阵替换为了单位阵，
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin();
            ActiveFont.DrawOutline(HexColor_label, real_left_top, Vector2.Zero, Vector2.One, Color.White * cofficient_for_textColor, 2.0f, Color.Black * cofficient_for_outlineColor);
            //右对齐绘制当前颜色的16进制代码串
            for (int i = 5; i >= 0; i--)
            {
                Vector2 char_offset = new Vector2(MAX_CHAR_WIDTH * (i - 5), HexColor_wiggler_digits[i].Value * 8.0f);
                //确定每个字符的颜色，默认为白色，如果组件被激活且该字符被选中，则根据时间间隔来交替选择两种颜色
                Color char_color = unselected_HexColor_color;
                if (HexColor_focus == true && selected_HexColor_digit == i)
                {
                    char_color = Calc.BetweenInterval(timer, 0.1f) ? selected_HexColor_color[0] : selected_HexColor_color[1];
                }
                //逐个绘制字符，每个字符有自己的偏移，对于颜色码ABCDEF而言，F应在最右侧，这里ease用于界面渐入和渐出时文字的颜色渐强和渐弱处理
                ActiveFont.DrawOutline(DBBSettings.HexTable[HexColor_digits[i]], real_right_top + char_offset, Vector2.UnitX, Vector2.One, char_color * cofficient_for_textColor, 2.0f, Color.Black * cofficient_for_outlineColor);
            }
            //返回该组件所有字体所占用的最大高度
            return ActiveFont.Measure(Dialog.Clean(HexColor_label) + "0123456789ABCDEF").Y;
        }

        //绘制拾色区域组件
        //canvas_lefttop_pos为画布左上角的位置
        //offest为该组件左上角位置相对于画布左上角位置的偏移量
        //返回拾色区域所占用的最大宽高
        private Vector2 ColorPickerArea_Render(Vector2 canvas_lefttop_pos, Vector2 offset)
        {
            //计算真正的左上角位置，绘制将基于该点的位置开始
            Vector2 real_left_top = canvas_lefttop_pos + offset;
            int real_left_top_X = (int)real_left_top.X;
            int real_left_top_Y = (int)real_left_top.Y;
            Rectangle pickerRectangle = ColorPickerArea_pickerRectangle;
            pickerRectangle.X += real_left_top_X;
            pickerRectangle.Y += real_left_top_Y;

            Rectangle knobOuterRectangle = ColorPickerArea_knobOuterRectangle;
            knobOuterRectangle.X += real_left_top_X;
            knobOuterRectangle.Y += real_left_top_Y;

            Rectangle knobInnerRectangle = ColorPickerArea_knobInnerRectangle;
            knobInnerRectangle.X += real_left_top_X;
            knobInnerRectangle.Y += real_left_top_Y;

            Draw.SpriteBatch.End();
            DBBEffectSourceManager.DBBEffect["DBBColorPicker"].Parameters["hue"].SetValue(HueBar_currentHue);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBColorPicker"], Matrix.Identity);
            Draw.SpriteBatch.Draw(General_square.Texture.Texture_Safe, pickerRectangle, Color.White * ease);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin();
            //绘制滑块，叠加两层，底层大一些，顶层小一些用于构成边框的效果
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, knobOuterRectangle, Draw.Pixel.ClipRect, ColorPickerArea_knobBorderColor * ease * 0.8f);//绘制滑块外部
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, knobInnerRectangle, Draw.Pixel.ClipRect, ColorPickerArea_currentColor * ease);//绘制滑块内部
            //返回拾色区域所占用的最大宽高
            return new Vector2(pickerRectangle.Width + knobOuterRectangle.Width, pickerRectangle.Height + knobOuterRectangle.Height);
        }

        //绘制色相条组件
        //canvas_lefttop_pos为画布左上角的位置
        //offest为该组件左上角位置相对于画布左上角位置的偏移量
        //返回色相条所占用的最大宽高
        private Vector2 HueBar_Render(Vector2 canvas_lefttop_pos, Vector2 offset)
        {
            //计算真正的左上角位置，绘制将基于该点的位置开始
            Vector2 real_left_top = canvas_lefttop_pos + offset;
            int real_left_top_X = (int)real_left_top.X;
            int real_left_top_Y = (int)real_left_top.Y;
            Rectangle barRectangle = HueBar_barRectangle;
            barRectangle.X += real_left_top_X;
            barRectangle.Y += real_left_top_Y;

            Rectangle knobOuterRectangle = HueBar_knobOuterRectangle;
            knobOuterRectangle.X += real_left_top_X;
            knobOuterRectangle.Y += real_left_top_Y;

            Rectangle knobInnerRectangle = HueBar_knobInnerRectangle;
            knobInnerRectangle.X += real_left_top_X;
            knobInnerRectangle.Y += real_left_top_Y;

            Draw.SpriteBatch.End();//Engine.ScreenMatrix
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBColorSpectrum"], Matrix.Identity);
            Draw.SpriteBatch.Draw(General_square.Texture.Texture_Safe, barRectangle, Color.White * ease);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin();
            //绘制滑块，叠加两层，底层大一些，顶层小一些用于构成边框的效果
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, knobOuterRectangle, Draw.Pixel.ClipRect, HueBar_knobBorderColor * ease * 0.8f);//绘制滑块外部
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, knobInnerRectangle, Draw.Pixel.ClipRect, HueBar_knobInnerColor * ease);//绘制滑块内部
            //返回拾色区域所占用的最大宽高
            return new Vector2(barRectangle.Width + knobOuterRectangle.Width, barRectangle.Height + knobOuterRectangle.Height);

        }
        //绘制RGBHSV组件
        //canvas_lefttop_pos为画布左上角的位置
        //offest为该组件左上角位置相对于画布左上角位置的偏移量
        //ref_width为该组件的参考宽度，该组件右对齐进行绘制
        //返回所占据的区域的宽高
        private Vector2 RGB_and_HSV_Render(Vector2 canvas_lefttop_pos, Vector2 offset, float ref_width)
        {
            //计算真正的左上角位置，绘制将基于该点的位置开始
            Vector2 real_left_top = canvas_lefttop_pos + offset;
            //计算真正的右上角位置
            Vector2 real_right_top = real_left_top;
            real_right_top.X += ref_width;
            //计算颜色和轮廓颜色的系数，这些值用于界面渐入渐出时文字的颜色渐入渐出
            float cofficient_for_textColor = ease;
            float cofficient_for_outlineColor = DBBMath.MotionMapping(ease, "easeInOutSin");
            //按照右对齐的方式依次绘制HSV栏和RGB栏
            //首先计算一块的宽度和高度
            Vector2 block_size = DBBMath.Max(ActiveFont.Measure("R: 255"), ActiveFont.Measure("H: 100")) + new Vector2(20.0f, 10.0f);
            for (int i = 1; i >= 0; i--)
            {
                for (int j = 0; j < 3; j++)
                {
                    //绘制各块字符
                    int index = i * 3 + j;
                    Vector2 block_offset = new Vector2(block_size.X * (i - 1), block_size.Y * j);
                    //确定每个字符的颜色，默认为白色，如果组件被激活且该字符被选中，则根据时间间隔来交替选择两种颜色
                    Color block_color = Color.White;
                    if (RGB_and_HSV_focus == true && selected_RGBHSV_index == index)
                    {
                        block_color = Calc.BetweenInterval(timer, 0.1f) ? selected_RGBHSV_color[0] : selected_RGBHSV_color[1];
                    }
                    ActiveFont.DrawOutline(RGB_HSV_name[index] + ": " + RGB_HSV[index].ToString(), real_right_top + block_offset, Vector2.UnitX, Vector2.One, block_color * cofficient_for_textColor, 2.0f, Color.Black * cofficient_for_outlineColor);
                }
            }
            //在输入非法值后，如有需要则绘制输入错误提示
            if (RGB_and_HSV_invalidInput_Draw == true)
            {
                Vector2 tip_offset_start = new Vector2(800.0f, block_size.Y * 3.0f);
                Vector2 tip_offset_end = new Vector2(0.0f, block_size.Y * 3.0f);
                Vector2 tip_offset = DBBMath.Linear_Lerp(RGB_and_HSV_invalidInput_tipEase, tip_offset_start, tip_offset_end);
                string invalidInput_label;
                if (selected_RGBHSV_index < 3)
                {
                    invalidInput_label = RGB_and_HSV_invalidInput_label[0];
                }
                else
                {
                    invalidInput_label = RGB_and_HSV_invalidInput_label[1];
                }
                ActiveFont.DrawOutline(Dialog.Clean(invalidInput_label), real_right_top + tip_offset, Vector2.UnitX, 0.5f * Vector2.One, Color.Red * cofficient_for_textColor * RGB_and_HSV_invalidInput_tipEase, 1.5f, Color.Black * cofficient_for_outlineColor * RGB_and_HSV_invalidInput_tipEase);
            }
            //返回RGBHSV所占据的区域的宽高
            return new Vector2(block_size.X * 2.0f - 20.0f, block_size.Y * 3.0f - 10.0f);
        }

        //绘制轮廓线组件
        //render_area为绘制区域
        //focused为组件是否处于聚焦状态
        private void OutLine_Render(Rectangle render_area, int outline_thickness, bool focused)
        {
            Color outline_color = selected_OutLine_color;
            //聚焦时采用别的颜色
            if (focused)
            {
                outline_color = focused_OutLine_color;
            }
            //计算上下左右边框的矩形
            Rectangle up = new Rectangle(render_area.X - outline_thickness, render_area.Y - outline_thickness, render_area.Width + 2 * outline_thickness, outline_thickness);
            Rectangle down = new Rectangle(render_area.X - outline_thickness, render_area.Y + render_area.Height, render_area.Width + 2 * outline_thickness, outline_thickness);
            Rectangle left = new Rectangle(render_area.X - outline_thickness, render_area.Y - outline_thickness, outline_thickness, render_area.Height + 2 * outline_thickness);
            Rectangle right = new Rectangle(render_area.X + render_area.Width, render_area.Y - outline_thickness, outline_thickness, render_area.Height + 2 * outline_thickness);
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, up, Draw.Pixel.ClipRect, outline_color);
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, down, Draw.Pixel.ClipRect, outline_color);
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, left, Draw.Pixel.ClipRect, outline_color);
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, right, Draw.Pixel.ClipRect, outline_color);
        }
    }

}