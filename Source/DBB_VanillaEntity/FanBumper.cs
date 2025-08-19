using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.DBBHelper.CustomizedCollider;
namespace Celeste.Mod.DBBHelper.Entities {
    
    [CustomEntity("DBBHelper/FanBumper")]
    public class FanBumper : Entity {
        private float radius=24.0f;//扇环内侧半径
        private float interval=12.0f;//内外半径间距
        private float start_theta=0.0f;//扇区起始角度，单位为PI
        private float delta_theta=0.0f;//扇区覆盖的角度，单位为PI
        
        private float rotate_velocity=0.0f;//扇环旋转速度，单位为PI
        private float timer=0.0f;//计时器，用于指示还有多久弹球才可恢复成可弹的状态
        private int segment_number=8;//外半圈的划分点的数目,因此扇区矩形数目应为segment_number-1
        private static ParticleType P_Launch=Cloud.P_Cloud;

        public SineWave sine;//用于引起物体的随机移动
        private Vector2 anchor;///用于引起物体的随机移动


        private MTexture texture_square=new MTexture();//绘制
        private MTexture texture_triangle=new MTexture();//绘制

        private List<Vector2>outer_polygon_point=new List<Vector2>();//扇环外圈点
        private List<Vector2> inner_polygon_point=new List<Vector2>();//扇环内圈点
        private Vector2 collider_absolute_position=new Vector2();

        public FanBumper(EntityData data,Vector2 offset):base(data.Position+offset)
        {
            anchor=Position;
            radius=data.Float("radius");
            interval=data.Float("interval");
            start_theta=data.Float("startTheta");
            delta_theta=data.Float("deltaTheta");
            rotate_velocity=data.Float("rotateSpeed");

            texture_square=GFX.Game["objects/DBB_Items/SpecialBumper/BSr"];
            texture_triangle=GFX.Game["objects/DBB_Items/SpecialBumper/BTr"];

            Collider = new FanCollider(Vector2.Zero,start_theta*(float)Math.PI,delta_theta*(float)Math.PI,radius,radius+interval,segment_number);
            collider_absolute_position=Collider.AbsolutePosition;
            outer_polygon_point=(Collider as FanCollider).OuterPolygonPoint;
            inner_polygon_point=(Collider as FanCollider).InnerPolygonPoint;

            Add(new PlayerCollider(OnPlayer));//碰撞事件
            Add(sine=new SineWave(0.44f, 0f).Randomize());//物体随机移动
            UpdatePosition();//物体随机移动
        }   
        public override void Added(Scene scene)
        {
            base.Added(scene); 
        }
        public override void Update()
        {
            base.Update();
            //扇环的旋转 
            if(rotate_velocity!=0.0f)
            {
                start_theta+=rotate_velocity*Engine.DeltaTime;
                start_theta%=2.0f;
                (Collider as FanCollider).Update(Vector2.Zero,start_theta*(float)Math.PI);
            }
            if(timer>0.0f)
            {
                timer-=Engine.DeltaTime;
                if(timer<=0.0f)
                {
                    Audio.Play("event:/game/06_reflection/pinballbumper_reset", Position);
                }
            }
            UpdatePosition();
            collider_absolute_position=Collider.AbsolutePosition;
        }
        public void UpdatePosition()
        {
            Position = new Vector2((float)((double)anchor.X + (double)sine.Value * 3.0), (float)((double)anchor.Y + (double)sine.ValueOverTwo * 2.0));
        }
        public override void Render()
        {
            //(Collider as FanCollider).Render(Color.GreenYellow,Color.Red);
            float inner_length=0.0f;
            float triangle_outer_length=0.0f;
            float square_angle=0.0f;
            float start_angle=0.0f;
            float end_angle=0.0f;
            float divided_angle=delta_theta/(segment_number-1);
            Vector2 square_center=Vector2.Zero;
            Vector2 start_triange_center=Vector2.Zero;
            Vector2 end_triange_center=Vector2.Zero;

            float interval_fix=16.0f+interval/6.0f;
            float width_fix=14.0f+interval/6.0f;

            
            for(int i=0;i<segment_number-1;i++)
            {
                start_angle=divided_angle*i+start_theta;
                end_angle=divided_angle*(i+1)+start_theta;
                square_angle=(start_angle+end_angle)*0.5f;

                start_triange_center=(outer_polygon_point[i]+inner_polygon_point[i])*0.5f;
                end_triange_center=(outer_polygon_point[i+1]+inner_polygon_point[i+1])*0.5f;
                square_center=(start_triange_center+end_triange_center)*0.5f;

                inner_length=(inner_polygon_point[i+1]-inner_polygon_point[i]).Length();
                triangle_outer_length=((outer_polygon_point[i+1]-outer_polygon_point[i]).Length()-inner_length)*0.5f;
                texture_square.DrawCentered(square_center+collider_absolute_position,Color.White*(1.0f-timer),new Vector2(interval/interval_fix,inner_length/width_fix),-square_angle*(float)Math.PI);             
                texture_triangle.DrawCentered(start_triange_center+collider_absolute_position,Color.White*(1.0f-timer),new Vector2(interval/interval_fix,triangle_outer_length/width_fix),-square_angle*(float)Math.PI);
                texture_triangle.DrawCentered(end_triange_center+collider_absolute_position,Color.White*(1.0f-timer),new Vector2(interval/interval_fix,triangle_outer_length/width_fix),-square_angle*(float)Math.PI,Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically);
            }
        }
        public void OnPlayer(Player player)
        {
            if(timer>0.0f)
            {
                return;
            }
            //计算人物到扇形圆心的距离，以此来确定弹开人物的方向
            //默认参考点为扇形的圆心，此时默认与扇区外侧碰撞
            Vector2 pos=Position;
            Vector2 dir=player.Center-Position;
            float tmp_inner_radius=(Collider as FanCollider).InnerRadius;
            float tmp_outer_radius=(Collider as FanCollider).OuterRadius;


            float tmp_start_theta=((Collider as FanCollider).StartTheta)%(2.0f*(float)(Math.PI));
            float tmp_end_theta=((Collider as FanCollider).EndTheta)%(2.0f*(float)(Math.PI));
            float delta_theta=Math.Abs(tmp_end_theta-tmp_start_theta);

            float length=dir.Length();
            float ref_distance=tmp_inner_radius+(Collider as FanCollider).Height*0.5f;

            Vector2 start_n=new Vector2((float)Math.Cos(tmp_start_theta),-(float)Math.Sin(tmp_start_theta));
            Vector2 end_n=new Vector2((float)Math.Cos(tmp_end_theta),-(float)Math.Sin(tmp_end_theta));
            Vector2 mid_n=new Vector2();
            bool bound=false;
            //如果起始角度小于终止角度
            if(tmp_end_theta>=tmp_start_theta)
            {
                mid_n=DBBMath.Rotate(Vector2.Zero,start_n,-delta_theta/2.0f,false);
                if(length>tmp_inner_radius && length<tmp_outer_radius)
                {
                    //如果在中间轴和起始轴的右侧
                    if(DBBMath.Cross(mid_n,dir)>0 && DBBMath.Cross(start_n,dir)>0)
                    {
                        //与起始轴相撞
                        pos+=new Vector2((float)Math.Cos(tmp_start_theta+delta_theta*0.02f),-(float)Math.Sin(tmp_start_theta+delta_theta*0.02f))*length;
                        bound=true;
                    }
                    //如果在中间轴和终止轴的左侧
                    else if(DBBMath.Cross(mid_n,dir)<0 && DBBMath.Cross(end_n,dir)<0)
                    {
                        //与终止轴相撞
                        pos+=new Vector2((float)Math.Cos(tmp_end_theta-delta_theta*0.02f),-(float)Math.Sin(tmp_end_theta-delta_theta*0.02f))*length;
                        bound=true;
                    }
                }
            }
            //如果起始角度大于终止角度
            else if(tmp_end_theta<tmp_start_theta)
            {
                mid_n=DBBMath.Rotate(Vector2.Zero,end_n,-delta_theta/2.0f,false);

                if(length>tmp_inner_radius && length<tmp_outer_radius)
                {
                    //如果在中间轴的左侧和起始轴的右侧
                    if(DBBMath.Cross(mid_n,dir)<0 && DBBMath.Cross(start_n,dir)>0)
                    {
                        //与起始轴相撞
                         pos+=new Vector2((float)Math.Cos(tmp_start_theta+(2.0f*Math.PI-delta_theta)*0.02f),-(float)Math.Sin(tmp_start_theta+(2.0f*Math.PI-delta_theta)*0.02f))*length;
                         bound=true;
                    }
                    //如果在中间轴的右侧和终止轴的左侧
                    else if(DBBMath.Cross(mid_n,dir)>0 && DBBMath.Cross(end_n,dir)<0)
                    {
                        //与终止轴相撞
                        pos+=new Vector2((float)Math.Cos(tmp_end_theta-(2.0f*Math.PI-delta_theta)*0.02f),-(float)Math.Sin(tmp_end_theta-(2.0f*Math.PI-delta_theta)*0.02f))*length;
                        bound=true;
                    }
                }
            }
            //如果小于内半径，代表与扇区内侧碰撞，因此要改变参考点
            if(bound==false && length<ref_distance && length>0)
            {
                pos+=Vector2.Normalize(dir)*(Collider as FanCollider).OuterRadius;
            }
            timer=0.6f;
            Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);
            //Vector2 vector2 = player.ExplodeLaunch(pos, snapUp: false, sidesOnly: false);
            Vector2 vector2 =ExplodeLaunch(player,pos);
            SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
        }
        private Vector2 ExplodeLaunch(Player player,Vector2 from)
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Celeste.Freeze(0.1f);
            player.launchApproachX = null;
            Vector2 vector = (player.Center-from).SafeNormalize(-Vector2.UnitY);

            player.Speed=280.0f*vector;

            if (player.Speed.Y <= 50f)
            {
                player.Speed.Y = Math.Min(-150f, player.Speed.Y);
                player.AutoJump = true;
            }
            
            if (player.Speed.X != 0f)
            {
                if (Input.MoveX.Value == Math.Sign(player.Speed.X))
                {
                    player.explodeLaunchBoostTimer = 0f;
                    player.Speed.X *= 1.2f;
                }
                else
                {
                    player.explodeLaunchBoostTimer = 0.01f;
                    player.explodeLaunchBoostSpeed = player.Speed.X * 1.2f;
                }
            }

            SlashFx.Burst(player.Center, player.Speed.Angle());
            if (!player.Inventory.NoRefills)
            {
                player.RefillDash();
            }

            player.RefillStamina();
            player.dashCooldownTimer = 0.2f;
            player.StateMachine.State = 7;
            return vector;
        }
       
    }

    
}