using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities 
{
    class CloudZipperPath:Entity{
        private MTexture m_cloudOutline;//云轮廓纹理，用于可视化终止位置
        private List<Vector2>m_node_pos=new List<Vector2>();//云各个节点的位置
        private List<Vector2> m_direction=new List<Vector2>();//云移动方向
        private List<float> m_distance=new List<float>();//云轨迹线长度
        private Vector2 m_percent=new Vector2(0,0);//处理轨迹绘制进度用
        private int m_path_index=0;//处理轨迹绘制进度用
        private bool b_is_back=false;//处理轨迹绘制进度用
        private float m_respawnTimer;//处理轨迹绘制进度用
        private float m_respawnTime;//处理轨迹绘制进度用
    //Bug处理：碎片云的轨迹绘制方式从基于距离转为基于重生时间时，有一帧会出现轨迹绘制错误的使用了基于距离的方式
    //这会造成一帧的闪烁
    //造成这种情况条件应为：云的m_respawnTimer<0.0f且云的b_fragile_pathinit==true
        private bool b_fragile_pathinit=false;
        private bool b_fragile=false;//是否为碎片云轨迹
        private bool b_small=false;//是否为小云轨迹
        private Color m_ropeColor=Color.Aqua;//轨迹颜色
        private List<int> m_trail_point_num=new List<int>();//云的轨迹点绘制数目
        private float m_trail_point_bloomRadius=12.0f;//轨迹点泛光半径
        private int m_trail_point_LightStartFade=12;//轨迹点点光源衰弱起始半径
        private int m_trail_point_LightEndFade=16;//轨迹点点光源衰弱结束半径
        private List<Vector2[]> m_points=new List<Vector2[]>();//轨迹线绘制点
        private List<float[]> m_lastper=new List<float[]>();//记录轨迹绘制点的上一次percent值，这用于碎片云的轨迹绘制
        private List<float[]> m_lastper_for_fragile;
        private List<float> m_lastper_outline=new List<float>();//记录轮廓上一次percent值，这用于碎片云的轮廓绘制
        private List<float> m_lastper_outline_for_fragile;//
        //以下为轨迹小矩形的参数
        private List<InvisibleLight[]> m_pointLight=new List<InvisibleLight[]>();//小矩形的光照
        private MTexture m_pointTexture;//小矩形纹理
        
        public CloudZipperPath(CloudZipper cloud)
        {
            //渲染的深度值
            Depth=1;
            //一些颜色设置
            b_fragile=cloud.i_fragile;
            b_small=cloud.i_small;
            m_ropeColor=b_fragile ? Color.LightPink:Color.Aqua;
            m_trail_point_bloomRadius=b_small? 9.948f:12.0f;
            //控制线条起始和终止位置的参数
            m_node_pos=cloud.i_node_pos;
            for(int i=1;i<m_node_pos.Count;i++)//添加云节点的位置
            {
                Vector2 temp=m_node_pos[i]-m_node_pos[i-1];
                m_distance.Add(temp.Length());//相邻节点的距离
                m_direction.Add(DBBMath.Normalize(temp));//相邻节点的方向
            }
            m_respawnTimer=cloud.i_respawnTimer;
            m_respawnTime=cloud.i_respawnTime;
            b_fragile_pathinit=false;
            m_percent=cloud.i_refPosPoint; 
            //确定轨迹线绘制点
            m_pointTexture=GFX.Game["objects/DBB_Items/CloudZipper/pointTexture"];
            int default_trail_point_num=cloud.i_trail_point_num;
            m_trail_point_bloomRadius=cloud.i_trail_point_bloomRadius;
            m_trail_point_LightStartFade=cloud.i_trail_point_LightStartFade;
            m_trail_point_LightEndFade=cloud.i_trail_point_LightEndFade;
            //根据轨迹长度自动调整一下轨迹点的数目
            m_lastper_outline.Add(1.0f);
            for(int i=0;i<m_distance.Count;i++)
            {
                m_trail_point_num.Add(default_trail_point_num);
                m_lastper_outline.Add(1.0f);
                if(m_distance[i]<default_trail_point_num*m_pointTexture.Width*0.02f)
                {
                    //Logger.Log(LogLevel.Info,"DBBHelper/CloudZipper","Overcrowded number of trail points");
                    //Logger.Log(LogLevel.Info,"DBBHelper/CloudZipper","Number of trail points has been adjusted");
                    int tmp_num=default_trail_point_num-1;
                    while(m_distance[i]<tmp_num*m_pointTexture.Width*0.02f)
                    {
                        tmp_num-=1;
                    }
                    m_trail_point_num[i]=tmp_num;
                }
                //如果轨迹点数目大于0则初始化轨迹点数组
                int current_trail_point_num=m_trail_point_num[i];
                if(current_trail_point_num>0)
                {
                    m_points.Add(new Vector2[current_trail_point_num]);
                    m_lastper.Add(new float[current_trail_point_num]);
                    m_pointLight.Add(new InvisibleLight[current_trail_point_num]);
                }
                 //对轨迹点的属性做进一步赋值
                for(int j=1;j<m_trail_point_num[i]+1;j++)
                {
                    float temp_j=j;
                    float temp_num=m_trail_point_num[i]+1;
                    m_points[i][j-1]=m_node_pos[i]+temp_j/temp_num*m_distance[i]*m_direction[i];
                    m_lastper[i][j-1]=0.0f;
                    //添加点光源和泛光
                    VertexLight li=new VertexLight(m_ropeColor,1.0f,m_trail_point_LightStartFade,m_trail_point_LightEndFade);
                    BloomPoint bp=new BloomPoint(1.0f,m_trail_point_bloomRadius);
                    m_pointLight[i][j-1]=new InvisibleLight(m_points[i][j-1],li,bp);
                }
            }
            //轮廓纹理
            m_cloudOutline=b_fragile?GFX.Game["objects/DBB_Items/CloudZipper/cloud_outline_p"]:GFX.Game["objects/DBB_Items/CloudZipper/cloud_outline_b"];
        }
        public override void Added(Scene scene)
        {
            //Active=false时实体的Update不会被执行，Invisible=false时实体的Render不会被执行
            //不希望CloudPath自己的Update自动被调用,而是把CloudPath的控制权完全交给CloudZipper
            Active=false;
            base.Added(scene);
            
            for(int i=0;i<m_trail_point_num.Count;i++)
            {
                if(m_trail_point_num[i]>0)
                {
                    foreach(var pl in m_pointLight[i])
                    {
                        scene.Add(pl);
                    }
                }
            }
           
        }
        //画轨迹线
        private void DrawTrail()
        {
            float length=(m_percent-m_node_pos[m_path_index]).Length();
            float per;
            //对于碎片云已经碎掉的情况，使用lastper记录值来确定最终的颜色值
            if(m_respawnTimer>0.0f||b_fragile_pathinit==true)
            {
                    per=Math.Clamp(m_respawnTimer/m_respawnTime,0.0f,m_respawnTime);
                    //对于碎片云已经碎掉的情况，使用m_lastper_for_fragile记录值来确定最终的颜色值，并更新
                    float temp;
                    for(int i=0;i<m_points.Count;i++)
                    {
                        for(int j=0;j<m_points[i].Length;j++)
                        {   
                            temp=m_lastper_for_fragile[i][j];
                            m_pointTexture.DrawCentered(m_points[i][j],m_ropeColor*(float)DBBMath.Linear_Lerp(m_lastper[i][j],0.5,0.5+temp),0.02f,m_direction[i].Angle());
                            m_lastper[i][j]=(float)DBBMath.Linear_Lerp(per,0.0,temp);
                        }
                    } 
            }
            //对于云未碎掉的情况，基于距离直接计算最终的颜色值(并保存颜色进度值)
            else
            {
                for(int i=0;i<m_trail_point_num[m_path_index];i++)
                {
                    float point_distance=(m_points[m_path_index][i]-m_node_pos[m_path_index]).Length();
                    per=Math.Clamp((length-point_distance)/(m_pointTexture.Width*0.02f),0.0f,1.0f);
                    m_lastper[m_path_index][i]=per;
                }
                //记录每一次的m_lastper_for_fragile值以便在碎片云碎掉时可以获取临界状态
                m_lastper_for_fragile=m_lastper;
                for(int i=0;i<m_points.Count;i++)
                {
                    for(int j=0;j<m_points[i].Length;j++)
                    {
                        m_pointTexture.DrawCentered(m_points[i][j],m_ropeColor*(float)DBBMath.Linear_Lerp(m_lastper[i][j],0.5,1.0),0.02f,m_direction[i].Angle());
                    }
                }
            }
            
           
        }
        //画轮廓
        private void DrawOutline()
        {
            //绘制云的轮廓
            float tmp_scale=b_small ? 25.0f/35.0f*1.0f : 1.0f;
            float per=0.0f;
            Color tmp_color=Color.White;
            //记录每一次的m_lastper_outline值以便在碎片云碎掉时可以获取临界状态
            if(m_respawnTimer==0.0f)
            {
                m_lastper_outline_for_fragile=m_lastper_outline;
            }
            //对于碎片云已经碎掉的情况，使用m_lastper_outline_for_fragile记录值来确定最终的颜色值，并更新
            if(m_respawnTimer>0.0f||b_fragile_pathinit==true)
            {
                per=Math.Clamp(m_respawnTimer/m_respawnTime,0.0f,m_respawnTime);

                for(int i=0;i<m_node_pos.Count;i++)
                {   
                    m_cloudOutline.DrawCentered(m_node_pos[i],Color.White*m_lastper_outline[i],new Vector2(tmp_scale,1.0f),0.0f);
                    m_lastper_outline[i]=(float)DBBMath.Linear_Lerp(per,1.0f,m_lastper_outline_for_fragile[i]);
                }
            }
            //对于云未碎掉的情况，基于距离直接计算最终的颜色值(并保存颜色进度值)
            else{
                    per=(m_percent-m_node_pos[m_path_index+1]).Length()/m_distance[m_path_index];
                    m_lastper_outline[m_path_index+1]=per;
                    m_lastper_outline[m_path_index]=1-per;
                    for(int i=0;i<m_node_pos.Count;i++)
                    {
                        m_cloudOutline.DrawCentered(m_node_pos[i],Color.White*m_lastper_outline[i],new Vector2(tmp_scale,1.0f),0.0f);
                    }
            }
           
            
        }
        public void Update(Vector2 per,int path_index,bool is_back,float respawnTimer,bool fragile_pathinit)
        {
            base.Update();
            m_percent=per;
            m_path_index=path_index;
            b_is_back=is_back;
            m_respawnTimer=respawnTimer;
            b_fragile_pathinit=fragile_pathinit;
        }
        public override void Render()
        {   
            //绘制Zipper的轮廓、轨迹线
            DrawOutline();
            DrawTrail();
        }
    
    
    }


}