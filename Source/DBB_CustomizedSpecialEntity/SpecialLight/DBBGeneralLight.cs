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
        //用于绘制光照贴图的两种颜色混合模式
        //alpha保持，这种混合方式会确保原版渲染内容中alpha不为1的贴图的颜色不会偏暗
        //但是会影响光照对这些半透明的贴图的效果，半透明贴图在光照影响下会更偏亮
        public static BlendState GeneralLight_AlphaKeep_Blend = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };
        //alpha减弱，这种混合方式会确保优先考虑光照的颜色，使得光照在半透明贴图的情况下能够更明显，而弱化贴图本身的颜色
        //该模式会影响常规情况下半透明贴图的亮度，使得贴图颜色更暗，特别的，对于alpha越低的贴图，这种影响越明显
        public static BlendState GeneralLight_AlphaWeakened_Blend = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.DestinationAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
        };
        public static BlendState GeneralLight_Current_Blend = GeneralLight_AlphaKeep_Blend;//当前所选用的混合模式
        public static bool OnlyEnableOriginalLight = false;
        public static Vector2 Light_WH = new Vector2(320, 180);//记录当前GameplayBuffers.Light的宽高
        public static Vector2 GeneralLight_WH = new Vector2(320, 180);//记录GeneralLight缓冲的的宽高
        public string label = "Default";//一个标记该实体的标签
        public bool DisableEntityLight = false;//是否禁用实体光
        public string LevelIn_Style = "Instant";//场景切入时的光照变化方式，默认立即变化
        public string LevelOut_Style = "Instant";//场景切除时的光照变化方式，默认立即变化

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
            //在未受到关卡内控制器的情况下，根据全局设置来设置OnlyEnableOriginalLight
            if (DBBSettings.SpecialLightMenu.OnlyEnableOriginalLight_InLevelControled == false)
            {
                DBBGlobalSettingManager.SpecialLight_OnlyEnableOriginalLight = DBBSettings.SpecialLightMenu.SpecialLight_OnlyEnableOriginalLight;
            }
            //更新一次OnlyEnableOriginalLight
            OnlyEnableOriginalLight = DBBGlobalSettingManager.SpecialLight_OnlyEnableOriginalLight == 0 ? false : true;
            //如果没找到GeneralLight实体则返回
            if (!DBBCustomEntityManager.TrackEntityList(typeof(DBBGeneralLight), out GeneralLightList) || GeneralLightList.Count == 0)
            {
                //清空GeneralLight的内容
                if (OnlyEnableOriginalLight == false && DBBGamePlayBuffers.DBBRenderTargets.ContainsKey("GeneralLight") == true)
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
            //只有在OnlyEnableOriginalLight为false并且存在GeneralLight缓冲区时才绘制
            if (OnlyEnableOriginalLight == false && DBBGamePlayBuffers.DBBRenderTargets.ContainsKey("GeneralLight") == true)
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
            //在OnlyEnableOriginalLight为true或者不存在GeneralLight缓冲区时跳过绘制
            if (OnlyEnableOriginalLight || DBBGamePlayBuffers.DBBRenderTargets.ContainsKey("GeneralLight") == false)
            {
                return;
            }
            //在未受到关卡内控制器的情况下，根据全局设置来设置blendState
            if (DBBSettings.SpecialLightMenu.GeneralLightBlendState_InLevelControled == false)
            {
                DBBGlobalSettingManager.SpecialLight_GeneralLightBlendState = DBBSettings.SpecialLightMenu.SpecialLight_GeneralLightBlendState;
            }
            //更新一次blendState
            GeneralLight_Current_Blend = DBBGlobalSettingManager.SpecialLight_GeneralLightBlendState == 0 ? GeneralLight_AlphaKeep_Blend : GeneralLight_AlphaWeakened_Blend;
            //Logger.Log(LogLevel.Warn, "GeneralLightBlendState_InLevelControled", DBBSettings.SpecialLightMenu.GeneralLightBlendState_InLevelControled.ToString());
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, GeneralLight_Current_Blend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["GeneralLight"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

    }
}