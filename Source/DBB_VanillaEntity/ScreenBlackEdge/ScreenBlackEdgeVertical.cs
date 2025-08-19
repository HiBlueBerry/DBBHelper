using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/ScreenBlackEdgeVertical")]
    public class ScreenBlackEdgeVertical:Entity
    {
//---------------------从Loenn传来的参数---------------------
        private float proportion=2.35f;//长宽比，用于计算黑边的参考高度
        private Color color=Color.Black;//边框颜色
        private string style_in="easeInOutSin";//边框的渐入方式
        private string style_out="easeInOutSin";//边框的渐退方式
        
//---------------------用于绘制边框的属性---------------------
        private bool first_frame=true;//避免第一帧的绘制，否则由没有该实体的场景进入有该实体的场景时会有一帧的闪烁
        private float lerp=0.0f;//用于插值
        private float edge_length=0.0f;//黑边的参考宽度
        private float right_x=0.0f;//右侧黑边的参考X坐标
        private float draw_width=0.0f;//左侧黑边实际要绘制的黑边的宽度
        private float draw_right_x=0.0f;//实际要绘制的右侧黑边的X坐标

        public ScreenBlackEdgeVertical(EntityData data,Vector2 offset)
        {
            //一些基本属性
            Position=data.Position+offset;
            proportion=data.Float("Proportion");
            color=Calc.HexToColor(data.Attr("Color"));
            style_in=data.Attr("InStyle");
            style_out=data.Attr("OutStyle");
            //计算绘制边框的属性
            float length=1080.0f*proportion;
            edge_length=(1924.0f-length)*0.5f;
            right_x=edge_length+length;
            //初始时应该默认已经绘制完边框
            draw_width=edge_length;
            draw_right_x=right_x;
        }
        public override void Added(Scene scene)
        {
            List<Entity>temp=scene.Entities.ToAdd;
            //如果场景中已经有ScreenBlackEdgeVertical实例了就不要让这个实例生效了
            foreach(var entity in temp)
            {
                if(entity is ScreenBlackEdgeVertical)
                {  
                    if(!entity.Equals(this))
                    {
                        Visible=false;
                    }
                    break;
                }
            }   
            //把它放在UI层
            Tag=TagsExt.SubHUD;
            base.Added(scene);
            TransitionListener val=new TransitionListener();
            //出场时应渐退
            val.OnOut=delegate(float f)
            {
               lerp=DBBMath.MotionMapping(f,style_out);
               draw_right_x=(float)DBBMath.Linear_Lerp(lerp,right_x,1926.0f);
               draw_width=(float)DBBMath.Linear_Lerp(lerp,edge_length,1.0f);

            };
            //入场时应渐入
            val.OnIn=delegate(float f)
            {
                lerp=DBBMath.MotionMapping(f,style_in);
                draw_right_x=(float)DBBMath.Linear_Lerp(lerp,1926.0f,right_x);
                draw_width=(float)DBBMath.Linear_Lerp(lerp,1.0f,edge_length);
            };
            Add(val);
            
        }
        public override void Render()
        {
            base.Render();
            if(first_frame==true)
            {
                first_frame=false;
                return;
            }
            Draw.Rect(-2.0f,-2.0f,draw_width,1086.0f,color);
            Draw.Rect(draw_right_x,-2.0f,edge_length+8.0f,1086.0f,color);
        }

    }
}
