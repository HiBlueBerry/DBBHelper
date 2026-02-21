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
    [CustomEntity("DBBHelper/AdvancedBlockGlitchEffectControl")]
    public class AdvancedBlockGlitchEffectControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个
        //------------------可以更改的特效性质------------------
        private float velocity_first_start = 1.0f;
        private float velocity_first_end = 1.0f;
        private string velocity_first_control_mode = "Linear";
        private float velocity_first_tmp = 1.0f;

        private float velocity_second_start = 1.0f;
        private float velocity_second_end = 1.0f;
        private string velocity_second_control_mode = "Linear";
        private float velocity_second_tmp = 1.0f;

        private float size_firstX_start = 4.0f;
        private float size_firstX_end = 4.0f;
        private string size_firstX_control_mode = "Linear";
        private float size_firstX_tmp = 4.0f;

        private float size_firstY_start = 4.0f;
        private float size_firstY_end = 4.0f;
        private string size_firstY_control_mode = "Linear";
        private float size_firstY_tmp = 4.0f;

        private float size_secondX_start = 4.0f;
        private float size_secondX_end = 4.0f;
        private string size_secondX_control_mode = "Linear";
        private float size_secondX_tmp = 4.0f;

        private float size_secondY_start = 4.0f;
        private float size_secondY_end = 4.0f;
        private string size_secondY_control_mode = "Linear";
        private float size_secondY_tmp = 4.0f;

        private float strength_start = 1.0f;
        private float strength_end = 1.0f;
        private string strength_control_mode = "Linear";
        private float strength_tmp = 1.0f;

        public AdvancedBlockGlitchEffectControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");

            velocity_first_start = data.Float("VelocityFirstStart");
            velocity_first_end = data.Float("VelocityFirstEnd");
            velocity_first_control_mode = data.Attr("VelocityFirstControlMode");

            velocity_second_start = data.Float("VelocitySecondStart");
            velocity_second_end = data.Float("VelocitySecondEnd");
            velocity_second_control_mode = data.Attr("VelocitySecondControlMode");

            size_firstX_start = data.Float("SizeFirstXStart");
            size_firstX_end = data.Float("SizeFirstXEnd");
            size_firstX_control_mode = data.Attr("SizeFirstXControlMode");

            size_firstY_start = data.Float("SizeFirstYStart");
            size_firstY_end = data.Float("SizeFirstYEnd");
            size_firstY_control_mode = data.Attr("SizeFirstYControlMode");

            size_secondX_start = data.Float("SizeSecondXStart");
            size_secondX_end = data.Float("SizeSecondXEnd");
            size_secondX_control_mode = data.Attr("SizeSecondXControlMode");

            size_secondY_start = data.Float("SizeSecondYStart");
            size_secondY_end = data.Float("SizeSecondYEnd");
            size_secondY_control_mode = data.Attr("SizeSecondYControlMode");

            strength_start = data.Float("StrengthStart");
            strength_end = data.Float("StrengthEnd");
            strength_control_mode = data.Attr("StrengthControlMode");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TransitionListener handle_value = new TransitionListener();
            handle_value.OnIn = delegate (float f)
            {
                UpdateParameter();
            };
            Add(handle_value);
        }
        private float Calc_tmpValue(float t, string mode, float start, float end)
        {
            float time = DBBMath.MotionMapping(t, mode);
            return (float)DBBMath.Linear_Lerp(time, start, end);
        }
        //尝试更新参数
        private void UpdateParameter()
        {
            if (Active == false||Scene == null)
            {
                return;
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return;
            }
            Vector2 offset = player.Position - Position;
            //获取X比例和Y比例，首先只有人物中心在区域之内才能有效果，其次要根据控制方式来确定是X生效还是Y生效
            float x_proportion = offset.X / area.Width;
            float y_proportion = offset.Y / area.Height;
            if (x_proportion >= 0.0f && x_proportion <= 1.0f && y_proportion >= 0.0f && y_proportion <= 1.0f)
            {
                //从左到右模式
                if (area_control_mode == "Left_to_Right")
                {
                    velocity_first_tmp = Calc_tmpValue(x_proportion, velocity_first_control_mode, velocity_first_start, velocity_first_end);
                    velocity_second_tmp = Calc_tmpValue(x_proportion, velocity_second_control_mode, velocity_second_start, velocity_second_end);
                    size_firstX_tmp = Calc_tmpValue(x_proportion, size_firstX_control_mode, size_firstX_start, size_firstX_end);
                    size_firstY_tmp = Calc_tmpValue(x_proportion, size_firstY_control_mode, size_firstY_start, size_firstY_end);
                    size_secondX_tmp = Calc_tmpValue(x_proportion, size_secondX_control_mode, size_secondX_start, size_secondX_end);
                    size_secondY_tmp = Calc_tmpValue(x_proportion, size_secondY_control_mode, size_secondY_start, size_secondY_end);
                    strength_tmp = Calc_tmpValue(x_proportion, strength_control_mode, strength_start, strength_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    velocity_first_tmp = Calc_tmpValue(y_proportion, velocity_first_control_mode, velocity_first_start, velocity_first_end);
                    velocity_second_tmp = Calc_tmpValue(y_proportion, velocity_second_control_mode, velocity_second_start, velocity_second_end);
                    size_firstX_tmp = Calc_tmpValue(y_proportion, size_firstX_control_mode, size_firstX_start, size_firstX_end);
                    size_firstY_tmp = Calc_tmpValue(y_proportion, size_firstY_control_mode, size_firstY_start, size_firstY_end);
                    size_secondX_tmp = Calc_tmpValue(y_proportion, size_secondX_control_mode, size_secondX_start, size_secondX_end);
                    size_secondY_tmp = Calc_tmpValue(y_proportion, size_secondY_control_mode, size_secondY_start, size_secondY_end);
                    strength_tmp = Calc_tmpValue(y_proportion, strength_control_mode, strength_start, strength_end);
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    velocity_first_tmp = velocity_first_end;
                    velocity_second_tmp = velocity_second_end;
                    size_firstX_tmp = size_firstX_end;
                    size_firstY_tmp = size_firstY_end;
                    size_secondX_tmp = size_secondX_end;
                    size_secondY_tmp = size_secondY_end;
                    strength_tmp = strength_end;
                }
                List<Entity> AdvancedBlockGlitchList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(AdvancedBlockGlitchEffect), out AdvancedBlockGlitchList) || AdvancedBlockGlitchList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in AdvancedBlockGlitchList)
                {
                    var item = entity_item as AdvancedBlockGlitchEffect;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        item.velocity_first = velocity_first_tmp;
                        item.velocity_second = velocity_second_tmp;
                        item.size_first = new Vector2(size_firstX_tmp, size_firstY_tmp);
                        item.size_second = new Vector2(size_secondX_tmp, size_secondY_tmp);
                        item.strength = strength_tmp;
                        
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