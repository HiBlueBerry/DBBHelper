using System;
using System.Linq;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Text.RegularExpressions;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/AlignedText")]
    public class LeftAlignedText:Entity
    {
        //-----坐标轴纹理-----
        private MTexture Texture=new MTexture();

        //-----位置修正-----
        //文字对齐位置，(0,0)为左对齐，(1,0)为右对齐，(0.5,0.0)为居中对其
        Vector2 aligned_pos=Vector2.Zero;
        //坐标轴位置修正
        Vector2 axis_pos_adjustified=new Vector2(8.7f,8.0f)*0.2f;
        //坐标轴贴图缩放
        Vector2 axis_scale=new Vector2(0.2f,0.2f);

        //-----字符串以及其模式-----
        private string text_id="DBBHelper_AlignedText_Default";//存储的字符串id
        private string text="";//存储的字符串，这里存储的是包含\n的单行的字符串
        private List<string> text_for_show=new List<string>();//存储的最终要显示的字符串，这里是把text的所有\n替换后所得到的最终的字符串组
        private bool  EllipsisMode=false;//是否开启省略号模式
        private string AlignedStyle="Left";//对齐模式
        private float PaddingProportion=1.0f;//换行时相邻行的空白填充的倍率，默认值为1，此时上下两行的字基本相邻，越大代表相邻两行相距越远，反之则越近
        private float ParallaxProportion=0.0f;//视差比率，0的时候为无视差效果

        //-----字符属性-----
        private float text_scale = 1.0f;//字符缩放
        private float stroke_width=2.0f;//字符外轮廓的厚度
        private Color text_color=Color.White;//字符颜色
        private float text_color_alpha=1.0f;//字符颜色的透明度
        private Color stroke_color=Color.Black;//字符外轮廓颜色
        private float stroke_color_alpha=1.0f;//字符外轮廓颜色的透明度
        private float FadeStartDistance=0.0f;//文字开始消失时玩家与文字锚点的距离
        private float FadeEndDistance=128.0f;//文字结束消失时玩家与文字锚点的距离
        private string FadeStyle="Linear";//文字消失的模式
        private bool FadeSwitch=false;//文字是否应该在玩家远离时消失
        private float alpha_cofficient=1.0f;//基于玩家与实体的距离来控制alpha的值的系数
        private float tmp_alpha_cofficient = 1.0f;//用于场景切换时的渐入渐出
        
        //-----字符串属性-----
        private int text_max_num = 10;//允许显示的最大字符串数目，仅当省略号模式开启时生效
        //-----调试模式，用于指示锚点位置-----
        private bool DebugMode=true;

        //解析ParallaxProportion字符串
        private Vector2 ParseFloatString(string str)
        {
            string[] tmp_parallax = str.Split(",");
            List<float> tmp_float = new List<float>();
            Vector2 final_value = Vector2.Zero;
            float result = 0.0f;
            foreach (var tmp in tmp_parallax)
            {
                //如果是有效的浮点数，则加入到tmp_float中
                if (float.TryParse(tmp, out result))
                {
                    tmp_float.Add(result);
                }
            }
            if (tmp_float.Count == 1)
            {
                final_value = new Vector2(tmp_float[0], tmp_float[0]);
            }
            else if (tmp_float.Count >= 2)
            {
                final_value = new Vector2(tmp_float[0], tmp_float[1]);
            }
            return final_value;
        }
        public LeftAlignedText(EntityData data, Vector2 offset)
        {

            Tag = Tags.HUD;
            this.Position = data.Position + offset;
            //文字模式相关
            this.text_id = data.Attr("TextID");
            this.text_scale = 1.25f * data.Float("TextScale");
            this.stroke_width = data.Float("StrokeWidth");
            this.AlignedStyle = data.Attr("AlignedStyle");
            this.PaddingProportion = data.Float("PaddingProportion");

            this.ParallaxProportion=data.Float("ParallaxProportion");
            this.DebugMode = data.Bool("DebugMode");
            //文字省略相关
            this.EllipsisMode = data.Bool("EllipsisMode");
            this.text_max_num = data.Int("TextMaxNum");
            //文字颜色相关
            this.text_color = Calc.HexToColor(data.Attr("TextColor"));
            this.text_color_alpha = data.Float("TextAlpha");
            this.stroke_color = Calc.HexToColor(data.Attr("StrokeColor"));
            this.stroke_color_alpha = data.Float("StrokeAlpha");
            //文字消失相关
            this.FadeStartDistance = data.Float("FadeStartDistance");
            this.FadeEndDistance = data.Float("FadeEndDistance");
            this.FadeStyle = data.Attr("FadeStyle");
            this.FadeSwitch = data.Bool("FadeSwitch");
            //如果FadeStartDistance等于FadeEndDistance，那么禁用掉文字消失的功能
            if (FadeStartDistance == FadeEndDistance)
            {
                FadeSwitch = false;
                //Logger.Log(LogLevel.Warn,"DBBHelper/AlignedText","FadeStartDistance equals FadeEndDistance, fade has been disabled!");
            }
            //获取最终要显示的文字
            string[] array = GetCleanFromTextID(text_id);
            if (data.Int("TextLine") < array.Length)
            {
                text = array[data.Int("TextLine")];
            }
            else
            {
                text = "Error: Index out of the bounds of array!";
                text_color = Color.Red;
            }
            //如果省略号模式开启，则截断要显示的文字，超出的部分用...替换，否则直接删除超出的部分，注意，\n也会占用一个字符的位置
            if (EllipsisMode == true)
            {
                if (text.Length > text_max_num)
                {
                    text = text.Substring(0, text_max_num);
                    text += "...";
                }
            }
            else
            {
                if (text.Length > text_max_num)
                {
                    text = text.Substring(0, text_max_num);
                }
            }
            //获取最终要显示出来的字符串组
            text_for_show = new List<string>(text.Split(['\n']));
            //根据对齐方式来确定显示的内容和位置
            aligned_pos = Vector2.Zero;
            //坐标轴位置修正
            axis_pos_adjustified = new Vector2(8.7f, 8.0f) * 0.2f;
            //坐标轴贴图缩放
            axis_scale = new Vector2(0.2f, 0.2f);

            if (AlignedStyle == "Left")
            {
                Texture = GFX.Game["objects/DBB_Items/CoordinateAxis/AxisLeftAligned"];
            }
            else if (AlignedStyle == "Right")
            {
                Texture = GFX.Game["objects/DBB_Items/CoordinateAxis/AxisLeftAligned"];
                aligned_pos.X = 1.0f;
                axis_pos_adjustified.X = -axis_pos_adjustified.X;
                axis_scale.X = -axis_scale.X;
            }
            else if (AlignedStyle == "Middle")
            {
                Texture = GFX.Game["objects/DBB_Items/CoordinateAxis/AxisMiddleAligned"];
                axis_pos_adjustified = new Vector2(0, -4.0f) * 0.2f;
                aligned_pos = new Vector2(0.5f, 0.0f);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && FadeSwitch == true)
            {
                float distance = (player.Position - Position).Length();
                //前面已经处理了FadeEndDistance等于FadeStartDistance的情况了
                float t = 1.0f - (distance - FadeStartDistance) / (FadeEndDistance - FadeStartDistance);
                alpha_cofficient = DBBMath.MotionMapping(t, FadeStyle);
                tmp_alpha_cofficient = alpha_cofficient;
            }
            TransitionListener handle_alpha = new TransitionListener();
            handle_alpha.OnOut = delegate (float f)
            {
                float time = (float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(f, "easeInOutSin"), 1.0f, 0.0f);
                alpha_cofficient = tmp_alpha_cofficient * time;
            };
            Add(handle_alpha);
        }
        public override void Update()
        {
            base.Update();
            Player player=Scene.Tracker.GetEntity<Player>();
            //fade功能开启，同时玩家存在时才能开始
            if (player != null && FadeSwitch == true)
            {
                float distance = (player.Position - Position).Length();
                //前面已经处理了FadeEndDistance等于FadeStartDistance的情况了
                float t = 1.0f - (distance - FadeStartDistance) / (FadeEndDistance - FadeStartDistance);
                alpha_cofficient = DBBMath.MotionMapping(t, FadeStyle);
                tmp_alpha_cofficient = alpha_cofficient;
            }
             
        }
        
        public override void Render()
        {
            //确定锚点位置
            Vector2 position=Position-(Scene as Level).Camera.Position;
            Vector2 offset = (position - new Vector2(160f, 90f)) * ParallaxProportion;//这个是偏移量，用来做视差的，虽然我不知道这有什么好玩的，但是原版加了所以我也加了，乐
            Vector2 anchor_left_top = (position + offset) * 6.0f;
            //坐标轴位置
            Vector2 origin_pos=(position+axis_pos_adjustified)*6.0f;
            //临时参数，用于最终绘制用
            Vector2 tmp_axis_scale=axis_scale;
            Vector2 tmp_aligned_pos=aligned_pos;
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
            {
                origin_pos=(position+new Vector2(-axis_pos_adjustified.X,axis_pos_adjustified.Y))*6.0f;
                origin_pos.X=1920.0f-origin_pos.X;
                anchor_left_top.X=1920f-anchor_left_top.X;
                tmp_axis_scale.X=-tmp_axis_scale.X;
                if(AlignedStyle=="Left")
                {
                    tmp_aligned_pos.X=1.0f;
                }
                else if(AlignedStyle=="Right")
                {
                    tmp_aligned_pos.X=0.0f;
                }
            }
            //调试模式下绘制坐标轴(即锚点)的参考位置
            if(DebugMode==true)
            {
                Texture.DrawCentered(origin_pos,Color.White,tmp_axis_scale);
            }
            //Draw.SpriteBatch.Draw(Monocle.Draw.Pixel.Texture.Texture_Safe,new Rectangle((int)anchor_left_top.X,(int)anchor_left_top.Y,2,20),Monocle.Draw.Pixel.ClipRect,Color.Wheat);
            //绘制文字，逐行绘制文字，这里的0.8f是一个修正用的值，不要在意
            float tmp_padding=ActiveFont.LineHeight*text_scale*PaddingProportion*0.8f;

            for(int i=0;i<text_for_show.Count;i++)
            {
                ActiveFont.DrawOutline(text_for_show[i],anchor_left_top+i*new Vector2(0.0f,tmp_padding),tmp_aligned_pos,Vector2.One*text_scale,text_color*text_color_alpha*alpha_cofficient,stroke_width,stroke_color*stroke_color_alpha*alpha_cofficient);
            }

            
        }
        //正则表达式,用于匹配所有形如{...}的内容
        private static readonly Regex CommandRegex = new Regex(@"\{(.*?)\}", RegexOptions.Compiled);

        //对原字符串进行替换，将回车符换为\r，将{n}换为\n，同时移除其他{...}内容
        public string RemoveAllBreakFromString(string origin)
        {
            string tmp_string=origin;
            //如果有{}，则处理一下
            if (origin.Contains('{'))
            {
                tmp_string=CommandRegex.Replace(origin, match =>
                {
                    ReadOnlySpan<char> valueSpan=match.ValueSpan;
                    //如果是{n}或{break}，则保留它们
                    if (valueSpan.SequenceEqual("{n}".AsSpan()))
                    {
                        return "\n";
                    }
                    else if(valueSpan.SequenceEqual("{break}".AsSpan()))
                    {
                        return "\r";
                    }
                    else if(valueSpan.SequenceEqual("{space}".AsSpan()))
                    {
                        return " ";
                    }
                    //否则将{...}替换为空字符串
                    return "";
                });
            }
            
            return tmp_string;
        }
        //基于对话ID获取其所有行的内容
        public string[] GetCleanFromTextID(string text_id)
        {
           string tmp_string=RemoveAllBreakFromString(Dialog.Get(text_id));
           return tmp_string.Split(['\r'],StringSplitOptions.RemoveEmptyEntries);
        }
    }
}