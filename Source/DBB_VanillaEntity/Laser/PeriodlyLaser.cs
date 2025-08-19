using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DBBHelper.Entities {

    [CustomEntity("DBBHelper/PeriodlyLaser")]
    class PeriodlyLaser:Solid
     {
        private Vector2 offset=new Vector2();//关卡偏移
        private float length;//激光长度
        private float width_ratio=1.0f;//激光贴图的宽度比例
        private float thickness=16.0f;//激光碰撞盒厚度

        private Solid solid;//激光所碰撞到的物体
        private bool in_bound;//激光终点是否在关卡内
        private Vector2 start_pos;//激光起始位置
        private Vector2 end_pos;//激光终止位置，仅作为参考用
        private Vector2 dir;//激光方向
        private MTexture laser_texture;//激光贴图
        private Vector2 laser_center;//激光贴图的中心
        private  ParticleType  laser_particle=new ParticleType();//激光粒子
        //激光粒子贴图
        Chooser<MTexture> sourceChooser = new Chooser<MTexture>(GFX.Game["particles/smoke0"], GFX.Game["particles/smoke1"], GFX.Game["particles/smoke2"], GFX.Game["particles/smoke3"]);
        private float t=0.8f; //颜色渐变计时器
        private bool go=true;//颜色渐变方向指示器
        private bool bad_generate=false;//指示激光产生是否合理，这适用于激光源在墙体内部的情况
        public bool active=true;//是否激活激光
        public LaserGeneratorTranslate generator=null;//产生该条激光的激光器，可以没有
        public PeriodlyLaser(EntityData data,Vector2 offset):base(data.Position+offset, 0f, 0f, safe: true)
        {
            //通用设置
            this.Position=data.Position+offset;
            this.offset=offset;
            start_pos=data.Position+offset;
            end_pos=data.Nodes[0]+offset;
            dir=end_pos-start_pos;
            dir=DBBMath.Normalize(dir);
            width_ratio=data.Float("thickness");
            //逻辑量初始化
            length=0.0f;
            in_bound=true;
            solid=null;
            //获取纹理，设置激光厚度
            laser_texture=GFX.Game["objects/DBB_Items/Laser/laser"];
            thickness=8.0f*width_ratio*0.5f;
            //设置激光粒子的一系列属性
            laser_particle=new ParticleType
            {
                SourceChooser=sourceChooser,
                Acceleration=new Vector2(0f, -4f),
                LifeMin=0.2f,
                LifeMax=0.5f,
                Size=0.5f,
                SizeRange=0f,
                Direction=-MathF.PI / 2f,
                DirectionRange=2.5f*width_ratio,
                SpeedMin=4f,
                SpeedMax=24f,
                RotationMode=ParticleType.RotationModes.Random,
                ScaleOut=true,
                Color=Color.Yellow,
                Color2=Color.YellowGreen,
                FadeMode=ParticleType.FadeModes.Late,
                ColorMode=ParticleType.ColorModes.Blink
            };
            Depth=100;
        }
        //这个给激光生成器用
        public PeriodlyLaser(Vector2 pos_start,Vector2 dir,float width_ratio):base(pos_start, 0f, 0f, safe: true)
        {
            //通用设置
            this.Position=pos_start;
            start_pos=pos_start;
            this.dir=DBBMath.Normalize(dir);
            this.width_ratio=width_ratio;
            //逻辑量初始化
            length=0.0f;
            in_bound=true;
            solid=null;
            //获取纹理，设置激光厚度
            laser_texture=GFX.Game["objects/DBB_Items/Laser/laser"];
            this.thickness=8.0f*width_ratio*0.48f;
            //设置激光粒子的一系列属性
            laser_particle=new ParticleType
            {
                SourceChooser=sourceChooser,
                Acceleration=new Vector2(0f, -4f),
                LifeMin=0.2f,
                LifeMax=0.5f,
                Size=0.5f,
                SizeRange=0f,
                Direction=-MathF.PI / 2f,
                DirectionRange=2.5f*width_ratio,
                SpeedMin=4f,
                SpeedMax=24f,
                RotationMode=ParticleType.RotationModes.Random,
                ScaleOut=true,
                Color=Color.Yellow,
                Color2=Color.YellowGreen,
                FadeMode=ParticleType.FadeModes.Late,
                ColorMode=ParticleType.ColorModes.Blink
            };
            Depth=-3;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
        }
        //改变激光的位置
        public void ChangePos(Vector2 new_pos)
        {
            MoveH((new_pos-Position).X);
            MoveV((new_pos-Position).Y);
            start_pos=Position;
        }
        //更新激光开关情况
        public void Switch(bool open_or_down)
        {
            active=open_or_down;
        }
        //用来更新射线长度
        private void UpdateLength()
        {
            //Bug修复：即使active为false，这个地方也需要继续更新，你可以不绘制，但是必须要保证绘制位置正确。
            //如果这里active为false时直接返回，则在下一次active激活时会有一帧被错误绘制(绘制位置未能正确更新)，从而出图形瞬移的问题
            Level level=base.Scene as Level;
            length=0.0f;
            in_bound=true;
            solid=null;
            //如果射线在关卡内部而且仍然还没有撞击到物体(撞击到自己的激光发生器不算撞击到物体)
            while(in_bound&&!CollideCheck_Not_Include_Its_Generator(Position,Position+dir*length))
            {
                //不断延长射线直到越界或者遇到第一个撞击点为止
                in_bound=level.IsInBounds(Position+length*dir);
                length+=8.0f;
            }
            //这种情况下很有可能发射源直接在墙体内部，这时候就不要再尝试后面的工作了
            if(length==0.0f)
            {
                bad_generate=true;
                return;
            }
            //能来到这一步的是合法射线
            bad_generate=false;
            //此时要么越界要么有撞击点
            if(!in_bound)
            {
                laser_center=start_pos+length*0.5f*dir;
                return;
            }
            else
            {
                //有撞击点的时候进行距离修正
                while(CollideCheck_Not_Include_Its_Generator(Position,Position+dir*length))
                {
                    length-=1.0f;
                }
                length+=1.0f;
                laser_center=start_pos+length*0.5f*dir;
                solid=CollideFirst_Not_Include_Its_Generator(Position,Position+dir*length);
                //激光粒子的方向应该要根据墙体进行调整
                //首先获取具体的砖块的位置
                Vector2 level_pos=Position+dir*length-offset;
                int x_block_num=(int)level_pos.X/8;
                int y_block_num=(int)level_pos.Y/8;
                level_pos.X=x_block_num*8.0f+4.0f;
                level_pos.Y=y_block_num*8.0f+4.0f;
                //Logger.Log(LogLevel.Warn,"x_block_num",x_block_num.ToString());
                //Logger.Log(LogLevel.Warn,"y_block_num",y_block_num.ToString());
                //Bug：求解数值不稳定，要改的话比较麻烦，不想改，而且对视觉造成不了什么大的影响，LPL >.<
                level_pos+=offset;
                Vector2 face_vec=start_pos+length*dir-level_pos;
                float axis_angle=face_vec.Angle();
                if(axis_angle>=-Math.PI*0.75f&&axis_angle<-Math.PI*0.25f)
                {
                    laser_particle.Direction=-MathF.PI/2.0f;
                }
                else if(axis_angle>=-Math.PI*0.25f&&axis_angle<Math.PI*0.25f)
                {
                    laser_particle.Direction=0;
                }
                else if(axis_angle>=Math.PI*0.25f&&axis_angle<Math.PI*0.75f)
                {
                    laser_particle.Direction=MathF.PI/2.0f;
                }
                else if( (axis_angle>=Math.PI*0.75f&&axis_angle<=Math.PI)||(axis_angle>=-Math.PI&&axis_angle<-Math.PI*0.75f) )
                {
                    laser_particle.Direction=(float)Math.PI;
                }
            }
            return;
        }
   
        //检查人物是否与之碰撞
        private bool CheckPlayer(Player player)
        {
            //没有人物则返回false
            if(player==null)
            {
                return false;
            }
            //获取垂直于方向的两个向量
            Vector2 side_one=DBBMath.Normalize(DBBMath.Rotate(start_pos,start_pos+dir,90.0f)-start_pos);
            Vector2 side_two=-side_one;
            //对应可以获取碰撞盒的四个点
            List<Vector2> all_p=new List<Vector2>();
            all_p.Add(start_pos+side_two*thickness*0.5f);
            all_p.Add(start_pos+side_one*thickness*0.5f);
            all_p.Add(all_p[1]+dir*length);
            all_p.Add(all_p[0]+dir*length);
            //获取人物碰撞盒的四个点，人物的碰撞盒位置好像有问题，这里找了一个最接近Debug模式下的碰撞盒
            List<Vector2>player_all_p=new List<Vector2>();
            player_all_p.Add(player.hurtbox.TopRight+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            player_all_p.Add(player.hurtbox.TopLeft+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            player_all_p.Add(player.hurtbox.BottomLeft+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            player_all_p.Add(player.hurtbox.BottomRight+player.Collider.AbsolutePosition+new Vector2(player.hurtbox.width/2.0f,player.hurtbox.height+1.0f));
            //获取两个碰撞盒的中心
            Vector2 player_center=0.5f*(player_all_p[0]+player_all_p[2]);
            Vector2 laser_center=0.5f*(all_p[0]+all_p[2]);
            //分离轴定理(SAT)以及OBB与OBB的检测
            //对于人物碰撞盒
            for(int i=0;i<player_all_p.Count();i++)
            {
                Vector2 temp_origin=player_all_p[i];
                Vector2 temp_axis;
                //获取轴
                if(i!=player_all_p.Count()-1)
                {
                    temp_axis=DBBMath.Normalize(DBBMath.Rotate(player_all_p[i],player_all_p[i+1],90)-player_all_p[i]);
                }
                else
                {
                    temp_axis=DBBMath.Normalize(DBBMath.Rotate(player_all_p[i],player_all_p[0],90)-player_all_p[i]);
                }
                //人物碰撞盒中心投影，投影最小值，投影最大值
                float x1_center,x1_min,x1_max;
                //获取人物碰撞盒中心在轴上的投影
                x1_center=Vector2.Dot(player_center-player_all_p[i],temp_axis);
                //初始化x1_min和x1_max
                x1_min=Vector2.Dot(player_all_p[0]-player_all_p[i],temp_axis);
                x1_max=Vector2.Dot(player_all_p[0]-player_all_p[i],temp_axis);
                //寻找最大区间范围
                for(int j=1;j<player_all_p.Count;j++)
                {
                    if(j==i)
                    {
                        continue;
                    }
                    x1_min=Math.Min(x1_min,Vector2.Dot(player_all_p[j]-player_all_p[i],temp_axis));
                    x1_max=Math.Max(x1_max,Vector2.Dot(player_all_p[j]-player_all_p[i],temp_axis));
                    
                }
                //激光碰撞盒中心投影，投影最小值，投影最大值
                float x2_center,x2_min,x2_max;
                //获取激光碰撞盒中心在轴上的投影
                x2_center=Vector2.Dot(laser_center-player_all_p[i],temp_axis);
                //初始化x2_min和x2_max
                x2_min=Vector2.Dot(all_p[0]-player_all_p[i],temp_axis);
                x2_max=Vector2.Dot(all_p[0]-player_all_p[i],temp_axis);
                //寻找最大区间范围
                for(int j=0;j<all_p.Count();j++)
                {
                    x2_min=Math.Min(x2_min,Vector2.Dot(all_p[j]-player_all_p[i],temp_axis));
                    x2_max=Math.Max(x2_max,Vector2.Dot(all_p[j]-player_all_p[i],temp_axis));
                    
                }
                //如果投影中心之间的距离大于0.5倍的两个投影长度，则二者分离
                if(Math.Abs(x1_center-x2_center)>0.5*(x1_max-x1_min)+0.5*(x2_max-x2_min))
                {
                    return false;
                }

            }
            //对于激光碰撞盒
            for(int i=0;i<all_p.Count;i++)
            {
                Vector2 temp_origin=all_p[i];
                Vector2 temp_axis;
                //获取轴
                if(i!=all_p.Count()-1)
                {
                    temp_axis=DBBMath.Normalize(DBBMath.Rotate(all_p[i],all_p[i+1],90)-all_p[i]);
                }
                else
                {
                    temp_axis=DBBMath.Normalize(DBBMath.Rotate(all_p[i],all_p[0],90)-all_p[i]);
                }
                //激光碰撞盒中心投影，投影最小值，投影最大值
                float x1_center,x1_min,x1_max;
                //获取激光碰撞盒中心在轴上的投影
                x1_center=Vector2.Dot(laser_center-all_p[i],temp_axis);
                //初始化x1_min和x1_max
                x1_min=Vector2.Dot(all_p[0]-all_p[i],temp_axis);
                x1_max=Vector2.Dot(all_p[0]-all_p[i],temp_axis);
                //寻找最大区间范围
                for(int j=1;j<all_p.Count();j++)
                {
                    if(j==i)
                    {
                        continue;
                    }
                    x1_min=Math.Min(x1_min,Vector2.Dot(all_p[j]-all_p[i],temp_axis));
                    x1_max=Math.Max(x1_max,Vector2.Dot(all_p[j]-all_p[i],temp_axis));
                    
                }
                //人物碰撞盒中心投影，投影最小值，投影最大值
                float x2_center,x2_min,x2_max;
                //获取人物碰撞盒中心在轴上的投影
                x2_center=Vector2.Dot(player_center-all_p[i],temp_axis);
                //初始化x2_min和x2_max
                x2_min=Vector2.Dot(player_all_p[0]-all_p[i],temp_axis);
                x2_max=Vector2.Dot(player_all_p[0]-all_p[i],temp_axis);
                //寻找最大区间范围
                for(int j=0;j<all_p.Count();j++)
                {
                    x2_min=Math.Min(x2_min,Vector2.Dot(player_all_p[j]-all_p[i],temp_axis));
                    x2_max=Math.Max(x2_max,Vector2.Dot(player_all_p[j]-all_p[i],temp_axis));
                }
                //如果投影中心之间的距离大于0.5倍的两个投影长度，则二者分离
                if(Math.Abs(x1_center-x2_center)>0.5*(x1_max-x1_min)+0.5*(x2_max-x2_min))
                {
                    return false;
                }
            }
            //两个碰撞盒相交
            return true;
        }
        //特殊碰撞检测
        private bool CollideCheck_Not_Include_Its_Generator(Vector2 from, Vector2 to)
        {
            if(Scene==null)
            {
                return false;
            }
            List<Solid>all_solid=base.Scene.CollideAll<Solid>(from,to);
            bool result=false;
            foreach(var s in all_solid)
            {
                if(s==generator)
                {
                    continue;
                }
                else
                {
                    result=true;
                    break;
                }
            }
            return result;
        }
        private Solid CollideFirst_Not_Include_Its_Generator(Vector2 from, Vector2 to)
        {
            if(Scene==null)
            {
                return default;
            }
            List<Solid>all_solid=base.Scene.CollideAll<Solid>(from,to);
            Solid result=default;
            foreach(var s in all_solid)
            {
                if(s==generator)
                {
                    continue;
                }
                else
                {
                    result=s;
                    break;
                }
            }
            return result;
        }
        public override void Update()
        {
            //激光发生器的更新永远慢激光一帧，因此激光发生器需要进行预更新来确保激光的位置与发生器的位置同步
            if(generator!=null)
            {
                generator.VirtualUpdate();
            }
            base.Update();
            UpdateLength();
            //如果未激活则直接返回
            if(active==false)
            {
                return;
            }
            //对于位置不好的激光，直接禁用掉
            if(bad_generate==true)
            {
                return;
            }
            //计算激光此时的亮度值
            if(go==true)
            {
                t+=0.2f*Engine.DeltaTime;
                if(t>1.0f)
                {
                    t=1.0f;
                    go=false;
                }
            }
            else if(go==false)
            {
                t-=0.2f*Engine.DeltaTime;
                if(t<0.8f)
                {
                    t=0.8f;
                    go=true;
                }
            }
            //如果激光撞击到物体，则产生粒子
            if(solid!=null)
            {
                //粒子效果
                (Scene as Level).ParticlesFG.Emit(laser_particle,start_pos+length*dir);
            }
            Player player=Scene.Tracker.GetEntity<Player>();
            //没有玩家或者玩家已经躺尸就结束更新
            if(player==null||player.Dead)
            {
                return;
            }
            //碰撞检测，如果玩家碰到了那就去Die吧
            if(CheckPlayer(player))
            {
               
                player.Die(new Vector2((player.Facing==Facings.Right)?1.0f:-1.0f,0.0f));
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            List<Vector2> all_p=new List<Vector2>();
            Vector2 side_one=DBBMath.Normalize(DBBMath.Rotate(start_pos,start_pos+dir,90.0f)-start_pos);
            Vector2 side_two=-side_one;
           // Logger.Log(LogLevel.Warn,"laser2",start_pos.ToString());
            all_p.Add(start_pos+side_two*thickness*0.5f);
            all_p.Add(start_pos+side_one*thickness*0.5f);
            all_p.Add(all_p[1]+dir*length);
            all_p.Add(all_p[0]+dir*length);
            Draw.Line(all_p[0],all_p[1],Color.BurlyWood);
            Draw.Line(all_p[1],all_p[2],Color.BurlyWood);
            Draw.Line(all_p[2],all_p[3],Color.BurlyWood);
            Draw.Line(all_p[3],all_p[0],Color.BurlyWood);
        }
      
        public override void Render()
        {
            base.Render();
            //如果未激活则直接返回
            if(active==false)
            {
                return;
            }
            //对于位置不好的激光，直接禁用掉
            if(bad_generate==true)
            {
                return;
            }
            // Logger.Log(LogLevel.Warn,"laser1",start_pos.ToString());
            //绘制激光
            laser_texture.DrawCentered(laser_center,Color.White*t,new Vector2(length/64.0f,width_ratio*0.5f),dir.Angle());
            Player player=Scene.Tracker.GetEntity<Player>();
            //没有玩家或者玩家已经躺尸就结束更新
            if(player==null||player.Dead)
            {
                return;
            }

        }   
    }
}