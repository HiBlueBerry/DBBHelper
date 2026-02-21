using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.DBBHelper.UI.OUI;
using Celeste.Mod.UI;
namespace Celeste.Mod.DBBHelper.UI.UIComponent
{
    public class DBBCustomUIComponent
    {
        
        public class ExtendedSlider : TextMenu.Slider
        {
            private MTexture warn_label = GFX.Game["objects/DBB_Items/UI/warn"];
            public Func<bool> event_handle;
            public Func<bool> disable_handle;
            public bool event_handle_message = false;
            public string event_handle_label = "ModOptions_DBBHelper_General_EventHandleLabel";
            public int default_value = 0;
            public bool isDefault = true;
            public Color UnselectedHightlightColor = Color.Goldenrod;
            public Color EventHightlightedColor = Color.OrangeRed;
            /// <summary>
            /// 扩展的滑块组件
            /// <para name="label">label：Dialog中的标签，指定该组件的标题的显示名称</para>
            /// <para name="values">values：一个委托，根据索引来返回对应的要显示的字符串值</para>
            /// <para name="min">min：索引的最小值</para>
            /// <para name="max">max：索引的最大值</para>
            /// <para name="UnselectedHightlightColor">UnselectedHightlightColor：当当前值不等于默认值的时候显示的字体颜色</para>
            /// <para name="EventHightlightedColor">EventHightlightedColor：当该组件的事件发生时显示的字体颜色</para>
            /// <para name="default_value">default_value：索引的默认值</para>
            /// <para name="previous_value">previous_value：上一次构建该组件后经过各种操作所得到的索引值，使用该属性是由于每次打开全局设置时都会重新构造一次该组件</para>
            /// <para name="event_handle">event_handle：该组件的事件，对应于EventHightlightedColor</para>
            /// <para name="event_handle_label">event_handle_label：该组件的事件发生后额外要显示的文本的Dialog中的标签</para>
            /// <para name="disable_label">disable_label：用于控制该组件禁用和启用的事件</para>
            /// </summary>
            public ExtendedSlider(
                string label,
                Func<int, string> values,
                int min,
                int max,
                Color UnselectedHightlightColor,
                Color EventHightlightedColor,
                int default_value = -1,
                int previous_value = -1,
                Func<bool> event_handle = null,
                string event_handle_label = "ModOptions_DBBHelper_General_EventHandleLabel",
                Func<bool> disable_handle = null
            ) : base(label, values, min, max, previous_value)
            {

                this.UnselectedHightlightColor = UnselectedHightlightColor;
                this.EventHightlightedColor = EventHightlightedColor;
                this.event_handle_label = event_handle_label;
                this.event_handle = event_handle;
                this.disable_handle = disable_handle;
                //如果事件不为空，则运行它
                if (event_handle != null)
                {
                    event_handle_message = event_handle.Invoke();
                }
                this.default_value = default_value < 0 ? 0 : default_value - min;
                isDefault = true;
            }
            public override float Height()
            {
                float extend_height = event_handle_message ? warn_label.Height : 0.0f;
                return ActiveFont.LineHeight + extend_height;
            }
            public override void Update()
            {
                base.Update();
                //如果事件不为空，则运行它
                if (disable_handle != null)
                {
                    Disabled = disable_handle.Invoke();
                }
                //如果事件不为空，则运行它
                if (event_handle != null)
                {
                    event_handle_message = event_handle.Invoke();
                }
            }
            public override void Render(Vector2 position, bool highlighted)
            {
                float alpha = Container.Alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                isDefault = (Index == default_value) ? true : false;
                //justify为(0,0)时为左上角锚点，为(0,0.5)时为左侧中点为锚点，为(0.5,0.5)时为中间锚点，为(1,0.5)时为右侧中点为锚点
                Color color = Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : (isDefault ? UnselectedColor : UnselectedHightlightColor)) * alpha);
                //居中绘制标签字体
                ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, strokeColor);
                //Draw.Rect(position, 16.0f, 16.0f, Color.Red);//这个是这个item真正的左上角锚点
                //Draw.Rect(position + new Vector2(0.0f, ActiveFont.LineHeight), 16.0f, 16.0f, Color.DeepSkyBlue);//这个用来确定字体高度
                //如果关卡内控制器接管参数，则显示警告
                if (event_handle_message == true)
                {
                    warn_label.Draw(position + new Vector2(0, 0.5f * ActiveFont.LineHeight));
                    ActiveFont.DrawOutline(Dialog.Clean(event_handle_label), position + new Vector2(warn_label.Width * 1.1f, 0.5f * ActiveFont.LineHeight), new Vector2(0f, 0.1f), new Vector2(0.6f, 0.6f), EventHightlightedColor, 2f, strokeColor);
                }
                //这个是右侧的选项
                if (Values.Count > 0)
                {
                    float num = RightWidth();
                    ActiveFont.DrawOutline(Values[Index].Item1, position + new Vector2(Container.Width - num * 0.5f + (float)lastDir * ValueWiggler.Value * 8f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);
                    Vector2 vector = Vector2.UnitX * (highlighted ? ((float)Math.Sin(sine * 4f) * 4f) : 0f);
                    bool flag = Index > 0;
                    Color color2 = flag ? color : (Color.DarkSlateGray * alpha);
                    Vector2 position2 = position + new Vector2(Container.Width - num + 40f + ((lastDir < 0) ? ((0f - ValueWiggler.Value) * 8f) : 0f), 0f) - (flag ? vector : Vector2.Zero);
                    ActiveFont.DrawOutline("<", position2, new Vector2(0.5f, 0.5f), Vector2.One, color2, 2f, strokeColor);
                    bool flag2 = Index < Values.Count - 1;
                    Color color3 = flag2 ? color : (Color.DarkSlateGray * alpha);
                    Vector2 position3 = position + new Vector2(Container.Width - 40f + ((lastDir > 0) ? (ValueWiggler.Value * 8f) : 0f), 0f) + (flag2 ? vector : Vector2.Zero);
                    ActiveFont.DrawOutline(">", position3, new Vector2(0.5f, 0.5f), Vector2.One, color3, 2f, strokeColor);
                }
            }
        }

        //我认为这是一个非常愚蠢的事情
        public class ExtendedDescription : TextMenuExt.SubHeaderExt
        {
            private float current_alpha;
            public bool FadeVisible { get; set; } = true;
            public float offset = 0.0f;
            public TextMenu.Item item;
            /// <summary>
            /// 扩展的描述组件
            /// <para name="item">item：将该描述组件绑定到对应的组件item上</para>
            /// <para name="description_label">description_label：Dialog中的标签，指定描述的显示名称</para>
            /// <para name="initial_visible">initial_visible：指定初始化时是否可见</para>
            /// <para name="icon">icon：Dialog中的标签，指定图标的路径</para>
            /// <para name="offset">offset:在打开描述时文字形成位置的偏移量，0.0时为对应item的左上角锚点的Y位置</para>
            /// </summary>
            public ExtendedDescription(TextMenu.Item item, string description_label, bool initial_visible, string icon = null, float offset = 0.0f) : base(description_label, icon)
            {
                string description = description_label.DialogCleanOrNull() ?? description_label;
                //这里重新改一下title的内容
                base.Title = description;
                //如果初始化时可见，则alpha直接设置为1，视觉效果上是突然显示，否则则alpha设置为0，视觉效果上是缓慢出现
                FadeVisible = initial_visible;
                Alpha = FadeVisible ? 1 : 0;
                current_alpha = Alpha;
                this.offset = offset;
                //绑定对应的item
                this.item = item;
                if (this.item != null)
                {
                    this.item.OnEnter += () => { this.FadeVisible = true; };
                    this.item.OnLeave += () => { this.FadeVisible = false; };
                }

            }
            public override float Height()
            {
                //offset是在打开注释时文字形成位置的偏移量，打开注释时文字是从item.position+Vector2(0,offset)处形成，然后再移动到item.position+Vector2(0,base.Height)
                return MathHelper.Lerp(offset, base.Height(), Alpha);
            }

            public override void Update()
            {
                base.Update();
                //确认最终的目标alpha是多少
                float target_alpha = FadeVisible ? 1 : 0;
                //根据目标alpha，让当前的alpha去逼近它
                if (current_alpha != target_alpha)
                {
                    //以这个速度去逼近
                    current_alpha = Calc.Approach(current_alpha, target_alpha, Engine.RawDeltaTime * 3f);
                    //根据是否可见来确定是渐入过程还是渐出过程
                    if (FadeVisible)
                    {
                        Alpha = Ease.SineOut(current_alpha);
                    }
                    else
                    {
                        Alpha = Ease.SineIn(current_alpha);
                    }
                }
                //当Alpha为0时设置为不可见
                Visible = Alpha != 0f;
            }
        }
        //占位用的组件，只用于显示空白行
        public class PlaceholderItem : TextMenu.Item
        {
            public int blank_num = 1;//占位行数
            public Vector2 scale = new Vector2(1.0f, 0.5f);
            public PlaceholderItem(int blank_num = 1, float scale = 0.5f)
            {
                this.blank_num = blank_num;
                Selectable = false;
                this.scale = new Vector2(1.0f, scale);
            }
            
            public override float LeftWidth()
            {
                return 32.0f;
            }
            //返回该项右侧内容的宽度
            public override float RightWidth()
            {
                return 120f;
            }
            //返回该项所占据的高度
            public override float Height()
            {
                return ActiveFont.LineHeight * scale.Y * blank_num;
            }
            //渲染
            public override void Render(Vector2 position, bool highlighted)
            {
                //绘制空白行
                string blank = string.Concat(Enumerable.Repeat("\n", blank_num));
                ActiveFont.DrawOutline(blank, position, new Vector2(0.0f, 0.5f), scale, Color.White, 2f, Color.Black);
            }
        }
        public class ColorPicker : TextMenu.Item
        {
            private MTexture warn_label = GFX.Game["objects/DBB_Items/UI/warn"];

            public Func<bool> event_handle = null;
            public Func<bool> disable_handle = null;
            public bool event_handle_message = false;
            public string event_handle_label = "ModOptions_DBBHelper_General_EventHandleLabel";


            public string Label;//该项设置在文本文件中对应的标签，文本文件中该标签所对应的内容将会被展示
            public Vector4 CurrentValue;//当前的归一化颜色值
            private Vector4 PreviousValue;//之前的归一化颜色值，这个用于判断值是否发生变化
            public Vector4 DefaultValue;//颜色的默认值
            public Action<Vector4> OnValueChange;//值改变时的回调函数
            public Color UnselectedColor = Color.White;
            public Color UnselectedHightlightColor = Color.Goldenrod;
            public Color EventHightlightedColor = Color.OrangeRed;

            private Vector2 colorRender_offset;
            public string current_colorString;
            /// <summary>
            /// 拾色器组件
            /// <para name="label">label：Dialog中的标签，指定该组件的标题的显示名称</para>
            /// <para name="UnselectedHightlightColor">UnselectedHightlightColor：当当前值不等于默认值的时候显示的字体颜色</para>
            /// <para name="EventHightlightedColor">EventHightlightedColor：当该组件的事件发生时显示的字体颜色</para>
            /// <para name="default_value">default_value：拾色器颜色值的默认值</para>
            /// <para name="previous_value">previous_value：上一次构建该组件后经过各种操作所得到的颜色值，使用该属性是由于每次打开全局设置时都会重新构造一次该组件</para>
            /// <para name="event_handle">event_handle：该组件的事件，对应于EventHightlightedColor</para>
            /// <para name="event_handle_label">event_handle_label：该组件的事件发生后额外要显示的文本的Dialog中的标签</para>
            /// <para name="disable_label">disable_label：用于控制该组件禁用和启用的事件</para>
            /// </summary>
            public ColorPicker(string label, Color UnselectedHightlightColor, Color EventHightlightedColor, Vector4 previous_value, Vector4 default_value, Func<bool> event_handle = null, string event_handle_label = "ModOptions_DBBHelper_General_EventHandleLabel", Func<bool> disable_handle = null)
            {
                //初始化一些通用设置
                Label = label;
                this.UnselectedHightlightColor = UnselectedHightlightColor;
                this.EventHightlightedColor = EventHightlightedColor;
                CurrentValue = DBBMath.Clamp(previous_value, Vector4.Zero, Vector4.One);
                DefaultValue = DBBMath.Clamp(default_value, Vector4.Zero, Vector4.One);
                PreviousValue = CurrentValue;
                //提供一个自定义事件，每次都运行，运行后获得event_handle_message的值，如果为真则触发event_handle_label的显示
                this.event_handle = event_handle;
                this.event_handle_label = event_handle_label;
                event_handle_message = false;
                this.disable_handle = disable_handle;
                //如果事件不为空，则先运行一下它
                if (event_handle != null)
                {
                    event_handle_message = event_handle.Invoke();
                }
                //它是可被选择的
                Selectable = true;
                //计算小偏移，这是为渲染准备的
                float stringRender_offset = 0.0f;
                string test_string = "0123456789ABCDEF";
                foreach (var test_char in test_string)
                {
                    stringRender_offset = Math.Max(stringRender_offset, ActiveFont.Measure(test_char).Y);
                }
                colorRender_offset = new Vector2(ActiveFont.Measure(">").X, stringRender_offset);
            }
            //当该项被添加到菜单中时会触发Added
            public override void Added()
            {
                //该项需要被双列显示
                Container.InnerContent = TextMenu.InnerContentMode.TwoColumn;
            }

            //当键盘确认键按下时应当执行的操作，这里确认键一般为C
            public override void ConfirmPressed()
            {
                //在游戏内不执行UI跳转
                bool inGame = Engine.Scene is Level;
                if (inGame == true)
                {
                    return;
                }
                Audio.Play("event:/ui/main/button_back");
                var color_picker = OuiModOptions.Instance.Overworld.Goto<DBB_ColorPicker_OUI>();
                color_picker.Init<OuiModOptions>
                (
                    Label,
                    current_colorString,
                    new Tuple<string, string>("ModOptions_DBBHelper_ColorCorrection_ColorPicker_HexColor_InputTip_DefaultMode_Label", "ModOptions_DBBHelper_ColorCorrection_ColorPicker_HexColor_InputTip_EditMode_Label"),
                    new Tuple<string, string>("ModOptions_DBBHelper_ColorCorrection_ColorPicker_ColorPickerArea_InputTip_DefaultMode_Label", "ModOptions_DBBHelper_ColorCorrection_ColorPicker_ColorPickerArea_InputTip_EditMode_Label"),
                    new Tuple<string, string>("ModOptions_DBBHelper_ColorCorrection_ColorPicker_HueBar_InputTip_DefaultMode_Label", "ModOptions_DBBHelper_ColorCorrection_ColorPicker_HueBar_InputTip_EditMode_Label"),
                    new Tuple<string, string>("ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InputTip_DefaultMode_Label", "ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InputTip_EditMode_Label"),
                    new Tuple<string, string>("ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InvalidInputTip_RGB_Label", "ModOptions_DBBHelper_ColorCorrection_ColorPicker_RGB_and_HSV_InvalidInputTip_HSV_Label"),
                    new Tuple<float, float>(1.0f, 1.0f),
                    delegate ()
                    {
                        PreviousValue = CurrentValue;
                        current_colorString = color_picker.HexColor_For_Show;
                        CurrentValue = DBBMath.ConvertColor(current_colorString);
                        //如果值发生了变化，则执行OnValueChange同时覆盖先前值
                        if (CurrentValue != PreviousValue)
                        {
                            PreviousValue = CurrentValue;
                            OnValueChange?.Invoke(CurrentValue);
                        }
                    }
                );

            }
            //该项通过不断调用Update来实现更新
            public override void Update()
            {
                current_colorString = DBBMath.ConvertColor(CurrentValue, false);
                //如果值发生了变化，则执行OnValueChange同时覆盖先前值
                if (CurrentValue != PreviousValue)
                {
                    PreviousValue = CurrentValue;
                    OnValueChange?.Invoke(CurrentValue);
                }
                if (disable_handle != null)
                {
                    Disabled = disable_handle.Invoke();
                }
            }

            //返回该项左侧内容的宽度，这个应该在两列展示时有用处
            //例如，通常的设置项类似：<设置名称>：<设置值>
            //这个测量的是<设置名称>所占用的宽度
            public override float LeftWidth()
            {
                return ActiveFont.Measure(Label).X + 32.0f;
            }
            //返回该项右侧内容的宽度
            public override float RightWidth()
            {
                return ActiveFont.Measure(current_colorString).X + 120f;
            }
            //返回该项所占据的高度
            public override float Height()
            {
                float extend_height = event_handle_message ? warn_label.Height : 0.0f;
                return ActiveFont.LineHeight + extend_height;
            }
            //渲染一些东西
            public override void Render(Vector2 position, bool highlighted)
            {
                base.Render(position, highlighted);
                float alpha = Container.Alpha;
                Color strokeColor = Color.Black * (alpha * alpha * alpha);
                Color color = Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : (CurrentValue == Vector4.One) ? UnselectedColor : UnselectedHightlightColor) * alpha);
                ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, strokeColor);

                //一个小的颜色展示区域，叠加两层，底层大一些，顶层小一些用于构成边框的效果
                Color current_color = new Color(CurrentValue);
                Vector2 color_pos = position + new Vector2(Container.Width - 40.0f - colorRender_offset.X * 0.5f, -colorRender_offset.Y * 0.2f);
                Rectangle knobOuterRectangle = new Rectangle((int)color_pos.X, (int)color_pos.Y, 30, 30);
                Rectangle knobInnerRectangle = new Rectangle((int)color_pos.X + 3, (int)color_pos.Y + 3, 24, 24);
                Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, knobOuterRectangle, Draw.Pixel.ClipRect, Color.White);//绘制小方块外部
                Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, knobInnerRectangle, Draw.Pixel.ClipRect, current_color);//绘制小方块内部
                //绘制颜色字符串
                Vector2 string_pos = position + new Vector2(Container.Width - 60.0f, 0.0f);
                ActiveFont.DrawOutline(current_colorString, string_pos, new Vector2(1.0f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);
                if (event_handle_message == true)
                {
                    warn_label.Draw(position + new Vector2(0, 0.5f * ActiveFont.LineHeight));
                    ActiveFont.DrawOutline(Dialog.Clean(event_handle_label), position + new Vector2(warn_label.Width * 1.1f, 0.5f * ActiveFont.LineHeight), new Vector2(0f, 0.1f), new Vector2(0.6f, 0.6f), EventHightlightedColor, 2f, strokeColor);
                }
            }
            //感觉是无用小函数
            public override string SearchLabel()
            {
                return Label;
            }
            /// <summary>
            /// Set the action that will be invoked when the value changes.
            /// </summary>
            public ColorPicker Change(Action<Vector4> do_something_on_value_change)
            {
                OnValueChange = do_something_on_value_change;
                return this;
            }

        }
    }
}