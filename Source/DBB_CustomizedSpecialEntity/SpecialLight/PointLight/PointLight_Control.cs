using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/PointLightControl")]
    public class PointLightControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个

        //------------------可以更改的特效性质------------------

        private Vector4 color_start=Vector4.One;
        private Vector4 color_end=Vector4.One;
        private string color_control_mode = "Linear";
        private Vector4 color_tmp=Vector4.One;
        private bool has_color = true;

        private float extinction_start = 10.0f;
        private float extinction_end = 10.0f;
        private string extinction_control_mode = "Linear";
        private float extinction_tmp = 10.0f;

        private float sphere_radius_start = 0.1f;
        private float sphere_radius_end = 0.1f;
        private string sphere_radius_control_mode = "Linear";
        private float sphere_radius_tmp = 0.1f;

        private float edge_width_start = 5.0f;
        private float edge_width_end = 5.0f;
        private string edge_width_control_mode = "Linear";
        private float edge_width_tmp = 5.0f;

        private float fresnel_coefficient_start = 1.0f;
        private float fresnel_coefficient_end = 1.0f;
        private string fresnel_coefficient_control_mode = "Linear";
        private float fresnel_coefficient_tmp = 1.0f;

        private float cameraz_start = 0.5f;
        private float cameraz_end = 0.5f;
        private string cameraz_control_mode = "Linear";
        private float cameraz_tmp = 0.5f;

        private float aspect_ratio_proportion_start = 1.0f;
        private float aspect_ratio_proportion_end = 1.0f;
        private string aspect_ratio_proportion_control_mode = "Linear";
        private float aspect_ratio_proportion_tmp = 1.0f;

        public PointLightControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");
            //为了兼容旧版本，这里需要判断是否有颜色参数
            if (!data.Has("ColorStart"))
            {
                has_color = false;
            }
            else
            {
                color_start = DBBMath.ConvertColor(data.Attr("ColorStart"));
                color_end = DBBMath.ConvertColor(data.Attr("ColorEnd"));
                color_control_mode = data.Attr("ColorControlMode");
                color_start.W = data.Float("AlphaStart",1.0f);
                color_end.W = data.Float("AlphaEnd",1.0f);
            }
            extinction_start = data.Float("ExtinctionStart");
            extinction_end = data.Float("ExtinctionEnd");
            extinction_control_mode = data.Attr("ExtinctionControlMode");

            sphere_radius_start = data.Float("SphereRadiusStart");
            sphere_radius_end = data.Float("SphereRadiusEnd");
            sphere_radius_control_mode = data.Attr("SphereRadiusControlMode");

            edge_width_start = data.Float("EdgeWidthStart");
            edge_width_end = data.Float("EdgeWidthEnd");
            edge_width_control_mode = data.Attr("EdgeWidthControlMode");

            fresnel_coefficient_start = data.Float("FresnelCoefficientStart");
            fresnel_coefficient_end = data.Float("FresnelCoefficientEnd");
            fresnel_coefficient_control_mode = data.Attr("FresnelCoefficientControlMode");

            cameraz_start = data.Float("CameraZStart");
            cameraz_end = data.Float("CameraZEnd");
            cameraz_control_mode = data.Attr("CameraZControlMode");

            aspect_ratio_proportion_start = data.Float("AspectRatioProportionStart");
            aspect_ratio_proportion_end = data.Float("AspectRatioProportionEnd");
            aspect_ratio_proportion_control_mode = data.Attr("AspectRatioProportionControlMode");

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
        private Vector4 Calc_tmpValue(float t, string mode, Vector4 start, Vector4 end)
        {
            float time = DBBMath.MotionMapping(t, mode);
            return DBBMath.Linear_Lerp(time, start, end);
        }
        //尝试更新参数
        private void UpdateParameter()
        {
            if (Scene == null)
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
                    if (has_color)
                    {
                        color_tmp = Calc_tmpValue(x_proportion, color_control_mode, color_start, color_end);
                    }
                    extinction_tmp = Calc_tmpValue(x_proportion, extinction_control_mode, extinction_start, extinction_end);
                    sphere_radius_tmp = Calc_tmpValue(x_proportion, sphere_radius_control_mode, sphere_radius_start, sphere_radius_end);
                    edge_width_tmp = Calc_tmpValue(x_proportion, edge_width_control_mode, edge_width_start, edge_width_end);
                    fresnel_coefficient_tmp = Calc_tmpValue(x_proportion, fresnel_coefficient_control_mode, fresnel_coefficient_start, fresnel_coefficient_end);
                    cameraz_tmp = Calc_tmpValue(x_proportion, cameraz_control_mode, cameraz_start, cameraz_end);
                    aspect_ratio_proportion_tmp = Calc_tmpValue(x_proportion, aspect_ratio_proportion_control_mode, aspect_ratio_proportion_start, aspect_ratio_proportion_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    if (has_color)
                    {
                        color_tmp = Calc_tmpValue(y_proportion, color_control_mode, color_start, color_end);
                    }
                    extinction_tmp = Calc_tmpValue(y_proportion, extinction_control_mode, extinction_start, extinction_end);
                    sphere_radius_tmp = Calc_tmpValue(y_proportion, sphere_radius_control_mode, sphere_radius_start, sphere_radius_end);
                    edge_width_tmp = Calc_tmpValue(y_proportion, edge_width_control_mode, edge_width_start, edge_width_end);
                    fresnel_coefficient_tmp = Calc_tmpValue(y_proportion, fresnel_coefficient_control_mode, fresnel_coefficient_start, fresnel_coefficient_end);
                    cameraz_tmp = Calc_tmpValue(y_proportion, cameraz_control_mode, cameraz_start, cameraz_end);
                    aspect_ratio_proportion_tmp = Calc_tmpValue(y_proportion, aspect_ratio_proportion_control_mode, aspect_ratio_proportion_start, aspect_ratio_proportion_end);
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    if (has_color)
                    {
                        color_tmp = color_end;
                    } 
                    extinction_tmp = extinction_end;
                    sphere_radius_tmp = sphere_radius_end;
                    edge_width_tmp = edge_width_end;
                    fresnel_coefficient_tmp = fresnel_coefficient_end;
                    cameraz_tmp = cameraz_end;
                    aspect_ratio_proportion_tmp = aspect_ratio_proportion_end;
                }
                List<Entity> PointLightList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(PointLight), out PointLightList) || PointLightList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in PointLightList)
                {
                    var item = entity_item as PointLight;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        //这里兼容之前的版本
                        if (has_color)
                        {
                            item.color = color_tmp;
                            item.ref_color = color_tmp;
                        }
                        item.extinction = extinction_tmp;
                        item.sphere_radius = sphere_radius_tmp;
                        item.edge_width = edge_width_tmp;
                        item.F0 = fresnel_coefficient_tmp;
                        item.camera_z = cameraz_tmp;
                        item.aspect_ratio_proportion = aspect_ratio_proportion_tmp;
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