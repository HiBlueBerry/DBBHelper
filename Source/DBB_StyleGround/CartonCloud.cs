using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Backdrops;
using System;
using Celeste.Mod.DBBHelper.Mechanism;
using System.Collections.Generic;

namespace Celeste.Mod.DBBHelper.BackDrops
{
    [CustomBackdrop("DBBHelper/CartonCloud")]
    public class CartonCloud : Backdrop
    {
        public static BlendState CartonCloudAlphaBlend = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaBlendFunction = BlendFunction.Add
        };
        //以下与形态有关
        public MTexture texture = null;//噪声贴图
        public Vector2 velocity = Vector2.Zero;//提供速度控制
        public Vector2 scroll = Vector2.Zero;
        private Vector2 velocity_offset = Vector2.Zero;//水平速度偏移
        private Vector2 horizon_offset = Vector2.Zero;//水平视角偏移
        private float height_offset = 0.0f;//垂直高度偏移
        public Vector2 sample_detail = new Vector2(512.0f, 512.0f);//网格采样细节度
        public float fre = 2.0f;//分形频率
        public float strength = 0.8f;//噪声强度
        public int num = 4;//分形次数
        public float baseline_height = 0.0f;//云层基础高度
        public float height_scale = 0.4f;//云层高度起伏的缩放
        //以下与颜色分层有关
        //layer_color1-5为云层颜色
        //layer_h1-5为不同云层的阈值高度
        public Vector4 layer_color1 = new Vector4(0.92f, 0.85f, 0.82f, 1.0f);
        public Vector4 layer_color2 = new Vector4(0.98f, 0.76f, 0.64f, 1.0f);
        public Vector4 layer_color3 = new Vector4(0.95f, 0.80f, 0.77f, 1.0f);
        public Vector4 layer_color4 = new Vector4(0.72f, 0.85f, 0.82f, 1.0f);
        public Vector4 layer_color5 = new Vector4(0.92f, 0.85f, 0.82f, 1.0f);
        public float layer_h1 = 0.01f;
        public float layer_h2 = 0.23f;
        public float layer_h3 = 0.35f;
        public float layer_h4 = 0.39f;
        //以下与光交互有关
        public float bloom_contrast = 4.0f;//辉光对比度
        public float bloom_strength = 1.5f;//辉光强度
        public float light_influence_coefficient = 1.0f;//亮度影响系数
        public CartonCloud(BinaryPacker.Element data)
        {
            //纹理
            string texture_path = data.Attr("Texture");
            if (GFX.Game.Has(texture_path) == true)
            {
                texture = GFX.Game[data.Attr("Texture")];
            }
            else
            {
                texture = DBBEffectSourceManager.DefaultTexture;
            }
            //以下与形态有关
            velocity = new Vector2(-data.AttrFloat("VelocityX", 0.5f), data.AttrFloat("VelocityY", 0.03f));
            scroll = new Vector2(data.AttrFloat("ScrollX", 0.0f), data.AttrFloat("ScrollY", 0.0f));
            sample_detail = new Vector2(data.AttrFloat("SampleDetailX", 512.0f), data.AttrFloat("SampleDetailY", 512.0f));
            fre = data.AttrFloat("Fre", 2.0f);
            strength = data.AttrFloat("Strength", 0.8f);
            num = data.AttrInt("IterNum", 4);
            baseline_height = 1.0f - data.AttrFloat("BaselineHeight", 0.0f);
            height_scale = data.AttrFloat("HeightScale", 0.4f);
            
            //以下与云层颜色有关
            layer_color1 = DBBMath.ConvertColor(data.Attr("LayerColor1", "331426"));
            layer_color2 = DBBMath.ConvertColor(data.Attr("LayerColor2", "CC334D"));
            layer_color3 = DBBMath.ConvertColor(data.Attr("LayerColor3", "F28066"));
            layer_color4 = DBBMath.ConvertColor(data.Attr("LayerColor4", "B380CC"));
            layer_color5 = DBBMath.ConvertColor(data.Attr("LayerColor5", "0D0526"));
            layer_color1.W = data.AttrFloat("LayerAlpha1", 1.0f);
            layer_color2.W = data.AttrFloat("LayerAlpha2", 1.0f);
            layer_color3.W = data.AttrFloat("LayerAlpha3", 1.0f);
            layer_color4.W = data.AttrFloat("LayerAlpha4", 1.0f);
            layer_color5.W = data.AttrFloat("LayerAlpha5", 1.0f);
            layer_h1 = data.AttrFloat("LayerH1", 0.01f);
            layer_h2 = data.AttrFloat("LayerH2", 0.23f);
            layer_h3 = data.AttrFloat("LayerH3", 0.35f);
            layer_h4 = data.AttrFloat("LayerH4", 0.39f);
            //以下与光交互有关
            bloom_contrast = data.AttrFloat("BloomContrast", 4.0f);
            bloom_strength = data.AttrFloat("BloomStrength", 1.5f);
            light_influence_coefficient = data.AttrFloat("LightInfluenceCoefficient", 1.0f);
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            horizon_offset = new Vector2((scene as Level).Camera.Position.X * scroll.X / (DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture320x180"].Width + 0.01f), 0.0f);
            height_offset = (scene as Level).Camera.Position.Y * scroll.Y / (DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture320x180"].Height + 0.01f);
            velocity_offset += Engine.DeltaTime * velocity;
        }
        public override void Render(Scene scene)
        {
            //形态参数
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["light_mask"].SetValue(GameplayBuffers.Light);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["noise_tex"].SetValue(texture.Texture.Texture);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["offset"].SetValue(horizon_offset + velocity_offset);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["sample_detail"].SetValue(sample_detail);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["fre"].SetValue(fre);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["strength"].SetValue(strength);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["num"].SetValue(num);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["baseline_height"].SetValue(baseline_height);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["height_scale"].SetValue(height_scale);
            
            //分层颜色
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_color1"].SetValue(layer_color1);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_color2"].SetValue(layer_color2);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_color3"].SetValue(layer_color3);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_color4"].SetValue(layer_color4);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_color5"].SetValue(layer_color5);
            //分层高度
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_h1"].SetValue(layer_h1 + height_offset);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_h2"].SetValue(layer_h2 + height_offset);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_h3"].SetValue(layer_h3 + height_offset);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["layer_h4"].SetValue(layer_h4 + height_offset);
            //光交互参数
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["bloom_contrast"].SetValue(bloom_contrast);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["bloom_strength"].SetValue(bloom_strength);
            DBBEffectSourceManager.DBBEffect["CartonCloud"].Parameters["light_influence_coefficient"].SetValue(light_influence_coefficient);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["CartonCloud"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture320x180"], Vector2.Zero, Color.Transparent);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
        }
    }
}