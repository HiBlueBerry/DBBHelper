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
    [CustomEntity("DBBHelper/BlockGlitchEffectControl")]
    public class BlockGlitchEffectControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个

        //------------------可以更改的特效性质------------------
        private float velocity_start = 1.0f;
        private float velocity_end = 1.0f;
        private string velocity_control_mode = "Linear";
        private float velocity_tmp = 1.0f;

        private float block_num_start = 4.0f;
        private float block_num_end = 4.0f;
        private string block_num_control_mode = "Linear";
        private float block_num_tmp = 4.0f;

        private float strength_start = 1.0f;
        private float strength_end = 1.0f;
        private string strength_control_mode = "Linear";
        private float strength_tmp = 1.0f;


        public BlockGlitchEffectControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");

            velocity_start = data.Float("VelocityStart");
            velocity_end = data.Float("VelocityEnd");
            velocity_control_mode = data.Attr("VelocityControlMode");

            block_num_start = data.Float("BlockNumStart");
            block_num_end = data.Float("BlockNumEnd");
            block_num_control_mode = data.Attr("BlockNumControlMode");

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
                    velocity_tmp = Calc_tmpValue(x_proportion, velocity_control_mode, velocity_start, velocity_end);
                    block_num_tmp = Calc_tmpValue(x_proportion, block_num_control_mode, block_num_start, block_num_end);
                    strength_tmp = Calc_tmpValue(x_proportion, strength_control_mode, strength_start, strength_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    velocity_tmp = Calc_tmpValue(y_proportion, velocity_control_mode, velocity_start, velocity_end);
                    block_num_tmp = Calc_tmpValue(y_proportion, block_num_control_mode, block_num_start, block_num_end);
                    strength_tmp = Calc_tmpValue(y_proportion, strength_control_mode, strength_start, strength_end);
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    velocity_tmp = velocity_end;
                    block_num_tmp = block_num_end;
                    strength_tmp = strength_end;
                }
                List<Entity> BlockGlitchEffectList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(BlockGlitchEffect), out BlockGlitchEffectList) || BlockGlitchEffectList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in BlockGlitchEffectList)
                {
                    var item = entity_item as BlockGlitchEffect;
                    if (item.label == label && item.Active == true)
                    {
                        item.velocity = velocity_tmp;
                        item.block_num = block_num_tmp;
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
            Draw.HollowRect(Position, area.Width, area.Height, Color.WhiteSmoke);
        }
    }
}