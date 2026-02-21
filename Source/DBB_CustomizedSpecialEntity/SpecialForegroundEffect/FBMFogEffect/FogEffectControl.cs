using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/FogEffectControl")]
    public class FogEffectControl:Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area=new Rectangle();//控制特效区域的范围
        private string area_control_mode="Left_to_Right";//区域的控制方式
        private string label="Default";//区域所控制的特效是哪一个
        //------------------可以更改的特效性质------------------
        private float velocityX_start=0.1f;//流体流速
        private float velocityX_end=0.1f;//流体流速
        private float velocityY_start=0.0f;//流体流速
        private float velocityY_end=0.0f;//流体流速
        private string velocity_control_mode="Linear";

        private float amplify_start=0.5f;//流体强度
        private float amplify_end=0.5f;//流体强度
        private string amplify_control_mode="Linear";

        private float frequency_start=2.0f;//流体细节度
        private float frequency_end=2.0f;//流体细节度
        private string frequency_control_mode="Linear";

        private float light_influence_coefficient_start=1.0f;//光照影响系数
        private float light_influence_coefficient_end=1.0f;//光照影响系数
        private string light_influence_coefficient_control_mode="Linear";

        private float velocityX_tmp=0.1f;
        private float velocityY_tmp=0.0f;
        private float amplify_tmp=0.5f;
        private float frequency_tmp=2.0f;
        private float light_influence_coefficient_tmp=1.0f;
        public FogEffectControl(EntityData data,Vector2 offset)
        {
            Position=data.Position+offset;
            area=new Rectangle((int)Position.X,(int)Position.Y,data.Width,data.Height);

            area_control_mode=data.Attr("AreaControlMode");
            label=data.Attr("Label");

            velocityX_start=data.Float("VelocityXStart");//流体流速
            velocityX_end=data.Float("VelocityXEnd");//流体流速
            velocityY_start=data.Float("VelocityYStart");//流体流速
            velocityY_end=data.Float("VelocityYEnd");//流体流速

            velocity_control_mode=data.Attr("VelocityControlMode");

            amplify_start=data.Float("AmplifyStart");//流体强度
            amplify_end=data.Float("AmplifyEnd");//流体强度
            amplify_control_mode=data.Attr("AmplifyControlMode");

            frequency_start=data.Float("FrequencyStart");//流体细节度
            frequency_end=data.Float("FrequencyEnd");//流体细节度
            frequency_control_mode=data.Attr("FrequencyControlMode");

            light_influence_coefficient_start=data.Float("LICStart");//光照影响系数
            light_influence_coefficient_end=data.Float("LICEnd");//光照影响系数
            light_influence_coefficient_control_mode=data.Attr("LICControlMode");

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TransitionListener handle_value=new TransitionListener();
            handle_value.OnIn=delegate(float f)
            {
                UpdateParameter();
            };
            Add(handle_value);
        }
        //尝试更新参数
        private void UpdateParameter()
        {
            if (Active == false||Scene == null)
            {
                return;
            }
            Player player=Scene.Tracker.GetEntity<Player>();
            if(player==null||player.Dead)
            {
                return;
            }
            Vector2 offset=player.Position-Position;
            //获取X比例和Y比例，首先只有人物中心在区域之内才能有效果，其次要根据控制方式来确定是X生效还是Y生效
            float x_proportion=offset.X/area.Width;
            float y_proportion=offset.Y/area.Height;
            if(x_proportion>=0.0f&&x_proportion<=1.0f&&y_proportion>=0.0f&&y_proportion<=1.0f)
            {
                //从左到右模式
                if(area_control_mode=="Left_to_Right")
                {
                    float time1=DBBMath.MotionMapping(x_proportion,velocity_control_mode);
                    float time2=DBBMath.MotionMapping(x_proportion,amplify_control_mode);
                    float time3=DBBMath.MotionMapping(x_proportion,frequency_control_mode);
                    float time4=DBBMath.MotionMapping(x_proportion,light_influence_coefficient_control_mode);
                    velocityX_tmp=(float)DBBMath.Linear_Lerp(time1,velocityX_start,velocityX_end);
                    velocityY_tmp=(float)DBBMath.Linear_Lerp(time1,velocityY_start,velocityY_end);
                    amplify_tmp=(float)DBBMath.Linear_Lerp(time2,amplify_start,amplify_end);
                    frequency_tmp=(float)DBBMath.Linear_Lerp(time3,frequency_start,frequency_end);
                    light_influence_coefficient_tmp=(float)DBBMath.Linear_Lerp(time4,light_influence_coefficient_start,light_influence_coefficient_end);
                }
                //从下到上模式
                else if(area_control_mode=="Bottom_to_Top")
                {
                    float time1=DBBMath.MotionMapping(y_proportion,velocity_control_mode);
                    float time2=DBBMath.MotionMapping(y_proportion,amplify_control_mode);
                    float time3=DBBMath.MotionMapping(y_proportion,frequency_control_mode);
                    float time4=DBBMath.MotionMapping(y_proportion,light_influence_coefficient_control_mode);
                    velocityX_tmp=(float)DBBMath.Linear_Lerp(time1,velocityX_start,velocityX_end);
                    velocityY_tmp=(float)DBBMath.Linear_Lerp(time1,velocityY_start,velocityY_end);
                    amplify_tmp=(float)DBBMath.Linear_Lerp(time2,amplify_start,amplify_end);
                    frequency_tmp=(float)DBBMath.Linear_Lerp(time3,frequency_start,frequency_end);
                    light_influence_coefficient_tmp=(float)DBBMath.Linear_Lerp(time4,light_influence_coefficient_start,light_influence_coefficient_end);
                }
                //立即数模式
                else if(area_control_mode=="Instant")
                {
                    velocityX_tmp=velocityX_end;
                    velocityY_tmp=velocityY_end;
                    amplify_tmp=amplify_end;
                    frequency_tmp=frequency_end;
                    light_influence_coefficient_tmp=light_influence_coefficient_end;
                }
                List<Entity> FogList;
                if (!DBBCustomEntityManager.TrackEntityList(typeof(FogEffect), out FogList) || FogList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in FogList)
                {
                    var item = entity_item as FogEffect;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        item.velocity_x = velocityX_tmp;
                        item.velocity_y = velocityY_tmp;
                        item.amplify = amplify_tmp;
                        item.frequency = frequency_tmp;
                        item.light_influence_coefficient = light_influence_coefficient_tmp;
                    }
                }
            }
            return;
        }
        public override void Update()
        {
            base.Update();
            UpdateParameter();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Position,area.Width,area.Height,Color.WhiteSmoke);
        }

    }
}