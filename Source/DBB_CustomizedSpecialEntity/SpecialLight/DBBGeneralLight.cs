using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [DBBCustomEntity(DBBCustomEntityIndexTable.SpecialLight, true)]
    [Tracked(true)]
    public class DBBGeneralLight : Entity
    {
        //用于绘制光照贴图
        public static BlendState GeneralLightAlphaBlend = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.DestinationAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };
        public static Vector2 Light_WH = new Vector2(320, 180);//记录当前GameplayBuffers.Light的宽高
        public static Vector2 GeneralLight_WH = new Vector2(320, 180);//记录GeneralLight缓冲的的宽高
        public string label = "Default";//一个标记该实体的标签
        public bool DisableEntityLight = false;//是否禁用实体光

        public override void Added(Scene scene)
        {
            base.Added(scene);
            //向DBBCustomEntityManager添加该实体
            DBBCustomEntityManager.Added_As_BaseType(this, typeof(DBBGeneralLight));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            //从DBBCustomEntityManager删除该实体
            DBBCustomEntityManager.Removed_As_BaseType(this, typeof(DBBGeneralLight));
        }
        public override void Update()
        {
            base.Update();
        }
        /// <summary>
        /// 在原版光照贴图上渲染
        /// </summary>
        public virtual void Render_On_Original_Light()
        {

        }
        /// <summary>
        /// 在自定义光照贴图上渲染
        /// </summary>
        public virtual void Render_On_Custom_Light()
        {

        }

        public static void Load()
        {
            //用于绘制自定义灯光
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create += Init_Buffer;
            //需要时调整缓冲区的大小
            DBBEffectSourceManager.Adjust_SomeBuffers_Before_Render += Adjust_GeneralLight_Buffer_WH;
            //绘制自定义光，先在原版Light缓冲上绘制光，然后在自定义GeneralLight缓冲上绘制光
            DBBEffectSourceManager.Draw_Something_On_Light += Draw_GeneralLight_On_Light_And_GeneralLight;
            //最后把GeneralLight缓冲的内容绘制到GameplayTempA上
            DBBEffectSourceManager.Draw_Something_On_GameplayTempA += Draw_GeneralLight_On_GamePlayTempA;
        }
        public static void UnLoad()
        {
            DBBEffectSourceManager.Draw_Something_On_GameplayTempA -= Draw_GeneralLight_On_GamePlayTempA;
            DBBEffectSourceManager.Draw_Something_On_Light -= Draw_GeneralLight_On_Light_And_GeneralLight;
            DBBEffectSourceManager.Adjust_SomeBuffers_Before_Render -= Adjust_GeneralLight_Buffer_WH;
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create -= Init_Buffer;
        }
        /// <summary>
        /// 初始化自定义缓冲区
        /// </summary>
        private static void Init_Buffer()
        {
            DBBGamePlayBuffers.Create("GeneralLight", (int)GeneralLight_WH.X, (int)GeneralLight_WH.Y);
        }

        /// <summary>
        /// 调整GeneralLight的大小，此处是为了与扩展镜头兼容
        /// </summary>
        private static void Adjust_GeneralLight_Buffer_WH()
        {
            Light_WH = new Vector2(GameplayBuffers.Light.Width, GameplayBuffers.Light.Height);
            if (GeneralLight_WH.X != Light_WH.X || GeneralLight_WH.Y != Light_WH.Y)
            {
                GeneralLight_WH = Light_WH;
                DBBGamePlayBuffers.ReloadBuffer("GeneralLight", GeneralLight_WH);
            }
        }

        /// <summary>
        /// 在原版光照贴图和自定义光照贴图上进行自定义光照绘制
        /// </summary>
        private static void Draw_GeneralLight_On_Light_And_GeneralLight()
        {
            //获取实体列表
            List<Entity> GeneralLightList;
            //如果没找到GeneralLight实体则返回
            if (!DBBCustomEntityManager.TrackEntityList(typeof(DBBGeneralLight), out GeneralLightList) || GeneralLightList.Count == 0)
            {
                //清空GeneralLight的内容
                if (DBBGamePlayBuffers.DBBRenderTargets.ContainsKey("GeneralLight") == true)
                {
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["GeneralLight"]);
                    Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                }
                return;
            }
            Adjust_GeneralLight_Buffer_WH();
            //如果当前场景不是Level，则不渲染 
            if (Engine.Scene as Level == null)
            {
                return;
            }
            //在原版Light缓冲上绘制光
            foreach (var item in GeneralLightList)
            {
                (item as DBBGeneralLight).Render_On_Original_Light();
            }
            //在GeneralLight上绘制渲染到GamePlay的光
            if (DBBGamePlayBuffers.DBBRenderTargets.ContainsKey("GeneralLight") == true)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["GeneralLight"]);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                foreach (var item in GeneralLightList)
                {
                    (item as DBBGeneralLight).Render_On_Custom_Light();
                }
            }
        }

        /// <summary>
        /// 将自定义GeneralLight缓冲绘制到GamePlayTempA缓冲上
        /// </summary>
        private static void Draw_GeneralLight_On_GamePlayTempA()
        {
            if (DBBGamePlayBuffers.DBBRenderTargets.ContainsKey("GeneralLight") == false)
            {
                return;
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, GeneralLightAlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["GeneralLight"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

    }
}