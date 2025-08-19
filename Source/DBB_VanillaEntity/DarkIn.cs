using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {
    [CustomEntity("DBBHelper/DarkIn")]
    class DarkIn:Entity{
        private float m_fadetime=5.0f;//渐变时间
        private float m_fadetimer=5.0f;//计时器
        private Player m_player=null;//记录场景的人物
        private VertexLight m_light;//离人物最近的光源
        private static float m_startRadius=32.0f;//记录人物光源起始半径
        private static float m_endRadius=64.0f;//记录人物光源终止半径
        private bool b_single=false;//希望一个场景中只有一个实例生效，为此需要用一个标志来判断
        private float m_descend_rate=1.0f;//计时器减小时的速率
        private float m_ascend_rate=2.0f;//计时器增加时的速率
        public DarkIn(EntityData data,Vector2 offset)
        {
            Position=data.Position+offset;
            m_fadetime=data.Float("fadetime");
            m_descend_rate=data.Float("descendRate");
            m_ascend_rate=data.Float("ascendRate");
            m_fadetimer=m_fadetime;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            List<Entity>temp=Scene.Entities.ToAdd;
            //如果场景中已经有DarkIn实例了就不要让这个实例生效了
            foreach(var entity in temp)
            {
                if(entity is DarkIn)
                {  
                    if(!entity.Equals(this))
                    {
                        b_single=true;
                        Active=false;
                        Logger.Log(LogLevel.Warn,"DBBHelper","A DarkIn object has existed in this scene.");
                        Logger.Log(LogLevel.Warn,"DBBHelper","This DarkIn object won't take effect.");
                    }
                    break;
                }
            }
            
        }
        
        private VertexLight GetNearestVertexLight()
        {
            //如果没有人物那么也没有必要获取光源
            if(m_player==null)
            {
                return null;
            }
            float distance=float.MaxValue;
            VertexLight final_light=null;
            //获取场景所有光源
            List<Component> all_lights=Scene.Tracker.GetComponents<VertexLight>();
            foreach(var light in all_lights)
            {
                //如果是人物的光源，忽略
                if(light.Entity==m_player)
                {
                    continue;
                }
                //如果不是人物的光源
                //如果光源的真实位置(光源所对应的实体位置加上光源的位置偏移量)与人物的位置的距离是最小的，则最终返回的光源暂定为它
                if((light.Entity.Position+(light as VertexLight).Position-m_player.Position).Length()<distance)
                {
                    distance=(light.Entity.Position-m_player.Position).Length();
                    final_light=light as VertexLight;
                }
            }
            //返回最终光源或者NULL
            return final_light;
        }
        public override void Update()
        {
            base.Update();
            
            //只让第一个DarkIn实例生效
            if(b_single==false)
            {
                m_player=Scene.Tracker.GetEntity<Player>();
            }

            //Logger.Log(LogLevel.Warn,"dark",m_fadetimer.ToString());
            
        //Bug修复：在玩家由于其他的机制触发Die时，如果死亡过程中倒计时恰好结束会再次触发Die事件，这时候概率会引起游戏崩溃
        //玩家调用Die函数的时候分两帧完成工作，第一帧是把玩家对象放到scene.toremove列表中并且移除掉它的所有组件，并且把它的scene设置为null
        //第二帧才是正式地将玩家对象从scene中清除掉
        //由于第一帧的时候并没有清除掉玩家对象，所以要在这里要做好检测，不能让玩家再次调用Die
        //其实Die本身做的很好，就算你这里代码有漏洞，那里也会帮你处理掉
        //但是你奈何不住有别的代码MOD在你的Player.Die后面挂钩子啊，它们调不调用那些已经被归为NULL的对象（比如scene）就不好说了（比如：XaphanHelper.Drone :)
            if(m_player==null||m_player.Dead)
            {
                return;
            }
            float player_endRadius=m_player.Light.endRadius;
            //获取距离人物最近的光源
            m_light=GetNearestVertexLight();
            //如果没有的话，计时结束后人物会Die
            if(m_light==null)
            {
                m_fadetimer-=m_descend_rate*Engine.DeltaTime;
                //如果人物光源开着，则削减人物光源半径
                if(m_player.Light.Active==true)
                {
                    m_player.Light.startRadius=Math.Max(0.0f,m_player.Light.startRadius-m_descend_rate*2.0f*m_startRadius/m_fadetime*Engine.DeltaTime);
                    m_player.Light.endRadius=Math.Max(0.0f,m_player.Light.endRadius-m_descend_rate*m_endRadius/m_fadetime*Engine.DeltaTime);
                }
                //重新设置初始值并让人物死亡
                if(m_fadetimer<0.0f)
                {
                    m_fadetimer=m_fadetime;
                    m_player.Die(new Vector2((m_player.Facing==Facings.Right)?1.0f:-1.0f,0.0f));
                }
                return;
            }
            //如果有光源，则获取光源最远半径
            float nearest_object_endRadius=m_light.endRadius;
            //如果光源(要获取真实位置)和人物光源相交
            if((m_light.Entity.Position+m_light.Position-m_player.Position).Length()<player_endRadius+nearest_object_endRadius)
            {   //让各个变化值逐渐加回到它们的初始值
                m_fadetimer=Math.Min(m_fadetime,m_fadetimer+m_ascend_rate*Engine.DeltaTime);
                m_player.Light.startRadius=Math.Min(m_startRadius,m_player.Light.startRadius+m_startRadius/m_fadetime*m_ascend_rate*2.0f*Engine.DeltaTime);
                m_player.Light.endRadius=Math.Min(m_endRadius,m_player.Light.endRadius+m_endRadius/m_fadetime*m_ascend_rate*Engine.DeltaTime);
            }
            //如果光源和人物光源没有相交
            else
            {
                m_fadetimer-=m_descend_rate*Engine.DeltaTime;
                //如果人物光源开着，则削减人物光源半径
                if(m_player.Light.Active==true)
                {
                    m_player.Light.startRadius=Math.Max(0.0f,m_player.Light.startRadius-m_descend_rate*2.0f*m_startRadius/m_fadetime*Engine.DeltaTime);
                    m_player.Light.endRadius=Math.Max(0.0f,m_player.Light.endRadius-m_descend_rate*m_endRadius/m_fadetime*Engine.DeltaTime);
                }
                //重新设置初始值并让人物死亡
                if(m_fadetimer<0.0f)
                {
                    m_fadetimer=m_fadetime;
                    m_player.Die(new Vector2((m_player.Facing==Facings.Right)?1.0f:-1.0f,0.0f));
                }
                return;
            }
        }
        //在场景过渡时恢复人物光源
        public override void Removed(Scene scene)
        {
            //如果场景中有一个实例了它就不要再生效了
            if(b_single==false)
            {
                m_player=Scene.Tracker.GetEntity<Player>();
            }
            //对人物光源重新设置
            if(m_player!=null)
            {
                m_fadetimer=m_fadetime;
                m_player.Light.startRadius=m_startRadius;
                m_player.Light.endRadius=m_endRadius;
            }
            base.Removed(scene);
            
        }
    }

}