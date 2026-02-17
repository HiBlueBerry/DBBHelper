using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/TextureLight")]
    public class TextureLight : DBBGeneralLight
    {
        private MTexture light_texture;
        private Color tint_color = Color.White;//光照的整体染色
        private float scaleX = 1.0f;
        private float scaleY = 1.0f;
        private float rotation = 0.0f;
        private float light_amplify = 1.0f;//通道增强
        private Vector2 light_center = Vector2.Zero;
        private bool need_debug_mask = false;
        public TextureLight(EntityData data, Vector2 offset)
        {
            //通用设置
            Position = data.Position + offset;
            scaleX = data.Float("ScaleX");
            scaleY = data.Float("ScaleY");
            rotation = -data.Float("Rotation") / 180.0f * (float)Math.PI;
            tint_color = Calc.HexToColor(data.Attr("TintColor"));
            light_amplify = data.Float("LightAmplify", 1.0f);
            //是否仅启用原版光照
            DisableEntityLight = data.Bool("OnlyEnableOriginalLight");
            //是否需要调试图案位置
            need_debug_mask = data.Bool("DebugMask");
            //贴图
            string path = data.String("TexturePath");
            light_texture = GFX.Game[path];
            //没找到贴图则更换为警告用贴图
            if (GFX.Game.Has(path) == false)
            {
                light_texture = GFX.Game["objects/DBB_Items/DBBLightTexture/texture_not_found"];
                scaleX = 1.0f;
                scaleY = 1.0f;
                rotation = 0.0f;
                tint_color = Color.Red;
            }
        }
        public override void Added(Scene scene)
        {
            //调用base.Added(scene)来将该实体分别添加到原版和自定义实体管理器中
            base.Added(scene);
            light_center = Position - (Scene as Level).Camera.Position;
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
            base.DebugRender(camera);
            if (need_debug_mask == true)
            {
                light_texture.DrawOutlineCentered(Position, Color.White, new Vector2(scaleX, scaleY), rotation);
            }
        }
        //GFX.SpriteBank.Create(spriteName);
        public override void Render_On_Original_Light()
        {
            //如果光源不可见，则不渲染
            if (Visible == false)
            {
                return;
            }
            light_center = Position - (Scene as Level).Camera.Position;
            //这里只需要光的颜色叠加，alpha不应该影响光的颜色
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            light_texture.DrawCentered(light_center, tint_color, new Vector2(scaleX, scaleY), rotation);
            Draw.SpriteBatch.End();
        }
        public override void Render_On_Custom_Light()
        {
            //如果光源不可见，则不渲染
            //如果只在原光照贴图上绘制，则不渲染
            if (Visible == false || DisableEntityLight == true)
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
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            light_texture.DrawCentered(light_center, adjusted_color, new Vector2(scaleX, scaleY), rotation);
            Draw.SpriteBatch.End();
        }
    }
}