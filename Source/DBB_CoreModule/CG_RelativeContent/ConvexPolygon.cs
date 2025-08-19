using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DBBHelper{
    public class ConvexPolygon {

        public List<Vector2> all_points=new List<Vector2>();
        public List<Vector2> all_normal=new List<Vector2>();
        public Vector2 center=new Vector2();

        /// <summary>
        /// <para>基于给定点列表points构造凸包，右手坐标系时默认按逆时针构造，此时center为凸多边形的形心</para>
        /// </summary>
        public ConvexPolygon(List<Vector2> points) 
        {
           all_points=ComputePoint(points);
           ComputeNormal();
        }

        /// <summary>
        /// <para>用于快速构造长方形，长方形以center为中心</para>
        /// <para>width，height依次为长方形的宽度和高度</para>
        /// <para>theta(角度制)为长方形的旋转角度，旋转以center为局部坐标系原点，右手坐标系时逆时针为正</para>
        /// </summary>
        public ConvexPolygon(Vector2 center,float width,float height,float theta)
        {
            ComputePoint(center,width,height,theta);    
            ComputeNormal();
        }
        /// <summary>
        /// <para>用于快速构造长方形，长方形以center为中心</para>
        /// <para>width，height依次为长方形的宽度和高度</para>
        /// </summary>
        public ConvexPolygon(Vector2 center,float width,float height)
        {
            ComputePoint(center,width,height);
            ComputeNormal();
        }
        public void UpdateConvexPolygon(Vector2 center,float width,float height,float theta)
        {
            ComputePoint(center,width,height,theta);    
            ComputeNormal();
        }
        public void UpdateConvexPolygon(Vector2 center,float width,float height)
        {
            ComputePoint(center,width,height);    
            ComputeNormal();
        }
        public void UpdateConvexPolygon(List<Vector2> points)
        {
            all_points=ComputePoint(points);
            ComputeNormal();
        }
        /// <summary>
        /// 使用分离轴定理进行多边形的相交测试，输入的点列表应为凸多边形的点位置
        /// </summary>
        public bool Intersect(List<Vector2>points)
        {
            //如果本身就没有点，失败
            if(all_points.Count==0)
            {
                return false;
            }
            //待检测图形没有点，失败
            if(points.Count==0)
            {
                return false;
            }
            //本身只有一个点
            if(all_points.Count==1)
            {
                //待检测也只有一个点，直接检查二者是否一致
                if(points.Count==1)
                {
                    if(all_points[0]==points[0])
                    {
                        return true;
                    }
                    return false;
                }
                //待检测有两个点，将本身按照待检测边进行投影，看是否落在[0,length]内
                else if(points.Count==2)
                {
                    Vector2 project=points[1]-points[0];
                    float t=Vector2.Dot(all_points[0]-points[0],project.SafeNormalize());
                    if(t>=0&&t<=project.Length())
                    {
                        return true;
                    }
                    return false;
                }
                //待检测有多个点，说明为多边形，按照待检测边逐个叉积
                else if(points.Count>2)
                {
                    int m=points.Count;
                    for(int i=1;i<m;i++)
                    {
                        if(DBBMath.Cross(points[(i+1)%m]-points[i],all_points[0]-points[i])<0.0f)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            //本身有多余1个点而待检测仅有一个点
            if(points.Count==1)
            {
                //本身有两个点，将待检测按照自身边进行投影，看是否落在[0,length]内
                if(all_points.Count==2)
                {
                    Vector2 project=all_points[1]-all_points[0];
                    float t=Vector2.Dot(points[0]-all_points[0],project.SafeNormalize());
                    if(t>=0&&t<=project.Length())
                    {
                        return true;
                    }
                    return false;
                }
                //本身有多个点，说明为多边形，按照自身边逐个叉积
                else if(all_points.Count>2)
                {
                    int m=all_points.Count;
                    for(int i=1;i<m;i++)
                    {
                        if(DBBMath.Cross(all_points[(i+1)%m]-all_points[i],points[0]-all_points[i])<0.0f)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            float x1_center=0.0f;//本多边形的投影中心坐标
            float x2_center=0.0f;//待检测多边形的投影中心坐标
            float x1_min=0.0f;//本多边形的投影最小坐标
            float x1_max=0.0f;//本多边形的投影最大坐标
            float x2_min=0.0f;//待检测多边形的投影最小坐标
            float x2_max=0.0f;//待检测多边形的投影最大坐标
            int all_counts_count=all_points.Count;
            int points_count=points.Count;
            Vector2 points_center=new Vector2();//待检测多边形的中心
            Vector2 n;//投影向量，需要归一化
            //获取待检测多边形的中心
            for(int i=0;i<points_count;i++)
            {
                points_center+=points[i];
            }
            points_center/=points_count;

            //以本多边形为主体去检测待检测多边形
            for(int i=0;i<all_counts_count;i++)
            {
                //初始化
                x1_min=0.0f;x1_max=0.0f;
                //获取该点对应的边以及其法线
                n=DBBMath.Rotate(all_points[i],all_points[(i+1)%all_counts_count],90)-all_points[i];
                n.Normalize();
                //以当前点为原点,n为投影轴,寻找多边形最大和最小的投影坐标
                for(int j=1;j<all_counts_count;j++)
                {
                    x1_min=Math.Min(x1_min,Vector2.Dot(all_points[(i+j)%all_counts_count]-all_points[i],n));
                    x1_max=Math.Max(x1_max,Vector2.Dot(all_points[(i+j)%all_counts_count]-all_points[i],n));
                }
                //初始化
                x2_min=Vector2.Dot(points[0]-all_points[i],n);
                x2_max=Vector2.Dot(points[0]-all_points[i],n);
                //以当前点为原点,n为投影轴,寻找待检测多边形最大和最小的投影坐标
                for(int k=1;k<points_count;k++)
                {
                    x2_min=Math.Min(x2_min,Vector2.Dot(points[k]-all_points[i],n));
                    x2_max=Math.Max(x2_max,Vector2.Dot(points[k]-all_points[i],n));

                }

                //获取本多边形中心和待检测多边形中心在投影轴上的坐标
                x1_center=Vector2.Dot(center-all_points[i],n);
                x2_center=Vector2.Dot(points_center-all_points[i],n);
                //如果投影中心之间的距离大于0.5倍的两个投影长度，则当前两个多边形二者分离
                if(Math.Abs(x1_center-x2_center)>0.5*(x1_max-x1_min)+0.5*(x2_max-x2_min))
                {
                    return false;
                }
            }
            //以待检测多边形为主体去检测本多边形
            for(int i=0;i<points_count;i++)
            {
                //初始化
                x2_min=0.0f;x2_max=0.0f;
                //获取该点对应的边以及其法线
                n=DBBMath.Rotate(points[i],points[(i+1)%points_count],90)-points[i];
                n.Normalize();
                //以当前点为原点,n为投影轴,寻找待检测多边形最大和最小的投影坐标
                for(int j=1;j<points_count;j++)
                {
                    x2_min=Math.Min(x1_min,Vector2.Dot(points[(i+j)%points_count]-points[i],n));
                    x2_max=Math.Max(x1_max,Vector2.Dot(points[(i+j)%points_count]-points[i],n));
                }
                //初始化
                x1_min=Vector2.Dot(all_points[0]-points[i],n);
                x1_max=Vector2.Dot(all_points[0]-points[i],n);
                //以当前点为原点,n为投影轴,寻找本多边形最大和最小的投影坐标
                for(int k=1;k<all_counts_count;k++)
                {
                    x1_min=Math.Min(x1_min,Vector2.Dot(all_points[k]-points[i],n));
                    x1_max=Math.Max(x1_max,Vector2.Dot(all_points[k]-points[i],n));

                }
                //获取本多边形中心和待检测多边形中心在投影轴上的坐标
                x1_center=Vector2.Dot(center-points[i],n);
                x2_center=Vector2.Dot(points_center-points[i],n);
                //如果投影中心之间的距离大于0.5倍的两个投影长度，则当前两个多边形二者分离
                if(Math.Abs(x1_center-x2_center)>0.5*(x1_max-x1_min)+0.5*(x2_max-x2_min))
                {
                    return false;
                }

            }
            return true;
            
        }
        
        
        //计算凸多边形各点
        private List<Vector2> ComputePoint(List<Vector2> points)
        {
            List<Vector2>tmp=points;
            List<Vector2>result=new List<Vector2>();
            int count=tmp.Count;
            List <int> index=new List<int>(new int[count+1]);
            center=Vector2.Zero;
            //Graham Scan形成凸包
            int temp=0;
            int total=0;
            //按照序关系对点排序，X最小的最小，X相同时Y最小的最小，排序后的第一个点肯定在凸包上
            tmp.Sort((a,b)=>
                {
                    int compare=a.X.CompareTo(b.X);
                    if(compare==0)
                    {
                        return a.Y.CompareTo(b.Y);
                    }
                    return compare;
                }
            );

            //处理没有点，一个点和两个点的特殊情况
            if(tmp.Count==0)
            {
                return result;
            }
            if(tmp.Count==1)
            {
                result.Add(tmp[0]);
                center=tmp[0];
                return result;
            }
            if(tmp.Count==2)
            {
                result.Add(tmp[0]);
                result.Add(tmp[1]);
                center=0.5f*(tmp[0]+tmp[1]);
                return result;
            }
            //下凸包
            for(int i=0;i<count;i++)
            {
                //迭代做叉积，如果凸性发生变化则回退已添加的凸包点，直到凸性正确，当然，回退到第一个点的时候就不要再回退了
                while(total>1 && DBBMath.Cross(tmp[index[total-1]]-tmp[index[total-2]],tmp[i]-tmp[index[total-1]])<=0)
                {
                    total--;
                }
                //此时凸性正确，把最新的待选点的索引加入index  
                index[total++]=i;
            }
            temp=total;
            //上凸包
            for(int i=count-2;i>=0;i--)
            {
                //迭代做叉积，如果凸性发生变化则回退已添加的凸包点，直到凸性正确，当然，回退到第temp个点的时候就不要再回退了
                while (total>temp && DBBMath.Cross(tmp[index[total-1]]-tmp[index[total-2]],tmp[i]-tmp[index[total-1]])<=0)
                {
                    total--;
                }
                //此时凸性正确，把最新的待选点的索引加入index
                index[total++]=i;
            }
            //把凸包点加进来
            for(int i=0;i<total-1;i++)
            {
                result.Add(tmp[index[i]]);
                center+=tmp[index[i]];
            }
            center=center/(total-1);
            return result;
        }
        //快速构建长方形
        private void ComputePoint(Vector2 center,float width,float height)
        {
            float W=Math.Abs(width);
            float H=Math.Abs(height);
            this.center=center;
            Vector2 bottom_left=new Vector2(-W/2.0f,-H/2.0f)+center;
            all_points=[bottom_left,bottom_left+new Vector2(W,0),bottom_left+new Vector2(W,H),bottom_left+new Vector2(0,H)];
        }
        //快速构建长方形
        private void ComputePoint(Vector2 center,float width,float height,float theta) 
        {
            float W=Math.Abs(width);
            float H=Math.Abs(height);
            this.center=center;
            Vector2 bottom_left=new Vector2(-W/2.0f,-H/2.0f)+center;
            all_points=[bottom_left,bottom_left+new Vector2(W,0),bottom_left+new Vector2(W,H),bottom_left+new Vector2(0,H)];
            for(int i=0;i<4;i++)
            {
                all_points[i]=DBBMath.Rotate(center,all_points[i],theta);
            }
        }
        //计算各边的法线
        private void ComputeNormal()
        {
            if(all_points.Count<=1)
            {
                return;
            }
            Vector2 edge=new Vector2();
            Vector2 normal=new Vector2();
            for(int i=0;i<all_points.Count-1;i++)
            {
                 edge=all_points[i+1]-all_points[i];
                 normal=DBBMath.Rotate(Vector2.Zero,edge,-90);
                 all_normal.Add(DBBMath.Normalize(normal));
            }
            if(all_points.Count==2)
            {
                return;
            }
            edge=all_points[0]-all_points[all_points.Count-1];
            normal=DBBMath.Rotate(Vector2.Zero,edge,-90);
            all_normal.Add(DBBMath.Normalize(normal));
        }
        
        
        
        /// <summary>
        /// <para>绘制凸多边形</para>
        /// <para>offset为偏移量，将对多边形的所有点加上offset后再绘制，这取决于你使用的是局部还是全局坐标系</para>
        /// <para>line_color为线段颜色，point_color为顶点的颜色</para>
        /// </summary>
        public void DrawPolygon(Vector2 offset,Color line_color,Color point_color)
        {
            //没有点就不要画了
            if(all_points.Count==0)
            {
                return;
            }
            //一个点就不要画线了
            if(all_points.Count==1)
            {
                Draw.Circle(offset+all_points[0],2.0f,point_color,4);
                return;
            }
            //两个点直接画一条线就行了
            if(all_points.Count==2)
            {
                Draw.Line(offset+all_points[0],offset+all_points[1],line_color);
                Draw.Circle(offset+all_points[0],2.0f,point_color,4);
                Draw.Circle(offset+all_points[1],2.0f,point_color,4);
                return;
            }
            for(int i=0;i<all_points.Count-1;i++)
            {
                Draw.Line(offset+all_points[i],offset+all_points[i+1],line_color);
                Draw.Circle(offset+all_points[i],2.0f,point_color,4);
            }
            Draw.Line(offset+all_points[all_points.Count-1],offset+all_points[0],line_color);
            Draw.Circle(offset+all_points[all_points.Count-1],2.0f,point_color,4);
        }
        /// <summary>
        /// <para>基于多边形的顶点位置直接绘制凸多边形</para>
        /// <para>line_color为线段颜色，point_color为顶点的颜色</para>
        /// </summary>
        public void DrawPolygon(Color line_color,Color point_color)
        {
            //没有点就不要画了
            if(all_points.Count==0)
            {
                return;
            }
            //一个点就不要画线了
            if(all_points.Count==1)
            {
                Draw.Circle(all_points[0],2.0f,point_color,4);
                return;
            }
            //两个点直接画一条线就行了
            if(all_points.Count==2)
            {
                Draw.Line(all_points[0],all_points[1],line_color);
                Draw.Circle(all_points[0],2.0f,point_color,4);
                Draw.Circle(all_points[1],2.0f,point_color,4);
                return;
            }
            for(int i=0;i<all_points.Count-1;i++)
            {
                Draw.Line(all_points[i],all_points[i+1],line_color);
                Draw.Circle(all_points[i],2.0f,point_color,4);
            }
            Draw.Line(all_points[all_points.Count-1],all_points[0],line_color);
            Draw.Circle(all_points[all_points.Count-1],2.0f,point_color,4);
        }
        /// <summary>
        /// <para>绘制凸多边形的法线，法线从边的中心外延</para>
        /// <para>offset为偏移量，将对多边形的所有法线加上offset后再绘制，这取决于你使用的是局部还是全局坐标系</para>
        /// <para>line_color为线段颜色</para>
        /// </summary>
        public void DrawNormal(Vector2 offset,Color line_color)
        {
            if(all_normal.Count==0)
            {
                return;
            }
            Vector2 edge_center=Vector2.Zero;
            for(int i=0;i<all_normal.Count-1;i++)
            {
                edge_center=(all_points[i]+all_points[i+1])*0.5f;
                Draw.Line(offset+edge_center,offset+edge_center+20.0f*all_normal[i],line_color);
            }
            if(all_normal.Count==1)
            {
                return;
            }
            edge_center=(all_points[all_normal.Count-1]+all_points[0])*0.5f;
            Draw.Line(offset+edge_center,offset+edge_center+20.0f*all_normal[all_normal.Count-1],line_color);
        }
        /// <summary>
        /// <para>直接绘制凸多边形的法线，法线从边的中心外延</para>
        /// <para>line_color为线段颜色</para>
        /// </summary>
        public void DrawNormal(Color line_color)
        {
            if(all_normal.Count==0)
            {
                return;
            }
            Vector2 edge_center=Vector2.Zero;
            for(int i=0;i<all_normal.Count-1;i++)
            {
                edge_center=(all_points[i]+all_points[i+1])*0.5f;
                Draw.Line(edge_center,edge_center+20.0f*all_normal[i],line_color);
            }
            if(all_normal.Count==1)
            {
                return;
            }
            edge_center=(all_points[all_normal.Count-1]+all_points[0])*0.5f;
            Draw.Line(edge_center,edge_center+20.0f*all_normal[all_normal.Count-1],line_color);
        }
        public void Log()
        {
            Logger.Log(LogLevel.Info,"DBBHelper","ConvexPolygon");
            for(int i=0;i<all_points.Count;i++)
            {
                Logger.Log(LogLevel.Info,"ConvexPolygon","Point["+i+"]: "+all_points[i].ToString());
            }
        }
    }
}