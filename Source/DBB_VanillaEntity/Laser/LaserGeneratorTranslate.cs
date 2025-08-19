using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {

    [CustomEntity("DBBHelper/LaserGeneratorTranslate")]
     class LaserGeneratorTranslate:Solid
     {
         private enum State
         {
            going_to_move,//慢启动状态
            moving,//启动状态
            going_to_stop,//即将停止状态
            stop//停止状态
         }
         //激光相关
         private PeriodlyLaser laser;//激光
         private bool laser_switch=true;//激光开关控制
         private string laser_switch_label=" ";//激光标签
         private bool revert=false;//控制激光标签生效的方式，是标签存在时生效还是标签不存在时生效
         private bool on=false;//整体开关控制
         private Vector2 dir=new Vector2(1,0);//激光方向
         private float width_ratio=1.0f;//激光宽度比率
         //位置
         private Vector2 start_pos;//起点
         private Vector2 end_pos;//终点

         //运动相关
         private float t_move=0.0f;//运动控制
         private string style;//运动方式
         private bool go=true;//是前往终点还是返回原点

         //状态机
         private State state=State.going_to_move;

         private float speed=1.0f;//运动速度
         private float acceleration=1.0f;//运动加速度
         private float acceleration_cofficient=1.0f;//运动加速度变化系数
         //所有运动方式
         public static readonly string[] allstyle={
            "Linear",
            "easeInSin","easeOutSin","easeInOutSin",
            "easeInCubic","easeOutCubic","easeInOutCubic",
            "easeInQuard","easeOutQuard","easeInOutQuard",
         };

         //发生器贴图
         private Sprite generator;
         private float angle=0.0f;//贴图角度修正
        public LaserGeneratorTranslate(EntityData data,Vector2 offset):base(data.Position+offset, 0f, 0f, safe: true)
        {
            //通用设置
            Position=data.Position+offset;
            end_pos=Position;
            start_pos=data.Nodes[0]+offset;
            Position = end_pos;
            style=data.Attr("style");
            if(!allstyle.Contains(style)){style="Linear";}
            string dir_name=data.Attr("direction");
            width_ratio=data.Float("thickness");
            speed=data.Float("speed");
            laser_switch_label=data.Attr("label");
            revert=data.Bool("revert");
            acceleration_cofficient=data.Float("acceleration");
            //激光方向设置
            if(dir_name=="up"){dir=new Vector2(0,-1);angle=(float)Math.PI/2.0f;Collider.Width=16.0f;Collider.Height=8.0f; Collider.Position=new Vector2(-8.0f,-7.0f);}
            else if(dir_name=="down"){dir=new Vector2(0,1);angle=(float)-Math.PI/2.0f;Collider.Width=16.0f;Collider.Height=8.0f;Collider.Position=new Vector2(-8.0f,0.0f);}
            else if(dir_name=="left"){dir=new Vector2(-1,0);Collider.Width=8.0f;Collider.Height=16.0f;Collider.Position=new Vector2(-7.0f,-8.0f);}
            else{dir=new Vector2(1,0);angle=(float)Math.PI;Collider.Width=8.0f;Collider.Height=16.0f;Collider.Position=new Vector2(0.0f,-8.0f);}

            //创建新的激光
            laser=new PeriodlyLaser(Position+dir*8.0f,dir,width_ratio)
            {
                generator = this
            };
            generator=DBBModule.SpriteBank.Create("LaserGenerator");
            generator.Rotation=angle;
            Depth=-15;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(generator);
            //要注意到激光与激光生成器的更新存在一帧的差距，这将导致位置上的不对应，且激光先更新
            //为此需要一种机制来对二者的位置进行同步
            scene.Add(laser);
        }

        public override void Update()
        {
            base.Update();
            //revert为false时，此时当标签存在时代表禁用
            if(revert==false)
            {
               if(!(Scene as Level).Session.GetFlag(laser_switch_label)){on=true;}
               else{on=false;}
            }
            //revert为true时，此时当标签不存在时代表禁用
            else
            {
               if(!(Scene as Level).Session.GetFlag(laser_switch_label)){on=false;}
               else{on=true;}
            }
            //如果未被禁用
            if(on==true)
            {
               laser_switch=true;
               //对于即将停止和停止状态，应当转到慢启动状态
               if(state==State.going_to_stop)
               {
                  state=State.going_to_move;
               }
               else if(state==State.stop)
               {
                  state=State.going_to_move;
               }
            }
            //如果被禁用了
            else
            {
               laser_switch=false;
               //对于慢启动和启动状态，应当转到即将停止的状态
               if(state==State.going_to_move)
               {
                  state=State.going_to_stop;
               }
               else if(state==State.moving)
               {
                  state=State.going_to_stop;
               }
            }
            laser.Switch(laser_switch);
            switch(state)
            {
               case State.going_to_move:GoingToMove();break;
               case State.moving:Moving();break;
               case State.going_to_stop:GoingToStop();break;
               case State.stop:Stop();break;
            }
            
        }
         private void GoingToMove()
         {
            acceleration+=Engine.DeltaTime*acceleration_cofficient;
            if(acceleration>1.0f)
            {
               acceleration=1.0f;
               state=State.moving;
            }
            if(go==true)
            {
               t_move+=speed*Engine.DeltaTime*acceleration;
               if(t_move>1.0f)
               {
                  t_move=1.0f;
                  go=false;
               }
            }
            else if(go==false)
            {
               t_move-=speed*Engine.DeltaTime*acceleration;
               if(t_move<0.0f)
               {
                  t_move=0.0f;
                  go=true;
               }
            }
            float real_t=DBBMath.MotionMapping(t_move,style);
            Vector2 temp_position=start_pos*real_t+(1.0f-real_t)*end_pos;
            MoveH((temp_position-Position).X);
            MoveV((temp_position-Position).Y);
            //转到发射激光的动画
            if(generator.CurrentAnimationID=="idle")
            {
               generator.Play("act",false,false);
            }
            else if(generator.CurrentAnimationID=="sustain")
            {
               generator.Play("sustain",false,false);
            }
         }
         private void Moving()
         {
            acceleration=1.0f;
            if(go==true)
            {
               t_move+=speed*Engine.DeltaTime;
               if(t_move>1.0f)
               {
                  t_move=1.0f;
                  go=false;
               }
            }
            else if(go==false)
            {
               t_move-=speed*Engine.DeltaTime;
               if(t_move<0.0f)
               {
                  t_move=0.0f;
                  go=true;
               }
            }
            float real_t=DBBMath.MotionMapping(t_move,style);
            Vector2 temp_position=start_pos*real_t+(1.0f-real_t)*end_pos;
            //solid移动时不要直接更改其Position，而是用MoveH和MoveV，不然人物与其交互时其移动无法对人物造成影响
            MoveH((temp_position-Position).X);
            MoveV((temp_position-Position).Y);
            if(generator.CurrentAnimationID=="idle")
            {
                generator.Play("act",false,false);
            }
            else if(generator.CurrentAnimationID=="sustain")
            {
               generator.Play("sustain",false,false);
            }
         }
         private void GoingToStop()
         {
            acceleration-=Engine.DeltaTime*acceleration_cofficient;
            if(acceleration<0.0f)
            {
               acceleration=0.0f;
               state=State.stop;
            }
            if(go==true)
            {
               t_move+=speed*Engine.DeltaTime*acceleration;
               if(t_move>1.0f)
               {
                  t_move=1.0f;
                  go=false;
               }
            }
            else if(go==false)
            {
               t_move-=speed*Engine.DeltaTime*acceleration;
               if(t_move<0.0f)
               {
                  t_move=0.0f;
                  go=true;
               }
            }
            float real_t=DBBMath.MotionMapping(t_move,style);
            Vector2 temp_position=start_pos*real_t+(1.0f-real_t)*end_pos;
            MoveH((temp_position-Position).X);
            MoveV((temp_position-Position).Y);
            //转到发射激光的动画
            if(generator.CurrentAnimationID=="sustain"||generator.CurrentAnimationID=="act")
            {
               generator.Play("idle",false,false);
            }
         }
         private void Stop()
         {
            generator.Play("idle",false,false);
            acceleration=0.0f;
         }
        //预更新，这是为了让激光的位置与激光器的位置同步
        public void VirtualUpdate()
        {
            Vector2 virtual_start_pos=start_pos;
            float virtual_t_move=t_move;
            float virtual_speed=speed;
            float virtual_acceleration=acceleration;
            bool virtual_go=go;
            if(virtual_go==true)
            {
               virtual_t_move+=virtual_speed*Engine.DeltaTime*virtual_acceleration;
               if(virtual_t_move>1.0f)
               {
                  virtual_t_move=1.0f;
               }
            }
            else if(virtual_go==false)
            {
               virtual_t_move-=virtual_speed*Engine.DeltaTime*virtual_acceleration;
               if(virtual_t_move<0.0f)
               {
                  virtual_t_move=0.0f;
               }
            }
            float virtual_real_t=DBBMath.MotionMapping(virtual_t_move,style);
            Vector2 virtual_temp_position=virtual_start_pos*virtual_real_t+(1.0f-virtual_real_t)*end_pos;
            laser.ChangePos(virtual_temp_position+dir*8.0f);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Rect(Collider.AbsolutePosition,Collider.Width,Collider.Height,Color.Azure);
        }
        public override void Render()
        {
            base.Render();
        }
    }
}