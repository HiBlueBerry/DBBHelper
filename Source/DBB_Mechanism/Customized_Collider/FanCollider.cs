using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.CustomizedCollider {

     public class FanCollider : Collider {

        public struct point_data
        {
            public float theta=0.0f;//点偏离轴的角度，逆时针计，以向上为正轴
            public float distance=0.0f;//点到中心的距离
            public point_data(float theta=0.0f,float distance=0.0f)
            {
                this.theta=theta;
                this.distance=distance;
            }

        }
        private float height=0.0f;//扇区宽度
        private float inner_radius=12.0f;//扇区内半径
        private float outer_radius=18.0f;//扇区外半径
        private float start_theta=0.0f;//扇区起始角度，弧度制
        private float end_theta=0.0f;//扇区终止角度，弧度制
        private float delta_theta=0.0f;//扇区角度，弧度制

        //用于存储近似多边形的点
        private List<Vector2> outer_polygon_point=new List<Vector2>();
        private List<Vector2> inner_polygon_point=new List<Vector2>();
        //用于存储近似凸多边形
        private List<ConvexPolygon> divided_polygon=new List<ConvexPolygon>();
        private int segment_number=8;//外半圈的划分点的数目,因此扇区矩形数目应为segment_number-1
        private float x_min,x_max,y_min,y_max;//用于存放扇区的近似矩形的最左下角和最右上角，用于粗相交测试
        
        /// <summary>
        /// <para>初始化扇环碰撞体</para>
        /// <para>Position为碰撞体的局部坐标系位置</para>
        /// <para>start_theta为扇区起始角度，delta_theta为扇区扫过的角度，delta_theta应不小于0不大于2，二者的单位均为PI，弧度制</para>
        /// <para>inner_radius为扇区内半径，outer_radius为扇区外半径</para>
        /// </summary>
        public FanCollider(Vector2 Position,float start_theta,float delta_theta,float inner_radius,float outer_radius,int segment_number=8) 
        {
            //外部接口
            this.Position = Position;

            this.start_theta=start_theta;
            this.delta_theta=delta_theta;
            this.end_theta=start_theta+delta_theta;

            this.inner_radius=Math.Min(Math.Abs(inner_radius),Math.Abs(outer_radius));
            this.outer_radius=Math.Max(Math.Abs(inner_radius),Math.Abs(outer_radius));
            height=Math.Abs(outer_radius-inner_radius);   

            this.segment_number=segment_number;
            //获取外部点和内部点
            for (int i=0;i<segment_number;i++)
            {
                float mid_theta=i*delta_theta/(segment_number-1)+start_theta;
                Vector2 mid_dir=new Vector2((float)Math.Cos(mid_theta),-(float)Math.Sin(mid_theta));
                outer_polygon_point.Add(mid_dir*outer_radius);
                inner_polygon_point.Add(mid_dir*inner_radius);
            }
            //切成多个凸多边形
            for(int i=0;i<segment_number-1;i++)
            {
                List<Vector2>tmp=[outer_polygon_point[i],outer_polygon_point[i+1],inner_polygon_point[i+1],inner_polygon_point[i]];
                
                divided_polygon.Add(new ConvexPolygon(tmp));
            }
            //获取粗相交测试用的数据
            x_min=Math.Min(outer_polygon_point[0].X,inner_polygon_point[0].X);
            x_max=Math.Max(outer_polygon_point[0].X,inner_polygon_point[0].X);
            y_min=Math.Min(outer_polygon_point[0].Y,inner_polygon_point[0].Y);
            y_max=Math.Max(outer_polygon_point[0].Y,inner_polygon_point[0].Y);
            for(int i=1;i<segment_number;i++)
            {
                x_min=Math.Min(x_min,Math.Min(outer_polygon_point[i].X,inner_polygon_point[i].X));
                x_max=Math.Max(x_max,Math.Max(outer_polygon_point[i].X,inner_polygon_point[i].X));
                y_min=Math.Min(y_min,Math.Min(outer_polygon_point[i].Y,inner_polygon_point[i].Y));
                y_max=Math.Max(y_max,Math.Max(outer_polygon_point[i].Y,inner_polygon_point[i].Y));
            }
        }
        /// <summary>
        /// 更新碰撞体局部坐标系位置（Position）和起始角度（start_theta），二者均为弧度制，单位均为PI
        /// </summary>
        public void Update(Vector2 Position,float start_theta)
        {

            //更新数据
            this.Position = Position;
            this.start_theta=start_theta;
            this.end_theta=start_theta+delta_theta; 
            //更新外部点和内部点
            for (int i=0;i<segment_number;i++)
            {
                float mid_theta=i*delta_theta/(segment_number-1)+start_theta;
                Vector2 mid_dir=new Vector2((float)Math.Cos(mid_theta),-(float)Math.Sin(mid_theta));
                outer_polygon_point[i]=mid_dir*outer_radius;
                inner_polygon_point[i]=mid_dir*inner_radius;
            }
            //切成多个凸多边形
            for(int i=0;i<segment_number-1;i++)
            {
                List<Vector2>tmp=[outer_polygon_point[i],outer_polygon_point[i+1],inner_polygon_point[i+1],inner_polygon_point[i]];
                divided_polygon[i].UpdateConvexPolygon(tmp);
            }
            //获取粗相交测试用的数据
            x_min=Math.Min(outer_polygon_point[0].X,inner_polygon_point[0].X);
            x_max=Math.Max(outer_polygon_point[0].X,inner_polygon_point[0].X);
            y_min=Math.Min(outer_polygon_point[0].Y,inner_polygon_point[0].Y);
            y_max=Math.Max(outer_polygon_point[0].Y,inner_polygon_point[0].Y);
            for(int i=1;i<segment_number;i++)
            {
                x_min=Math.Min(x_min,Math.Min(outer_polygon_point[i].X,inner_polygon_point[i].X));
                x_max=Math.Max(x_max,Math.Max(outer_polygon_point[i].X,inner_polygon_point[i].X));
                y_min=Math.Min(y_min,Math.Min(outer_polygon_point[i].Y,inner_polygon_point[i].Y));
                y_max=Math.Max(y_max,Math.Max(outer_polygon_point[i].Y,inner_polygon_point[i].Y));
            }
        } 
        //弃用
        public override float Width 
        { 
            get
            {
                return 2.0f*outer_radius;
            } 
            set
            {

            } 
        }
        public override float Height 
        { 
            get
            {
                return height;
            } 
            set
            {

            } 
        }
        public float InnerRadius
        {   get
            {
                return inner_radius;
            } 
            set
            {
                float tmp=Math.Abs(value);
                inner_radius=Math.Min(tmp,outer_radius);
                outer_radius=Math.Max(tmp,outer_radius);
                height=outer_radius-inner_radius;
            } 
        }
        public float OuterRadius
        { 
            get
            {
                return outer_radius;
            } 
            set
            {
                float tmp=Math.Abs(value);
                inner_radius=Math.Min(tmp,inner_radius);
                outer_radius=Math.Max(tmp,inner_radius);
                height=outer_radius-inner_radius;
            } 
        }
        public float StartTheta
        {
            get 
            { 
                return start_theta; 
            }
            set 
            { 

            }
        }
        public float EndTheta
        {
            get 
            { 
                return end_theta; 
            }
            set 
            { 

            }
        }
        public override float Left
        {
            get
            {
                return Position.X-outer_radius;
            }
            set
            {
                
            }
        }

        public override float Top
        {
            get
            {
                return Position.Y-outer_radius;
            }
            set
            {
                
            }
        }

        public override float Right
        {
            get
            {
                return Position.X+outer_radius;
            }
            set
            {
                
            }
        }

        public override float Bottom
        {
            get
            {
                return Position.Y+outer_radius;
            }
            set
            {
                
            }
        }
      
        public List<Vector2> OuterPolygonPoint
        {
            get{return outer_polygon_point;}
        }
        public List<Vector2> InnerPolygonPoint
        {
            get{return inner_polygon_point;}
        }
        /// <summary>
        /// 获取point在扇区的角度和距离信息
        /// </summary>
        /// <returns>point所在的角度(弧度制)，point到扇区参考点的距离</returns>
        public point_data GetPointData(Vector2 point)
        {
            Vector2 direction=point-AbsolutePosition;
            Vector2 ref_axis=new Vector2(1.0f,0.0f);
            float len=direction.Length();
            //获取角度的cos值
            direction.Y=-direction.Y;
            float v=Vector2.Dot(direction,ref_axis);
            float cos_value=v/len;
            float angle=(float)Math.Acos(cos_value);
            //angle为Nan时直接返回，移交给外部处理
            if(angle==float.NaN)
            {
                return new point_data(angle,len);
            }
            //Acos返回从0到PI的值，为此需要判断是0到PI还是从PI到2PI
            if(direction.Y<0)
            {
                angle=2.0f*(float)Math.PI-angle;
            }
            return new point_data(angle,len);
        }
        /// <summary>
        /// 粗相交检测，输入左下角和右上角的点坐标，该坐标应为相对坐标
        /// </summary>
        public bool QuickCheck(Vector2 bottom_left,Vector2 top_right)
        {
            float x1,y1,x2,y2,x3,y3,x4,y4;
            x1=x_min;y1=y_min;x2=x_max;y2=y_max;
            //确保输入的bottom_left和top_right的合法性
            x3=Math.Min(bottom_left.X,top_right.X);
            y3=Math.Min(bottom_left.Y,top_right.Y);
            x4=Math.Max(bottom_left.X,top_right.X);
            y4=Math.Max(bottom_left.Y,top_right.Y);
            if(Math.Max(x1,x3)<=Math.Min(x2,x4) && Math.Max(y1,y3)<=Math.Min(y2,y4))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 使用分离轴定理进行和矩形的相交测试，输入的坐标应为相对坐标，依次为左上角、左下角、右上角、右上角
        /// </summary>
        private bool CheckRect(Vector2 top_left,Vector2 bottom_left,Vector2 top_right,Vector2 bottom_right)
        {
            List<Vector2>points=[top_left,bottom_left,top_right,bottom_right];
            int failure=0;
            for(int i=0;i<divided_polygon.Count;i++)
            {
                if(!divided_polygon[i].Intersect(points))
                {
                    failure+=1;
                }
            }
            if(failure==segment_number-1)
            {
                return false;
            }
            return true;
        }
        public override bool Collide(Vector2 point)
        {
            //获取点数据
            point_data  data=GetPointData(point);
            float distance=data.distance;
            float angle=data.theta;

            //角度为Nan值的不要
            if(angle==float.NaN)
            {
                return false;
            }

            //距离处理，越界的不要
            if(distance<inner_radius||distance>outer_radius)
            {
                return false;
            }

            //角度处理，越界的不要
            if(angle>end_theta||angle<start_theta)
            {
                return false;
            }

            return true;
        }

        public override bool Collide(Rectangle rect)
        {
            Vector2 top_left=new Vector2(rect.X,rect.Y)-base.AbsolutePosition;
            Vector2 bottom_left=top_left+new Vector2(0,rect.Height);
            Vector2 top_right=top_left+new Vector2(rect.Width,0);
            Vector2 bottom_right=bottom_left+new Vector2(rect.Width,0);
            //先进行粗相交检测，如果检测通过则进行SAT检测
            if(QuickCheck(bottom_left,top_right))
            {
                return CheckRect(top_left,bottom_left,top_right,bottom_right);
            }
            return false;
            
        }

        public override bool Collide(Vector2 from, Vector2 to)
        {
            point_data p1=GetPointData(from);
            point_data p2=GetPointData(to);
            //角度为Nan值的不要
            if(p1.theta==float.NaN||p2.theta==float.NaN)
            {
                return false;
            }
            
            //距离测试，越界的不要
            float rect_inner_radius=Math.Min(p1.distance,p2.distance);
            float rect_outer_radius=Math.Max(p1.distance, p2.distance);
            if(rect_inner_radius>outer_radius||rect_outer_radius<inner_radius)
            {
                return false;
            }

            //角度测试，越界的不要
            float rect_start_angle=Math.Min(p1.theta,p2.theta);
            float rect_end_angle=Math.Max(p1.theta,p2.theta);
            if(rect_start_angle>end_theta||rect_end_angle<start_theta)
            {
                return false;
            }

            return true;
        }

        public override bool Collide(Hitbox hitbox)
        {
            Vector2 top_left=hitbox.AbsolutePosition-base.AbsolutePosition;
            Vector2 top_right=top_left+new Vector2(hitbox.width,0.0f);
            Vector2 bottom_left=top_left+new Vector2(0.0f,hitbox.height);
            Vector2 bottom_right=bottom_left+new Vector2(hitbox.width,0.0f);
            //先进行粗相交检测，如果检测通过则进行SAT检测
            if(QuickCheck(bottom_left,top_right))
            {   
                return CheckRect(top_left,bottom_left,top_right,bottom_right);
            }
            return false;
        }

        public override bool Collide(Grid grid)
        {
            return false;
        }

        public override bool Collide(Circle circle)
        {
            return false;
        }

        public override bool Collide(ColliderList list)
        {
            for(int i=0;i<list.colliders.Length;i++)
            {
                if(SpecialCollide(list.colliders[i]))
                {
                    return true;
                }
            }
            return false;
        }

        //外部接口，当collider1调用Collide时，如果collider2是FanCollider，会被定向到collider2.SpecialCollide这里来，这意味着这里要实现对各种原版不存在的Collider的处理
        public bool SpecialCollide(Collider collider)
        {
            //角度范围为0时直接测试失败
            if(start_theta==end_theta)
            {
                return false;
            }
            if (collider is Hitbox)
            {
                return Collide(collider as Hitbox);
            }

            if (collider is Grid)
            {
                return Collide(collider as Grid);
            }

            if (collider is ColliderList)
            {
                return Collide(collider as ColliderList);
            }

            if (collider is Circle)
            {
                return Collide(collider as Circle);
            }
            if(collider is FanCollider)
            {
                return false;
            }
            return false;
        }
        public override Collider Clone()
        {
            return new FanCollider(this.Position,this.start_theta,this.end_theta,this.inner_radius,this.outer_radius);
        }

        //Debug用的
        public override void Render(Camera camera, Color color)
        {
            Vector2 n1=new Vector2((float)Math.Cos(start_theta),-(float)Math.Sin(start_theta));
            Vector2 n2=new Vector2((float)Math.Cos(end_theta),-(float)Math.Sin(end_theta));
            for(int i=0;i<divided_polygon.Count;i++)
            {
                divided_polygon[i].DrawPolygon(base.AbsolutePosition,Color.GreenYellow,Color.Red);
            }
            DrawFan(color,16);
            Draw.Line(base.AbsolutePosition,base.AbsolutePosition+n1*outer_radius,Color.DarkGray);
            Draw.Line(base.AbsolutePosition,base.AbsolutePosition+n2*outer_radius,Color.White);
            //绘制粗相交碰撞盒
            Draw.Line(base.AbsolutePosition+new Vector2(x_min,y_min),base.AbsolutePosition+new Vector2(x_min,y_max),Color.Purple);
            Draw.Line(base.AbsolutePosition+new Vector2(x_min,y_max),base.AbsolutePosition+new Vector2(x_max,y_max),Color.Purple);
            Draw.Line(base.AbsolutePosition+new Vector2(x_max,y_max),base.AbsolutePosition+new Vector2(x_max,y_min),Color.Purple);
            Draw.Line(base.AbsolutePosition+new Vector2(x_max,y_min),base.AbsolutePosition+new Vector2(x_min,y_min),Color.Purple);
        }
        //给用户用的
        public void Render(Color line_color,Color point_color)
        {
            Vector2 n1=new Vector2((float)Math.Cos(start_theta),-(float)Math.Sin(start_theta));
            Vector2 n2=new Vector2((float)Math.Cos(end_theta),-(float)Math.Sin(end_theta));
            for(int i=0;i<divided_polygon.Count;i++)
            {
                divided_polygon[i].DrawPolygon(base.AbsolutePosition,line_color,point_color);
                
            }
            Draw.Line(base.AbsolutePosition,base.AbsolutePosition+n1*outer_radius,Color.RoyalBlue);
            Draw.Line(base.AbsolutePosition,base.AbsolutePosition+n2*outer_radius,Color.SkyBlue);
 
        }
        //绘制扇形区域
        private void DrawFan(Color color,int resolution)
        {
            Vector2 start_dir=new Vector2((float)Math.Cos(start_theta),-(float)Math.Sin(start_theta));
            float mid_theta;
            for (int i = 1; i <= resolution; i++)
            {
                mid_theta=i*(end_theta-start_theta)/resolution+start_theta;
                Vector2 mid_dir=new Vector2((float)Math.Cos(mid_theta),-(float)Math.Sin(mid_theta));
                Draw.Line(base.AbsolutePosition + start_dir*inner_radius, base.AbsolutePosition + mid_dir*inner_radius, color);
                Draw.Line(base.AbsolutePosition + start_dir*outer_radius, base.AbsolutePosition + mid_dir*outer_radius, color);
                start_dir=mid_dir;
            }
        }
     }

}