using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {
    [CustomEntity("DBBHelper/Fluid")]
    public class Fluid:Entity
    {
        int row=1;
        int column=1;
        int real_row=1;
        int real_column=1;
        List<float>grid_density=null;//密度场
        List<float>velocity_x=null;//水平速度场
        List<float>velocity_y=null;//垂直速度场
        List<float>grid_p=null;//标量场，此场用于获取速度场中的无散分量
        List<float>grid_div=null;//无旋场，此场用于获取速度场中的无散分量
        Vector2 topleft=new Vector2();
        float width;
        float height;
        float unit=8.0f;
        float half_unit=0.0f;
        public Fluid(EntityData data,Vector2 offset)
        {
            Position=data.Position+offset;
            width=data.Width;
            height=data.Height;

            unit=2.0f;
            half_unit=unit/2.0f;

            column=(int)(height/unit);
            row=(int)(width/unit);
            real_row=row+2;
            real_column=column+2;
            grid_density=new List<float>();
            velocity_x=new List<float>();
            velocity_y=new List<float>();
            grid_p=new List<float>();
            grid_div=new List<float>();
            //初始化密度场和粒子场
            for(int i=0;i<real_row*real_column;i++)
            {
                grid_density.Add(0.0f);
                velocity_x.Add(0.0f);
                velocity_y.Add(0.0f);
                grid_p.Add(0.0f);
                grid_div.Add(0.0f);
            }
            Collider=new Hitbox(width,height,Position.X,Position.Y);
            topleft.X=Position.X;
            topleft.Y=Position.Y;
            Depth=-100;
        }
        //绘制第(i,j)个格子，送入的参数应当是虚拟格子数
        private void DrawVirtualGridAt(Color color,float x_block_num,float y_block_num)
        {
            Draw.Rect(topleft.X+(int)x_block_num*unit,topleft.Y+(int)y_block_num*unit,unit,unit,color);
        }
        //获取指定位置所对应的格子索引，得到的是真实的格子数
        private Vector2 GetGridIndexAt(float x,float y)
        {
            int x_block_num=(int)(x/unit);
            int y_block_num=(int)(y/unit);

            return new Vector2(x_block_num+1,y_block_num+1);
        }
        //获取第(i,j)个格子的密度，参数为真实的格子数
        private float GetGridValueAt(ref List<float>field,float x_block_num,float y_block_num)
        {
            return field[(int)x_block_num*real_column+(int)y_block_num];
        }
        //更新第(i,j)格子的密度，参数为真实的格子数
        private void SetGridValue(ref List<float>field,float d,float x_block_num,float y_block_num,bool limit=false)
        {
            //如果限制模式开启，则会将其范围限制在0到1
            if(limit==true)
            {
                if(d>1.0f)
                {
                    field[(int)x_block_num*real_column+(int)y_block_num]=1.0f;
                    return;
                }
                else if(d<0.0f)
                {
                    field[(int)x_block_num*real_column+(int)y_block_num]=0.0f;
                    return;
                }
            }
            //没有开启限制模式，则正常赋值
            field[(int)x_block_num*real_column+(int)y_block_num]=d;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        //处理输入源
        private void AddSourceValue(ref List<float>field,float acceleration=0.01f,bool limit=false)
        {
            Player player=Scene.Tracker.GetEntity<Player>();
            //人物不存在，则返回
            if(player==null)
            {
                return;
            }
            //人物没有与物体碰撞，则返回
            if(CollideCheck<Player>()==false)
            {   
                return;
            }
            //获取人物碰撞盒的四个顶点的位置
            player=CollideFirst<Player>();
            List<Vector2>player_all_p=new List<Vector2>();
            player_all_p.Add(player.hurtbox.TopRight+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            player_all_p.Add(player.hurtbox.TopLeft+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            player_all_p.Add(player.hurtbox.BottomLeft+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            player_all_p.Add(player.hurtbox.BottomRight+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            int x_min=99999;int x_max=-99999;int y_min=99999;int y_max=-99999;
            //获取人物碰撞盒的覆盖的格子范围
            foreach(var player_p in player_all_p)
            {
                Vector2 temp=player_p-topleft;
                Vector2 index=GetGridIndexAt(temp.X,temp.Y);
                x_min=(int)Math.Min(x_min,index.X);
                x_max=(int)Math.Max(x_max,index.X);
                y_min=(int)Math.Min(y_min,index.Y);
                y_max=(int)Math.Max(y_max,index.Y);
            }
            //修正格子范围
            if(x_min<1){x_min=0;}else if(x_min>column+1){x_min=column+1;}
            if(x_max<1){x_max=0;}else if(x_max>column+1){x_max=column+1;}
            if(y_min<1){y_min=0;}else if(y_min>row+1){y_min=row+1;}
            if(y_max<1){y_max=0;}else if(y_max>row+1){y_max=row+1;}
            //对范围内的所有格子添加输入源
            for(int i=x_min;i<=x_max;i++)
            {
                for(int j=y_min;j<=y_max;j++)
                {
                    if(i!=0&&i!=column+1&&j!=0&&j!=row+1)
                    {
                        SetGridValue(ref field,GetGridValueAt(ref field,i,j)+acceleration,i,j,limit);
                    }
                }
            }
        }

        //处理扩散项
        private void DiffuseField(ref List<float>field,int mode,bool limit=false)
        {
            //高斯-塞德尔迭代，对于这个式子来说是收敛的，因为系数矩阵严格对角占优
            List<float> old_field=new List<float>(field.ToArray());
            float a=Engine.DeltaTime*row*column;
            for(int iter=0;iter<10;iter++)
            {
                for(int i=1;i<row+1;i++)
                {
                    for(int j=1;j<column+1;j++)
                    {
                        
                        float value=GetGridValueAt(ref old_field,i,j)+a*
                        (
                            GetGridValueAt(ref old_field,i-1,j)+GetGridValueAt(ref old_field,i+1,j)+GetGridValueAt(ref old_field,i,j-1)+GetGridValueAt(ref old_field,i,j+1)
                        );
                        value=value/(1.0f+4.0f*a);
                        SetGridValue(ref field,value,i,j,limit);
                    }
                }
                old_field=new List<float>(field.ToArray());
            }
            FieldBoundJustify(ref field,mode);
        }

        //处理平流项
        private void AdvectField(ref List<float>field,int mode,bool limit=false)
        {
            //对于平流项，它是依赖于速度场的
            //采用粒子模拟的方法来更新场值
            //记录旧场值
            List<float>old_filed=new List<float>(field);
            for(int i=1;i<row+1;i++)
            {
                for(int j=1;j<column+1;j++)
                {
                    //在每个格子的中心产生粒子后根据该格子的速度值来发射粒子
                    //要注意当前产生的粒子是下一时刻的粒子，要更新当前时刻的场值需要对此进行回溯
                    float particle_x=i*unit+half_unit-GetGridValueAt(ref velocity_x,i,j)*Engine.DeltaTime;
                    float particle_y=j*unit+half_unit-GetGridValueAt(ref velocity_y,i,j)*Engine.DeltaTime;
                    //判断现在粒子的位置，首先先修正粒子位置
                    if(particle_x<half_unit){particle_x=half_unit;}else if(particle_x>(column+1)*unit+half_unit){particle_x=(column+1)*unit+half_unit*0.6f;}
                    if(particle_y<half_unit){particle_y=half_unit;}else if(particle_y>(row+1)*unit+half_unit){particle_y=(row+1)*unit+half_unit*0.6f;}
                    //获取粒子所在的网格
                    int x_block_num=(int)((particle_x-half_unit)/unit);float x_lerp_ratio=(particle_x-x_block_num*unit-half_unit)/unit;
                    int y_block_num=(int)((particle_y-half_unit)/unit);float y_lerp_ratio=(particle_y-y_block_num*unit-half_unit)/unit;

                    float d1=GetGridValueAt(ref old_filed,x_block_num,y_block_num);
                    float d2=GetGridValueAt(ref old_filed,x_block_num+1,y_block_num);
                    float d3=GetGridValueAt(ref old_filed,x_block_num,y_block_num+1);
                    float d4=GetGridValueAt(ref old_filed,x_block_num+1,y_block_num+1);
                    //获取对粒子所携带的场值，通过双线性插值当前时刻粒子周围网格的场值来得到粒子所携带的值，并将其赋予其所在的网格
                    float d5=d1*x_lerp_ratio+d2*(1.0f-x_lerp_ratio);
                    float d6=d3*x_lerp_ratio+d4*(1.0f-x_lerp_ratio);
                    float d_final=d5*y_lerp_ratio+d6*(1.0f-y_lerp_ratio);
                    //更新当前时刻的场值
                    SetGridValue(ref field,d_final,i,j,limit);
                }
            }
            FieldBoundJustify(ref field,mode);
        }
        //获取无散场，尽可能除去场中的无旋场分量
        private void Project()
        {
            float h=1.0f/row/column;
            for(int i=1;i<row+1;i++)
            {
                for(int j=1;j<column+1;j++)
                {
                    float value=-0.1f*h*(
                        GetGridValueAt(ref velocity_x,i+1,j)-GetGridValueAt(ref velocity_x,i-1,j)+GetGridValueAt(ref velocity_y,i,j-1)-GetGridValueAt(ref velocity_y,i,j+1)
                    );
                    SetGridValue(ref grid_div,value,i,j);
                    SetGridValue(ref grid_p,0.0f,i,j);
                }
            }
            FieldBoundJustify(ref grid_div,0);
            FieldBoundJustify(ref grid_p,0);
            List<float>old_p=new List<float>(grid_p);
            for(int iter=0;iter<10;iter++)
            {
                for(int i=1;i<row+1;i++)
                {
                    for(int j=1;j<column+1;j++)
                    {
                        float value=0.25f*(
                            GetGridValueAt(ref grid_div,i,j)+GetGridValueAt(ref old_p,i-1,j)+GetGridValueAt(ref old_p,i+1,j)+GetGridValueAt(ref old_p,i,j-1)+GetGridValueAt(ref old_p,i,j+1)
                        );
                        SetGridValue(ref grid_p,value,i,j);
                    }
                }
                old_p=new List<float>(grid_p);
            }
            FieldBoundJustify(ref grid_p,0);
            List<float>old_velocity_x=new List<float>(velocity_x);
            List<float>old_velocity_y=new List<float>(velocity_y);
            for(int i=1;i<row+1;i++)
            {
                for(int j=1;j<column+1;j++)
                {
                    float x_value=GetGridValueAt(ref old_velocity_x,i,j)-0.5f*h*(GetGridValueAt(ref old_velocity_x,i+1,j)-GetGridValueAt(ref old_velocity_x,i-1,j));
                    float y_value=GetGridValueAt(ref old_velocity_y,i,j)-0.5f*h*(GetGridValueAt(ref old_velocity_y,i,j-1)-GetGridValueAt(ref old_velocity_y,i,j+1));
                    SetGridValue(ref velocity_x,x_value,i,j);
                    SetGridValue(ref velocity_y,y_value,i,j);
                }
            }
            FieldBoundJustify(ref velocity_x,1);
            FieldBoundJustify(ref velocity_y,2);
        }
        //依据不同模式处理边界条件，mode=0为密度模式，mode=1为速度X分量模式，mode=2为速度Y分量模式
        private void FieldBoundJustify(ref List<float>field,int mode)
        {

            //处理边界条件，mode=0为密度模式，mode=1为速度X分量模式，mode=2为速度Y分量模式
                //对于密度而言，边界格子和它相邻的格子的值相同就可以
                //对于速度的X分量，当位于垂直壁时，速度的X分量应该为0，因此对于第0列和第column+1列，其格子的X分量应当为相邻格子速度X分量取反
                //对于速度的Y分量，当位于水平壁时，速度的Y分量应该为0，因此对于第0行和第row+1行，其格子的Y分量应当为相邻格子速度Y分量取反
            //第0行不包括边角
            //第row+1行不包括边角
            for(int j=1;j<column+1;j++)
            {
                if(mode==2)
                {
                    SetGridValue(ref field,-GetGridValueAt(ref field,1,j),0,j);
                    SetGridValue(ref field,-GetGridValueAt(ref field,row,j),row+1,j);
                }
                else
                {
                    SetGridValue(ref field,GetGridValueAt(ref field,1,j),0,j);
                    SetGridValue(ref field,GetGridValueAt(ref field,row,j),row+1,j);
                   // SetGridValue(ref field,0,0,j);
                    //SetGridValue(ref field,0,row+1,j);
                }
            }
            //第0列不包括边角
            //第column+1列不包括边角
            for(int i=1;i<row+1;i++)
            {
                if(mode==1)
                {
                    SetGridValue(ref field,-GetGridValueAt(ref field,i,0),i,1);
                    SetGridValue(ref field,-GetGridValueAt(ref field,i,column),i,column+1);

                }
                else
                {
                    SetGridValue(ref field,GetGridValueAt(ref field,i,0),i,1);
                    SetGridValue(ref field,GetGridValueAt(ref field,i,column),i,column+1);
                   // SetGridValue(ref field,0,i,1);
                   // SetGridValue(ref field,0,i,column+1);
                }
            }
            //处理四个角的密度值
            SetGridValue(ref field,0.5f*(GetGridValueAt(ref field,0,1)+GetGridValueAt(ref field,1,0)),0,0);
            SetGridValue(ref field,0.5f*(GetGridValueAt(ref field,row,0)+GetGridValueAt(ref field,row+1,1)),row+1,0);
            SetGridValue(ref field,0.5f*(GetGridValueAt(ref field,0,column)+GetGridValueAt(ref field,1,column+1)),0,column+1);
            SetGridValue(ref field,0.5f*(GetGridValueAt(ref field,row,column+1)+GetGridValueAt(ref field,row+1,column)),row+1,column+1);
        }
        private void Simulate_Density()
        {
            Player player=Scene.Tracker.GetEntity<Player>();
            if(player!=null)
            {
                AddSourceValue(ref grid_density,1.0f*Engine.DeltaTime,true);
            }
            DiffuseField(ref grid_density,0,true);
            AdvectField(ref grid_density,0,true);
        }
        private void Simulate_Velocity()
        {
            Player player=Scene.Tracker.GetEntity<Player>();
            if(player!=null)
            {
                AddSourceValue(ref velocity_x,-player.Speed.X*Engine.DeltaTime,false);
                AddSourceValue(ref velocity_y,-player.Speed.Y*Engine.DeltaTime,false);
            }
            DiffuseField(ref velocity_x,1,false);DiffuseField(ref velocity_y,2,false);
            /*
            for(int i=1;i<row+1;i++)
            {
                for(int j=1;j<column+1;j++)
                {
                    Logger.Log(LogLevel.Warn,"diffuse",GetGridValueAt(ref velocity_x,i,j).ToString());
                }
            }*/
            Project();
            AdvectField(ref velocity_x,1,false);AdvectField(ref velocity_y,2,false);
            Project();
            //Logger.Log(LogLevel.Warn,"fluid","//////////////////////////////////");
            //Logger.Log(LogLevel.Warn,"fluid","//////////////////////////////////");
            //Logger.Log(LogLevel.Warn,"fluid","//////////////////////////////////");
        }
        public override void Update()
        {
            base.Update();
            Simulate_Velocity();
            Simulate_Density();
            //Player player=Scene.Tracker.GetEntity<Player>();
             //获取人物碰撞盒的四个顶点的位置
            Player player=CollideFirst<Player>();
            if(player==null)
            {
                for(int i=1;i<row+1;i++)
                {
                    for(int j=1;j<column+1;j++)
                    {
                        float d=GetGridValueAt(ref grid_density,i,j);d-=0.1f*Engine.DeltaTime;if(d<0.0f){d=0.0f;}
                        float vx=GetGridValueAt(ref velocity_x,i,j);if(vx<0.0f){vx+=Engine.DeltaTime;}else if(vx>0.0f){vx-=Engine.DeltaTime;}
                        float vy=GetGridValueAt(ref velocity_y,i,j);if(vy<0.0f){vy+=Engine.DeltaTime;}else if(vy>0.0f){vy-=Engine.DeltaTime;}
                        SetGridValue(ref grid_density,d,i,j);
                        SetGridValue(ref velocity_x,vx,i,j);
                        SetGridValue(ref velocity_y,vy,i,j);
                    }
                }
                return;
            }
            List<Vector2>player_all_p=
            [
                player.hurtbox.TopRight+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f),
                player.hurtbox.TopLeft+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f),
                player.hurtbox.BottomLeft+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f),
                player.hurtbox.BottomRight+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f),
            ];
            int x_min=0;int x_max=0;int y_min=0;int y_max=0;
            //获取人物碰撞盒的覆盖的格子范围
            foreach(var player_p in player_all_p)
            {
                Vector2 temp=player_p-topleft;
                Vector2 index=GetGridIndexAt(temp.X,temp.Y);
                x_min=(int)Math.Min(x_min,index.X);
                x_max=(int)Math.Max(x_max,index.X);
                y_min=(int)Math.Min(y_min,index.Y);
                y_max=(int)Math.Max(y_max,index.Y);
            }
            //修正格子范围
            if(x_min<1){x_min=0;}else if(x_min>column+1){x_min=column;}
            if(x_max<1){x_max=0;}else if(x_max>column+1){x_max=column;}
            if(y_min<1){y_min=0;}else if(y_min>row+1){y_min=row;}
            if(y_max<1){y_max=0;}else if(y_max>row+1){y_max=row;}
            for(int i=1;i<row+1;i++)
            {
                for(int j=1;j<column+1;j++)
                {

                    if(i>=x_min&&i<=x_max)
                    {
                        if(j>=y_min&&j<=y_max)
                        {
                            continue;
                        }
                    }
                    float d=GetGridValueAt(ref grid_density,i,j);d-=0.1f*Engine.DeltaTime;if(d<0.0f){d=0.0f;}
                    float vx=GetGridValueAt(ref velocity_x,i,j);if(vx<0.0f){vx+=Engine.DeltaTime;}else if(vx>0.0f){vx-=Engine.DeltaTime;}
                    float vy=GetGridValueAt(ref velocity_y,i,j);if(vy<0.0f){vy+=Engine.DeltaTime;}else if(vy>0.0f){vy-=Engine.DeltaTime;}
                    SetGridValue(ref grid_density,d,i,j);
                    SetGridValue(ref velocity_x,vx,i,j);
                    SetGridValue(ref velocity_y,vy,i,j);
                }
            }
        }
        public override void Render()
        {
            base.Render();
            for(int i=1;i<row+1;i++)
            {
                for(int j=1;j<column+1;j++)
                {
                    float lerp_ratio=GetGridValueAt(ref grid_density,i,j);
                    //Color color=Color.SkyBlue*lerp_ratio;
                    float vx=GetGridValueAt(ref velocity_x,i,j);
                    float vy=GetGridValueAt(ref velocity_y,i,j);
                    Color color=new Color(Math.Abs(vx),Math.Abs(vy),Math.Abs(vy),lerp_ratio);
                    DrawVirtualGridAt(color,i-1,j-1);
                }
            }
            Draw.HollowRect(topleft.X,topleft.Y,width,height,Color.White);

        }
        /*
 Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, depthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
        foreach (ParticleSystem system in Systems)
        {
            system.Render();
        }
        Draw.SpriteBatch.Draw(Source.Texture.Texture_Safe, position, Source.ClipRect, Color, Rotation, Source.Center, Size, SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
        */

    }

}