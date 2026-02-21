using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
using System.Collections.Generic;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/AnimatedTextureLight")]
    public class AnimatedTextureLight : DBBGeneralLight
    {
        public Sprite light_sprite = null;
        public Color tint_color = Color.White;//光照的整体染色
        public Color ref_tint_color = Color.White;
        public float scaleX = 1.0f;
        public float scaleY = 1.0f;
        public float rotation = 0.0f;
        public float delay_time = 0.01f;
        private float light_amplify = 1.0f;
        private Vector2 light_center = Vector2.Zero;
        private bool need_debug_mask = false;

        public AnimatedTextureLight(EntityData data, Vector2 offset)
        {
            //场景切入或切出时的光颜色变化方式
            LevelIn_Style = data.Attr("LevelInStyle", "easeInOutSin");
            LevelOut_Style = data.Attr("LevelOutStyle", "easeInOutSin");
            //通用设置
            Position = data.Position + offset;
            scaleX = data.Float("ScaleX", 1.0f);
            scaleY = data.Float("ScaleY", 1.0f);
            rotation = -data.Float("Rotation", 0.0f) / 180.0f * (float)Math.PI;
            delay_time = data.Float("DelayTime", 0.01f);
            tint_color = Calc.HexToColor(data.Attr("TintColor"));
            ref_tint_color = tint_color;
            light_amplify = data.Float("LightAmplify", 1.0f);
            //是否仅启用原版光照
            DisableEntityLight = data.Bool("OnlyEnableOriginalLight");
            //是否需要调试图案位置
            need_debug_mask = data.Bool("DebugMask");
            //精灵
            string path = data.String("SpritePath");
            //新建一个精灵并设置它的属性
            light_sprite = new Sprite(GFX.Game, path);
            light_sprite.Scale = new Vector2(scaleX, scaleY);
            light_sprite.Rotation = rotation;
            light_sprite.Color = tint_color;
            light_sprite.Position = Position;

            //先尝试获取一次动画，如果获取失败，这时候设置为警告动画
            List<MTexture> list = GFX.Game.orig_GetAtlasSubtextures(path + "");
            if (list == null || list.Count == 0)
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/AnimatedTextureLight", " Not find any textures about: " + path);
                light_sprite.Reset(GFX.Game, "objects/DBB_Items/DBBLightTexture/AnimatedWarning/sprite_not_found");
                //重新设置精灵属性
                light_sprite.Scale = Vector2.One;
                light_sprite.Rotation = 0.0f;
                light_sprite.Color = Color.Red;
                tint_color = Color.Red;
                light_sprite.Position = Position;
                //设置它的状态为idel并且设置为循环动画
                light_sprite.AddLoop("idle", "", 0.15f);
            }
            else
            {
                //设置它的状态为idel并且设置为循环动画
                light_sprite.AddLoop("idle", "", delay_time);
            }
        }
        public override void Added(Scene scene)
        {
            //调用base.Added(scene)来将该实体分别添加到原版和自定义实体管理器中
            base.Added(scene);
            Add(light_sprite);
            //这里避免使用原版的精灵绘制，不然会绘制出两个不同位置的精灵出来
            light_sprite.Visible = false;

            TransitionListener handle_attribute = new TransitionListener();
            //在场景过渡进入时，将一些值进行渐进处理
            handle_attribute.OnIn = delegate (float f)
            {
                //只有在非Instant模式下才进行渐变效果
                if (LevelIn_Style == "Instant")
                {
                    return;
                }
                float time = (float)DBBMath.MotionMapping(f, LevelIn_Style);
                tint_color.R = (byte)(time * ref_tint_color.R);
                tint_color.G = (byte)(time * ref_tint_color.G);
                tint_color.B = (byte)(time * ref_tint_color.B);
                tint_color.A = (byte)(time * ref_tint_color.A);
            };
            handle_attribute.OnInBegin = delegate ()
            {
                if (LevelIn_Style != "Instant")
                {
                    tint_color = Color.Transparent;
                }
            };
            handle_attribute.OnOutBegin = delegate ()
            {
                if (LevelOut_Style == "Instant")
                {
                    tint_color = Color.Transparent;
                }
            };
            handle_attribute.OnOut = delegate (float f)
            {
                //只有在非Instant模式下才进行渐变效果
                if (LevelOut_Style == "Instant")
                {
                    return;
                }
                float time = (float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(f, LevelOut_Style), 1.0f, 0.0f);
                tint_color.R = (byte)(time * ref_tint_color.R);
                tint_color.G = (byte)(time * ref_tint_color.G);
                tint_color.B = (byte)(time * ref_tint_color.B);
                tint_color.A = (byte)(time * ref_tint_color.A);
            };
            Add(handle_attribute);
        }
        public override void Removed(Scene scene)
        {
            //调用base.Removed(scene)来将该实体分别从原版和自定义实体管理器中移除
            base.Removed(scene);
        }

        //实体的更新由原版管理器控制
        public override void Update()
        {
            base.Update();
        }
        public override void DebugRender(Camera camera)
        {
            //Debug走的是原版渲染流程
            base.DebugRender(camera);
            if (need_debug_mask == true)
            {
                light_sprite.Texture.DrawCentered(Position, tint_color, new Vector2(scaleX, scaleY), rotation);
            }
        }
        public override void Render_On_Original_Light()
        {
            //如果光源不可见，则不渲染
            //如果贴图不存在，则不渲染
            if (Visible == false || light_sprite == null)
            {
                return;
            }
            light_center = Position - (Scene as Level).Camera.Position;
            light_sprite.Play("idle",false,false);
            //这里只需要光的颜色叠加，alpha不应该影响光的颜色
            //这里把sprite.Render里的东西单独拿出来运行
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            light_sprite.Texture.DrawCentered(light_center, tint_color, new Vector2(scaleX, scaleY), rotation);
            Draw.SpriteBatch.End();
        }
        public override void Render_On_Custom_Light()
        {
            //如果光源不可见，则不渲染
            //如果贴图不存在，则不渲染
            //如果只在原光照贴图上绘制，则不渲染
            if (Visible == false || light_sprite == null || DisableEntityLight == true)
            {
                return;
            }
            //alpha通道增强
            Color adjusted_color = tint_color;
            adjusted_color.R = (byte)Math.Clamp(adjusted_color.R * light_amplify, 0.0, 255.0);
            adjusted_color.G = (byte)Math.Clamp(adjusted_color.G * light_amplify, 0.0, 255.0);
            adjusted_color.B = (byte)Math.Clamp(adjusted_color.B * light_amplify, 0.0, 255.0);
            adjusted_color.A = (byte)Math.Clamp(adjusted_color.A * light_amplify, 0.0, 255.0);
            //这里将贴图视为一种贴图而不是光，因此alpha应该去影响最终的颜色，推荐贴图的低亮度的地方为低alpha值
            //这里把sprite.Render里的东西单独拿出来运行
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            light_sprite.Texture.DrawCentered(light_center, adjusted_color, new Vector2(scaleX, scaleY), rotation);
            Draw.SpriteBatch.End();
        }
    }
}