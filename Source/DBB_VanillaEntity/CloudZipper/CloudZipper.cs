using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities 
{
    [CustomEntity("DBBHelper/CloudZipper")]
    
     class CloudZipper:JumpThru{
        //云自身的属性
        public  ParticleType s_PCloud=Cloud.P_Cloud;
        public  ParticleType sPFragileCloud=Cloud.P_FragileCloud;
        public Sprite m_sprite;//设置云的外貌
        public Wiggler m_wiggler;
        public ParticleType m_particleType;//云的粒子效果
        public SoundSource m_sfx;//云的音效
        private bool b_waiting = true;//云弹跳等待状态
        private bool b_down=false;//云弹跳下降状态
        private bool b_up=false;//云弹跳上升状态
        private bool b_back=false;//云弹跳返回状态
        //----------------------------------------------------
        private float m_startY;//云的基准点，在此不会用到，但是保留
        private float m_respawnTimer;//云的重生计时器
        private bool b_fragile;//指定云是否为碎片云
        private float m_respawnTime=2.0f;//指定碎片云的重生时间
        private float m_timer;//云本身运动的计时器
        private Vector2 m_scale;//云的大小
        private bool b_canRumble;//指示云是否要碎掉
        private bool? b_small;//指示云是否为小云
        private float m_rate=1.0f;//云的更新速度
        private string m_downstyle="easeOutQuard";//云下降时的方式
        private string m_upstyle="easeOutSin";//云上升时的方式
        private bool b_jump_mode=true;//弹跳模式，云上升时是否会将玩家弹起
        private float m_jump_speed=-200.0f;//用于弹跳模式,若弹跳模式开,则云上升时玩家被赋予的额外速度
        private float m_deltaY=0.0f;//控制云的自身弹跳偏移
        private float m_bounceTime=0.0f;//控制云的弹跳速度
        
        //以下控制云的Zip移动
        private List<Vector2>m_node_pos=new List<Vector2>();//云各个节点的位置
        private List<Vector2> m_direction=new List<Vector2>();//云移动方向
        private Vector2 m_moveOffset;//云移动偏移量暂存器
        private List<float> m_distance=new List<float>();//起始到终止的距离
        private int m_path_count=0;//路径总共的条数
        private int m_path_index=0;//当前路径的序号，规定从节点0到节点1的路径为0号路径，以此类推

        private bool b_path_can_move=false;//云是否可以真的移动，这在控制云的启动条件时有用
        private bool b_pathmove=false;//云移动状态
        private bool b_pathback=false;//云移动返回状态
        private float m_pathtimer=0.0f;//云移动的计时器
        private float m_gotime=1.0f;//云移动过去的用时
        private float m_backtime=1.0f;//云移动回来的用时
        private string m_gostyle="Linear";//云移动过去的方式
        private string m_backstyle="Linear";//云移动回来的方式
        private bool b_fragile_pathinit=false;//碎片云重置移动状态的标志，当云碎掉时触发
         //云移动的所有方式
        public static readonly string[]m_allstyle={
            "Linear",
            "easeInSin","easeOutSin","easeInOutSin",
            "easeInCubic","easeOutCubic","easeInOutCubic",
            "easeInQuard","easeOutQuard","easeInOutQuard",
            "easeInBack","easeOutBack","easeInOutBack"
            };
        
        //云的轨迹绘制
        private CloudZipperPath m_cloudpath;//云的Zipper轨迹
        private Vector2 m_refPosPoint=new Vector2(0.0f,0.0f);//云的位置的参考点
        private int m_trail_point_num=16;//云的轨迹点绘制数目
        private float m_trail_point_bloomRadius=12.0f;//轨迹点泛光半径
        private int m_trail_point_LightStartFade=12;//轨迹点点光源衰弱起始半径
        private int m_trail_point_LightEndFade=16;//轨迹点点光源衰弱结束半径

        //以下为接口
        public List<Vector2> i_node_pos{get{return m_node_pos;}}
        public Vector2 i_refPosPoint{get{return m_refPosPoint;}}
        //----------------------------------------------------
        public float i_respawnTimer{get{return m_respawnTimer;}}
        public float i_respawnTime{get{return m_respawnTime;}}
        //----------------------------------------------------
        public bool i_fragile{get{return b_fragile;}}
        public bool i_small{get{return b_small.Value;}}
        //----------------------------------------------------
        public int i_trail_point_num{get{return m_trail_point_num;}}
        public float i_trail_point_bloomRadius{get{return m_trail_point_bloomRadius;}}
        public int i_trail_point_LightStartFade{get{return m_trail_point_LightStartFade;}}
        public int i_trail_point_LightEndFade{get{return m_trail_point_LightEndFade;}}

        public CloudZipper(Vector2 position, bool b_fragile):base(position,32,false)
        {
            Depth=-2;//尽管在这里对Depth进行了设置，实际的更新是在base.Added函数完成的，因此要在Added里面加上base.Added
            m_refPosPoint=position;
            this.b_fragile=b_fragile;
            m_startY=position.Y;
            Collider.Position.X=-16f;
            m_timer=Calc.Random.NextFloat() * 4f;
            Add(m_wiggler=Wiggler.Create(0.3f, 4f));
            m_particleType=b_fragile ? sPFragileCloud : s_PCloud;
            SurfaceSoundIndex=4;
            Add(new LightOcclude(0.2f));
            m_scale=Vector2.One;
            Add(m_sfx = new SoundSource());
            //核心函数之一，用于控制云移动
            Add(new Coroutine(CloudPositionUpdate()));
        }
        public CloudZipper(EntityData data,Vector2 offset):this(data.Position+offset,data.Bool("fragile"))
        {
            //一些云的通用属性;
            b_small=data.Bool("small");
            m_respawnTime=data.Float("respawntime");
            //控制云的运动的参数
            m_gotime=data.Float("gotime");
            m_backtime=data.Float("backtime");
            m_gostyle=data.Attr("gostyle");
            m_backstyle=data.Attr("backstyle");
            m_downstyle=data.Attr("downstyle");
            m_upstyle=data.Attr("upstyle");
            b_jump_mode=data.Bool("jumpMode");
            if(!m_allstyle.Contains(m_gostyle)){m_gostyle="Linear";}
            if(!m_allstyle.Contains(m_backstyle)){ m_backstyle="Linear";}
            if(!m_allstyle.Contains(m_downstyle)){m_downstyle="Linear";}
            if(!m_allstyle.Contains(m_upstyle)){ m_upstyle="Linear";}
            m_rate=data.Float("CloudUpdateRate");
            //控制云的位置和运动角度的参数
            m_node_pos.Add(data.Position+offset);//添加云初始节点的位置
            for(int i=0;i<data.Nodes.Length;i++)//添加云节点的位置，没有节点项的话Nodes为空，否则第一个节点的位置是Nodes[0]
            {
                Vector2 temp=data.Nodes[i]+offset-m_node_pos[i];
                m_node_pos.Add(data.Nodes[i]+offset);
                m_distance.Add(temp.Length());//相邻节点的距离
                m_direction.Add(DBBMath.Normalize(temp));//相邻节点的方向
            }
            m_moveOffset=new Vector2(0.0f,0.0f);
            m_path_count=m_distance.Count+1;
            //Logger.Log(LogLevel.Info,"DBBHelper/CloudZipper",m_path_count.ToString());
            if(b_jump_mode==false)//云上升时的弹跳模式以及其速度设置
            {
                m_jump_speed=0.0f;
            }
            //控制云Zipper轨迹的变量
            m_trail_point_num=data.Int("TrailPointNum");
            m_trail_point_bloomRadius=data.Float("TrailPointBloomRadius");
            m_trail_point_LightStartFade=data.Int("TrailPointLightStartFade");
            m_trail_point_LightEndFade=data.Int("TrailPointLightEndFade");
            m_cloudpath =new CloudZipperPath(this);

         }
        public override void Added(Scene scene)
        {   
            //这一句必须要加，这里是更新实际深度的，另外加上这个函数里面也执行了this.Scene=scene
            base.Added(scene);
            
            //把云的Added复制过来
            string text=b_fragile ? "cloudFragile" : "cloud";
            if (IsSmall(SceneAs<Level>().Session.Area.Mode!=0))
            {
                Collider.Position.X += 2f;
                Collider.Width -= 6f;
                text += "Remix";
            }
            Add(m_sprite=GFX.SpriteBank.Create(text));
            m_sprite.Origin=new Vector2(m_sprite.Width / 2f, 8f);
            m_sprite.OnFrameChange=(string s) =>
            {
                if (s == "spawn" && m_sprite.CurrentAnimationFrame == 6){m_wiggler.Start();}
            };
            //添加轨迹绘制

        //这里有个微妙的事情，你可以在这里调用scene.Add(m_cloudpath)，也可以调用m_cloudpath.Added(scene)，它们的作用不同
        //scene.Add(m_cloudpath)的作用是向场景中添加这个实体，本质上建立了由场景到实体的映射，是告诉场景：你有我这个实体，这时候可以通过调用scene.Entities或者Scene.Tracker中和Entity相关的函数来获取这个实体
        //m_cloudpath.Added(scene)的作用是向实体的组件告知自己属于某个场景，本质上建立了由组件到场景的映射，这时候可以通过调用scene.Tracker中和Component相关的函数来获取这个实体
        //当你调用scene.Add(m_cloudpath)的时候m_cloudpath.Added(scene)会自动被调用，反之并不会
            scene.Add(m_cloudpath);
        }
        //云自身运动
        //不要问我为什么不用云的速度来写代码，问就是写Bug
        private void CloudStart()
        {
            if (b_waiting==true)
            {
                Player playerRider=GetPlayerRider();
                if (playerRider != null && playerRider.Speed.Y >= 0f)
                {   
                    if(b_pathmove==false&&b_pathback==false)
                    {
                        b_path_can_move=true;
                    }
                    b_canRumble=true;
                    m_bounceTime=0.0f;
                    m_scale=new Vector2(1.3f,0.7f);
                    if (b_fragile){Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);}
                    else{Audio.Play("event:/game/04_cliffside/cloud_blue_boost", Position);}
                    b_waiting=false;
                    b_down=true;
                }
            }
        }
        private void CloudDown()
        {
            if(b_down==true)
            {   
                if (m_deltaY<12.0f)
                {
                    m_bounceTime=Math.Min(1.0f,m_bounceTime+7.0f*Engine.DeltaTime*m_rate);
                    m_deltaY=12.0f*DBBMath.MotionMapping(m_bounceTime,m_downstyle);
                }
                else
                {
                    m_bounceTime=0.0f;
                    b_down=false;
                    b_up=true;
                }
            }
        }
        private void CloudUp()
        {
            if(b_up==true)
            {   
                Player playerRider2 = GetPlayerRider();
                if(b_canRumble==true)
                {
                    b_canRumble=false;
                    if (HasPlayerRider())
                    {
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    }
                }
                if(b_fragile && (HasPlayerRider()==false))
                {
                    Collidable=false;
                    m_sprite.Play("fade");
                    m_respawnTimer=m_respawnTime;
                    m_scale=new Vector2(0.7f,1.3f);
                    b_up=false;
                    b_back=true;
                }
                if(m_deltaY>-18.0f)
                {
                    m_bounceTime=Math.Min(1.0f,m_bounceTime+2.9f*Engine.DeltaTime*m_rate);
                    m_deltaY=(float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(m_bounceTime,m_upstyle),12.0f,-18.0f);
                    if(m_deltaY<-8.0f)
                    {
                        if(playerRider2!=null && playerRider2.Speed.Y>=0f)
                        {
                            playerRider2.Speed.Y=m_jump_speed;
                        }
                    }
                }
                else
                {
                    m_scale=new Vector2(0.7f,1.3f);
                    b_up=false;
                    b_back=true;
                }
                
            }
        }
        private void CloudBack()
        {
            if(b_back==true)
            {   
                //如果勾选了JumpMode但是玩家迟迟不离开云,则云会在上升返回时自动碎裂并给予玩家一个小的弹跳速度
                if(b_fragile && Collidable && (b_jump_mode==false))
                {
                    Collidable=false;
                    m_sprite.Play("fade");
                    m_respawnTimer=m_respawnTime;
                    m_scale=new Vector2(0.7f,1.3f);
                    b_up=false;
                    b_back=true;
                }
                if(m_deltaY!=0.0f)
                {
                    m_deltaY=Math.Min(0.0f,m_deltaY+Engine.DeltaTime*75.0f*m_rate);
                }
                else
                {
                    b_back=false;
                    b_waiting=true;
                }
            }
        }
        private void CloudIdle()
        {
            m_scale.X = Calc.Approach(m_scale.X, 1f, 1f * Engine.DeltaTime);
            m_scale.Y = Calc.Approach(m_scale.Y, 1f, 1f * Engine.DeltaTime);
            m_timer += Engine.DeltaTime;
            if (GetPlayerRider() != null)
            {
                m_sprite.Position = Vector2.Zero;
            }
            else
            {
                m_sprite.Position = Calc.Approach(m_sprite.Position, new Vector2(0f, (float)Math.Sin(m_timer * 2f)), Engine.DeltaTime * 4f);
            }
            if(m_respawnTimer > 0.0f)
            {
                m_respawnTimer-=Engine.DeltaTime;
                if(m_respawnTimer <= 0.0f)
                {
                    b_waiting = true;
                    b_fragile_pathinit=true;
                    m_deltaY=0.0f;
                    m_bounceTime=0.0f;
                    m_scale = Vector2.One;
                    Collidable = true;
                    m_sprite.Play("spawn");
                    m_sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
                }
            }
        }
        /// <summary>
        /// 指示云是否是小云
        /// </summary>
        public bool IsSmall(bool value)
        {
            return b_small.GetValueOrDefault(value);
        }
       
        /// <summary>
        /// 更新云的位置
        /// </summary>
        private IEnumerator<float> CloudPositionUpdate()
        {
           while(true)
           {


                if(b_fragile_pathinit==true)
                {
                    b_fragile_pathinit=false;
                    b_pathmove=false;
                    m_path_index=0;
                    m_pathtimer=0.0f;
                    m_startY=m_node_pos[0].Y;
                    m_moveOffset=m_node_pos[0];
                    Position=m_node_pos[0];
                    m_refPosPoint=m_node_pos[0];
                    yield return 0.0f;
                    continue;
                }            
                //玩家第一次碰到云的时候启动云的移动
                if(b_path_can_move==true)
                {
                    b_path_can_move=false;
                    b_pathmove=true;
                }
                else
                {
                    yield return 0.0f;
                    continue;
                }
                //云在移动中且云还不返回时
                while(b_pathmove==true)
                {
                    //如果碎片云已经处于碎片后的状态，应该直接回到重置所有移动状态
                    if(b_fragile_pathinit==true)
                    {
                        b_fragile_pathinit=false;
                        b_pathmove=false;
                        m_path_index=0;
                        m_pathtimer=0.0f;
                        m_startY=m_node_pos[0].Y;
                        m_moveOffset=m_node_pos[0];
                        Position=m_node_pos[0];
                        m_refPosPoint=m_node_pos[0];
                        yield return 0.0f;
                        continue;
                    }
                    //云还不是返回的时候
                    m_moveOffset=m_node_pos[m_path_index]+m_direction[m_path_index]*m_distance[m_path_index]*DBBMath.MotionMapping(m_pathtimer,m_gostyle);
                    m_pathtimer+=Engine.DeltaTime/m_gotime;
                    //更新云的位置
                    MoveTo(m_moveOffset+m_deltaY*Vector2.UnitY);  
                    m_startY=m_moveOffset.Y;
                    m_refPosPoint=m_moveOffset;
                    //计时器达到指定值之后路径数加一，代表进入下一条路径
                    if(m_pathtimer>1.0)
                    {
                        m_pathtimer=0.0f;
                        m_path_index++;
                        //如果最后一条路径的计时器达到指定值，云开始往回走
                        if(m_path_index==m_path_count-1)
                        {
                            b_pathmove=false;
                            b_pathback=true;
                            m_moveOffset=m_node_pos[m_path_index];
                            m_startY=m_moveOffset.Y;
                            m_refPosPoint=m_moveOffset;
                        }
                    } 
                    yield return 0.0f;
                    
                }
                //云在移动且返回的时候
                while(b_pathback==true)
                {
                    //如果碎片云已经处于碎片后的状态，应该直接回到重置所有移动状态
                    if(b_fragile_pathinit==true)
                    {
                        b_fragile_pathinit=false;
                        b_pathback=false;
                        m_path_index=0;
                        m_pathtimer=0.0f;
                        m_startY=m_node_pos[0].Y;
                        m_moveOffset=m_node_pos[0];
                        Position=m_node_pos[0];
                        m_refPosPoint=m_node_pos[0];
                        yield return 0.0f;
                        continue;
                    }
                    m_moveOffset=m_node_pos[m_path_index]-m_direction[m_path_index-1]*m_distance[m_path_index-1]*DBBMath.MotionMapping(m_pathtimer,m_backstyle);
                    m_pathtimer+=Engine.DeltaTime/m_backtime;
                    //更新云的位置
                    MoveTo(m_moveOffset+m_deltaY*Vector2.UnitY);  
                    m_startY=m_moveOffset.Y;
                    m_refPosPoint=m_moveOffset;
                    //计时器达到指定值之后路径数减一，代表回退到前一条路径
                    if(m_pathtimer>1.01f)
                    {
                        m_pathtimer=0.0f;
                        m_path_index--;
                        //如果第一条路径的计时器达到指定值，代表一轮移动完成
                        if(m_path_index==0)
                        {
                            b_pathback=false;
                            m_moveOffset=m_node_pos[0];
                            m_startY=m_moveOffset.Y;
                            m_refPosPoint=m_moveOffset;
                        }
                       
                    }  
                    yield return 0.0f;
                }
                //如果云返回后云还处在上下运动的状态，应该等待云的上下运动完成才能开始新一轮的云的位置移动
                while(b_path_can_move==false)
                {
                    //如果碎片云已经处于碎片后的状态，应该直接回到重置所有移动状态
                    if(b_fragile_pathinit==true)
                    {
                        b_fragile_pathinit=false;
                        m_pathtimer=0.0f;
                        m_path_index=0;
                        m_startY=m_node_pos[0].Y;
                        m_moveOffset=m_node_pos[0];
                        Position=m_node_pos[0];
                        m_refPosPoint=m_node_pos[0];
                        yield return 0.0f;
                        continue;
                    }
                    //Logger.Log(LogLevel.Warn,"m_path_index",m_path_index.ToString());
                    //Logger.Log(LogLevel.Warn,"b_pathmove",b_pathmove.ToString());
                    //Logger.Log(LogLevel.Warn,"b_pathback",b_pathback.ToString());
                    MoveTo(m_moveOffset+m_deltaY*Vector2.UnitY); 
                    yield return 0.0f;
                }
            }
        }
        public override void Update()
        {
        //虽然前面说到scene.Add(m_cloudpath)的作用，但是这个其实十分笼统，一个实体被添加到场景并被更新和渲染的工作流程应该是这样的
        //1.首先调用Scene中Entities列表的Add
        //2.Entities.Add会把这些要添加的实体添加到Entities.toadd里面，toadd顾名思义就是将要添加进去但是还没有添加
        //3.场景的Update会调用所有已添加的(toadd里面的内容不算已添加)Entity的Update，Render同理
        //4.在Scene.Update之前其实有一个Scene.BeforeUpdate，它负责调用Entities.UpdateLists
        //5.Entities.UpdateLists会检查自身toadd列表里面有没有内容，如果有，就根据它们的一些属性将它们添加到已添加列表中（此时建立场景到实体的映射）
        //6.Entities.UpdateLists同时完成对每个要添加到已添加列表的实体的自身的Added调用
        //7.Entities.UpdateLists同时完成对场景的已添加实体列表的EntityAdded调用，而EntityAdded将当前要添加的实体添加到当前场景的Tracker对象的Entities列表中（此时建立了场景Tracker到实体的映射）
        //7.如果你的代码比较规范的话(添加了base.Added)，实体的Added会调用Entity.Added，它负责了将该实体的组件添加到当前场景的Tracker对象的Components列表中（此时建立了场景Tracker到实体的组件的映射）
        //8.Entity.Added还负责了其他工作，例如设置当前实体的场景属性值(建立了实体到场景的映射)，并更新了一次实体的深度
        //所以说，一个Add配合着BeforeUpdate完成了这么多的映射工作：
        //Ⅰ.实体可以找到自己属于哪个场景了
        //Ⅱ.场景知道自己有哪些实体了
        //Ⅲ.场景Tracker知道自己有哪些实体了
        //Ⅳ.场景Tracker知道自己有哪些组件了
        //Test:运行以下注释代码，你将会在控制台看到Celeste.Level的输出，但是如果你把这些代码放到Added里面去执行，将不会得到任何输出
        //验证目的：验证Add将实体添加进入了toadd列表而非直接添加到Scene.Entities中，同时验证了Update之前会进行BeforeUpdate，将toadd列表的内容送往Scene.Entities
        //Tip:在Update中运行大量的控制台输出往往是性能炸弹，你可以先在游戏场景中按下ESC进行暂停（此时不会更新Update），然后取消暂停，观察控制台输出的结果，同时观察这种行为会对性能造成多大的影响
            /*
             foreach(var e in Scene.Entities.FindAll<CloudZipperPath>())
            {
               
                if(e.Scene==null)
                {
                     Logger.Log(LogLevel.Warn,"Cloud test","NULL!!!");
                }
                else
                {
                    Logger.Log(LogLevel.Warn,"Cloud test",e.Scene.ToString());
                }
            }
            */
            base.Update();
            CloudIdle();
            CloudStart();
            CloudDown();
            CloudUp();
            CloudBack();
            //更新cloudpath的一些逻辑，这个函数并不会自动被调用
            int temp_path_index=m_path_index;
            if(b_pathback)
            {
                temp_path_index--;
            }
            m_cloudpath.Update(m_refPosPoint,temp_path_index,b_pathback,m_respawnTimer,b_fragile_pathinit);
        }  
        public override void Render()
        {   
            //绘制云
            Vector2 vector = m_scale;
            vector *= 1f + 0.1f * m_wiggler.Value;
            m_sprite.Scale = vector;
            base.Render();
            //绘制云的轨迹，不要在这里重复绘制，之前已经把cloudpath添加进场景中了
            //m_cloudpath.Render();
        }
        
     }

}
