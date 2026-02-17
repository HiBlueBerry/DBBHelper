using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.DBBHelper.Mechanism
{
    /// <summary>
    /// 管理所有特效文件及其相关资源
    /// </summary>
    public class DBBEffectSourceManager
    {
        //-------------------全局参数设置-------------------
        //用于控制缓冲的创建与删除
        private static bool isFirstLoad = true;
        //用于兼容扩展镜头
        private static Vector2 GameplayTemp_WH;//记录当前的GameplayTemp的宽高
        private static Vector2 GameplayTempA_WH;//记录当前自定义GameplayTempA的宽高
        //用于兼容CelesteNet
        private static RenderTarget2D Default_RT = null;
        //用于控制TmpGameContent的大小来适配不同分辨率的屏幕
        public static Vector2 ref_resolution_vector = new Vector2(1922.0f, 1082.0f);

        //-------------------核心资源设置-------------------

        //存储所有的特效shader
        public static Dictionary<string, Effect> DBBEffect = new Dictionary<string, Effect>();

        //一个1920*1080的空纹理，目的在于铺满渲染shader屏幕，因为shader默认需要使用一张绘制纹理，对于雾效之类的shader不需要游戏向它实际送入任何画面，因此需要用一张全透明图像铺底
        public static MTexture DefaultTexture = null;

        //一个320*180的空纹理
        public static MTexture DefaultTexture320x180 = null;

        //-------------------默认提供的IL钩子事件-------------------
        public static Action Init_SomeBuffers_When_Level_Create = () => { };//在关卡创建时创建一些缓冲
        
        
        public static Action Adjust_SomeBuffers_Before_Render = () => { };//在正式渲染之前调整一些缓冲的大小
        public static Action Draw_Something_On_Light = () => { };//在Light上绘制一些东西

        public static Action Draw_Something_On_GameplayTempA = () => { };//在GameplayTempA上绘制一些东西

        public static Action Redraw_Something_On_Gameplay = () => { };//在Gameplay上重新绘制一些东西

        public static Action Draw_Something_On_TmpGameContent = () => { };//在TmpGameContent上绘制一些东西

        public static Action Redraw_Something_On_DefaultBuffer = () => { };//在默认缓冲上绘制一些东西

        //-------------------以下为函数-------------------



        //读取特效.cso文件
        private static void DBBEffectLoad(string path)
        {
            //如果特效已经存在
            if (string.IsNullOrEmpty(path))
            {
                Logger.Log(LogLevel.Warn, "DBBHelper", "Failed to load DBBeffect,because path in null.");
                return;
            }
            if (DBBEffect.ContainsKey(path))
            {
                Logger.Log(LogLevel.Info, "DBBHelper", "DBBEffect: " + path + " has existed.");
                return;
            }
            //如果特效不存在
            else
            {
                //尝试读取特效
                ModAsset effect_asset = default;
                if (Everest.Content.TryGet("Effects/" + path + ".cso", out effect_asset))
                {
                    DBBEffect.Add(path, new Effect(Engine.Graphics.GraphicsDevice, effect_asset.Data));
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "DBBHelper", "Failed to load DBBeffect " + path);
                }
            }

        }
        //创建所有缓冲纹理
        private static void DBBCreateAllBuffers()
        {
            GameplayTempA_WH = new Vector2(320, 180);
            ref_resolution_vector = new Vector2(1922.0f, 1082.0f);
            DBBGamePlayBuffers.Create("HDLight", 1922, 1082);//作为光照贴图的高清版本的副本
            DBBGamePlayBuffers.Create("GameplayTempA", (int)GameplayTempA_WH.X, (int)GameplayTempA_WH.Y);//作为Gameplay的副本
            DBBGamePlayBuffers.Create("TmpGameContent", 1922, 1082);//作为游戏画面的副本
            DBBGamePlayBuffers.Create("DefaultTexture", 1922, 1082);//创建一张1922*1082的透明贴图用于绘制
            DBBGamePlayBuffers.Create("DefaultTexture320x180",320,180);//创建一张320*180的透明贴图用于绘制
        }
        //加载所有特效
        private static void DBBLoadAllEffect()
        {
            //这两个用于UI组件
            DBBEffectLoad("DBBColorPicker");//拾色器
            DBBEffectLoad("DBBColorSpectrum");//色谱
            //FBM雾效，先雾效，之后是后处理
            DBBEffectLoad("DBBEffect_FBMFluid");//FBM流体
            DBBEffect["DBBEffect_FBMFluid"].CurrentTechnique.Passes[0].Apply();
            //模糊特效
            DBBEffectLoad("DBBEffect_GrainyBlur");//粒状模糊
            DBBEffectLoad("DBBEffect_BokehBlur");//散景模糊
            DBBEffectLoad("DBBEffect_DirectionalBlur");//方向模糊
            DBBEffectLoad("DBBEffect_IrisBlur");//光圈模糊
            DBBEffectLoad("DBBEffect_RadialBlur");//径向模糊
            DBBEffectLoad("DBBEffect_TiltShiftBlur");//移轴模糊
            //失真特效
            DBBEffectLoad("DBBEffect_AdvancedBlockGlitch");//块状故障(高级)
            DBBEffectLoad("DBBEffect_AnalogNoiseGlitch");//模拟噪点故障
            DBBEffectLoad("DBBEffect_BlockGlitch");//块状故障
            DBBEffectLoad("DBBEffect_ColorShiftGlitch");//RGB分离故障
            DBBEffectLoad("DBBEffect_LineGlitch");//错位线故障
            DBBEffectLoad("DBBEffect_ScanLineJitterGlitch");//扫描线抖动故障
            DBBEffectLoad("DBBEffect_FluidGlitch");//流体故障
            //程序天空
            DBBEffectLoad("DBBProgramSky_BaseColor");
            DBBEffect["DBBProgramSky_BaseColor"].CurrentTechnique.Passes[0].Apply();
            //色调、饱和度、对比度和伽马矫正
            DBBEffectLoad("DBBEffect_ColorCorrection");
            DBBEffect["DBBEffect_ColorCorrection"].CurrentTechnique.Passes[0].Apply();
            //液体
            DBBEffectLoad("DBBLiquid");
            DBBEffect["DBBLiquid"].CurrentTechnique.Passes[0].Apply();
            //自定义灯光：圣光
            DBBEffectLoad("GodLight2D");
            DBBEffect["GodLight2D"].CurrentTechnique.Passes[0].Apply();
            //菲涅尔点光源
            DBBEffectLoad("PointLight");
            DBBEffect["PointLight"].CurrentTechnique.Passes[0].Apply();
            
        }

        //调整一些自定义缓冲区的大小来与原版对应的缓冲区对齐，主要为了与扩展镜头兼容
        private static void Adjust_Some_Buffers_WH()
        {
            GameplayTemp_WH = new Vector2(GameplayBuffers.Gameplay.Width, GameplayBuffers.Gameplay.Height);
            if (GameplayTempA_WH.X != GameplayTemp_WH.X || GameplayTempA_WH.Y != GameplayTemp_WH.Y)
            {
                GameplayTempA_WH = GameplayTemp_WH;
                DBBGamePlayBuffers.ReloadBuffer("GameplayTempA", GameplayTempA_WH);
                DBBGamePlayBuffers.ReloadBuffer("DefaultTexture320x180", GameplayTempA_WH);
            }
            //调整TmpGameContent的缓冲大小，首先获取当前分辨率
            float adjusted_width = 1922.0f;
            float adjusted_height = 1082.0f;
            if (Engine.Graphics.IsFullScreen)
            {
                adjusted_width = Engine.Viewport.Width + 2.0f;
                adjusted_height = Engine.Viewport.Height + 2.0f;
            }
            else
            {
                adjusted_width = MathF.Max(Engine.Viewport.Width, 1922.0f);
                adjusted_height = MathF.Max(Engine.Viewport.Height, 1082.0f);
            }
            Vector2 adjusted_vector2 = new Vector2(adjusted_width + 320.0f, adjusted_height + 180.0f);
            //如果当前分辨率与上一次的分辨率不同，则调整
            if (ref_resolution_vector != adjusted_vector2)
            {
                DBBGamePlayBuffers.ReloadBuffer("TmpGameContent", adjusted_vector2);//调整TmpGameContent的大小
                DBBGamePlayBuffers.ReloadBuffer("DefaultTexture", adjusted_vector2);
                ref_resolution_vector = adjusted_vector2;
            }
            Adjust_SomeBuffers_Before_Render();
        }
        //在游戏开始时加载所有缓冲
        private static void FirstLoadGameplayBuffers(On.Celeste.Level.orig_Begin orig, Level self)
        {
            orig(self);
            if (isFirstLoad)
            {
                DBBCreateAllBuffers();
                Init_SomeBuffers_When_Level_Create();
                isFirstLoad = false;
            }
            Adjust_Some_Buffers_WH();
        }
        private static void UnLoad_All_Buffers()
        {
            isFirstLoad = true;
            DBBGamePlayBuffers.Unload();
        }
        //在游戏结束时卸载所有缓冲
        private static void EndGameplayBuffers(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);
            UnLoad_All_Buffers();
        }
        //在重新加载场景时做什么事情
        private static void Do_Something_When_ReLoad(On.Celeste.Level.orig_Reload orig, Level self)
        {
            orig(self);
            Adjust_Some_Buffers_WH();
        }
        /// <summary>
        /// 在原版的LightingRenderer之后挂一个钩子，可以在这个地方进行自定义光源的绘制
        /// </summary>
        private static void Do_Something_After_LightingRenderer_BeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene)
        {
            orig(self, scene);
            //在正式渲染开始前调整一次缓冲区大小
            Adjust_Some_Buffers_WH();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture320x180"]);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"]);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Light);
            Draw_Something_On_Light();
        }
        /// <summary>
        /// 在原版的Level.Render里插入几个钩子，用于截取Gameplay缓冲和Level缓冲的内容
        /// </summary>
        private static void LoadLevelRenderHook(ILContext il)
        {
            //寻找到光照贴图，因为雾效需要受到光照影响
            ILCursor cursor = new ILCursor(il);

            //在原版Render中，在将Gameplay缓冲上的内容绘制到Level缓冲上之前插入一个钩子来截取Gmaeplay的所有内容
            //可以在这里对GameplayTempA的内容进行自定义修改，GameplayTempA是Gameplay的一个副本，最后会被重新绘制到Gameplay上以覆盖原来的Gameplay的内容
            //Draw_Something_On_GameplayTemp负责在GameplayTempA上进行自定义绘制
            //在切换回Gameplay后，可以用Redraw_Something_On_Gameplay再次在Gameplay上进行自定义绘制
            if (cursor.TryGotoNext(MoveType.After,[
                    x =>x.MatchLdarg0(),
                    x=>x.MatchLdfld<Level>("Lighting"),
                    x=>x.MatchLdarg0(),
                    x=>x.MatchCallvirt<Monocle.Renderer>("Render")
            ]))
            {
                cursor.EmitDelegate<Action>(Store_Light_In_HDLight);
                cursor.EmitDelegate<Action>(Store_GameplayContent_In_GameplayTempA);
                //此时设置的当前缓冲区为GameplayTempA
                //注意这里一定不要直接注入Draw_Something_On_GameplayTempA事件，这样子会固化代码，后续即使改动了Draw_Something_On_GameplayTempA_Wrapper
                //实际的代码也不会被修改
                cursor.EmitDelegate<Action>(Draw_Something_On_GameplayTempA_Wrapper);//可自定义写入GameplayTempA
                cursor.EmitDelegate<Action>(Cover_GameplayContent_With_GameplayTempA);
                //此时设置的当前缓冲区为Gameplay
                cursor.EmitDelegate<Action>(Redraw_Something_On_Gameplay_Wrapper);//可自定义写入Gameplay
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to find the position before Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level)");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "The hooks below will fail to be injected.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "---------DividingLine---------");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Store_Light_In_HDLight.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Store_GameplayContent_In_GameplayTempA.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Draw_Something_On_GameplayTempA.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Cover_GameplayContent_With_GameplayTempA.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Redraw_Something_On_Gameplay.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "---------DividingLine---------");
            }
            //在原版Render中，插入一个钩子来截取到所有绘制到默认缓冲的内容
            //在这里开始截获默认缓冲的内容
            if (cursor.TryGotoNext(MoveType.Before, [
                x => x.MatchLdcR4(6),
            ]))
            {
                cursor.EmitDelegate<Action>(Start_Store_LevelContent_In_TmpGameContent);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to find the position before Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "The hooks below will fail to be injected.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "---------DividingLine---------");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Start_Store_LevelContent_In_TmpGameContent.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "---------DividingLine---------");
            }
            //在原版Render中，进行SubHudRenderer渲染之前插入一个钩子来将重新绘制默认缓冲
            //这里与SorbertHelper和CelesteNet兼容，
            //可以在这里将所提供的一系列自定义缓冲绘制到默认缓冲
            cursor.Index = -1;
            if (cursor.TryGotoPrev(MoveType.AfterLabel,[
                (Instruction x) => x.MatchLdarg0(),
                (Instruction x) => x.MatchLdfld<Level>("SubHudRenderer")
            ]))
            {
                cursor.EmitDelegate<Action>(End_Store_LevelContent_In_TmpGameContent);
                //此时设置的当前缓冲区为TmpGameContent
                cursor.EmitDelegate<Action>(Draw_Something_On_TmpGameContent_Wrapper);
                cursor.EmitDelegate<Action>(Draw_TmpGameContent_On_DefaultBuffer);
                //此时设置的当前缓冲区为默认缓冲
                cursor.EmitDelegate<Action>(Redraw_Something_On_DefaultBuffer_Wrapper);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to find the position before SubHudRenderer.Render(this)");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "---------DividingLine---------");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: End_Store_LevelContent_In_TmpGameContent.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Draw_Something_On_TmpGameContent.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Draw_TmpGameContent_On_DefaultBuffer.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "Failed to inject hook: Redraw_Something_On_DefaultBuffer.");
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBEffectSourceManager", "---------DividingLine---------");
            }
        }
        //将Light的内容绘制到HDLight上供后续使用
        private static void Store_Light_In_HDLight()
        {
            Scene scene = Engine.Scene;
            Level level = (scene is Level) ? (Level)scene : null;
            if (level == null) { return; }
            Vector2 vector = new Vector2(320f, 180f);
            Vector2 vector2 = vector / level.ZoomTarget;
            Vector2 vector3 = (level.ZoomTarget != 1f) ? ((level.ZoomFocusPoint - vector2 / 2f) / (vector - vector2) * vector) : Vector2.Zero;
            Vector2 vector4 = new Vector2(level.ScreenPadding, level.ScreenPadding * 0.5625f);
            Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;
            if (SaveData.Instance.Assists.MirrorMode)
            {
                vector4.X = -vector4.X;
                vector3.X = 320.0f - vector3.X;
            }
            float scale = level.Zoom * ((320f - level.ScreenPadding * 2f) / 320f);
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["HDLight"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            Draw.SpriteBatch.Draw(GameplayBuffers.Light, vector3 + vector4, GameplayBuffers.Level.Bounds, Color.White, 0f, vector3, scale, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();

        }
        //将Gameplay上的东西画到GameplayTempA上
        private static void Store_GameplayContent_In_GameplayTempA()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["GameplayTempA"]);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

        //在GameplayTempA上自定义绘制一些东西，这里使用一个函数去包裹住Draw_Something_On_GameplayTempA事件，以支持动态的函数插入修改
        private static void Draw_Something_On_GameplayTempA_Wrapper()
        {
            Draw_Something_On_GameplayTempA();
        }
        //切换回Gameplay缓冲，并用GameplayTempA上的内容覆盖Gameplay
        private static void Cover_GameplayContent_With_GameplayTempA()
        {
            //将GameplayTempA上的东西画到Gameplay上
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["GameplayTempA"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

        //重新在Gameplay上自定义绘制一些东西
        private static void Redraw_Something_On_Gameplay_Wrapper()
        {
            Redraw_Something_On_Gameplay();
        }

        //开始将Level上的东西画到TmpGameContent上
        private static void Start_Store_LevelContent_In_TmpGameContent()
        {
            //后处理的位置，然而如果已经加载了CelesteNet，那么它会用自己的FakeRT缓冲提前覆盖掉这里，为此需要先检测CelesteNet是否生效
            var All_FakeRT = Engine.Instance.GraphicsDevice.GetRenderTargets();
            //如果不存在其他已经加载的缓冲，则加载自己的缓冲
            if (All_FakeRT.Length == 0)
            {
                Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"]);
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            }
        }
        //完成将Level上的东西画到TmpGameContent上
        private static void End_Store_LevelContent_In_TmpGameContent()
        {
            //获取当前的所有缓冲
            var All_FakeRT = Engine.Instance.GraphicsDevice.GetRenderTargets();
            if (All_FakeRT.Length == 0)
            {
                return;
            }
            else
            {
                //记录当前默认的后处理缓冲
                Default_RT = All_FakeRT[0].RenderTarget as RenderTarget2D;
            }
            //如果是自己的缓冲，那么此时TmpGameContent就是默认的内容
            if (Default_RT == DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"].Target)
            {
                Default_RT = null;//记录Default_RT来为后续切换到默认屏幕做准备
                Engine.Instance.GraphicsDevice.SetRenderTarget(null);
                Engine.Instance.GraphicsDevice.Clear(Color.Black);
            }
            //否则，如果有别人的缓冲，那么从该缓冲上获取内容并切换到该缓冲上
            else
            {
                Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"]);
                Engine.Instance.GraphicsDevice.Clear(Color.Black);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
                Draw.SpriteBatch.Draw(Default_RT, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }

        }
        //在TmpGameContent上自定义绘制一些内容
        private static void Draw_Something_On_TmpGameContent_Wrapper()
        {
            Draw_Something_On_TmpGameContent();
        }
        //将TmpGameContent的内容绘制到默认屏幕上
        private static void Draw_TmpGameContent_On_DefaultBuffer()
        {
            //此时切换到默认屏幕缓冲，Default_RT已经记录了是切换到null还是其他Helper设定好的默认屏幕缓冲
            Engine.Instance.GraphicsDevice.SetRenderTarget(Default_RT);
            //如果是默认屏幕，则调整一次视口
            if (Default_RT == null)
            {
                Engine.Instance.GraphicsDevice.Viewport = Engine.Viewport;
            }
            //绘制TmpGameContent到默认屏幕缓冲中
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
        //在默认屏幕上自定义绘制一些东西
        private static void Redraw_Something_On_DefaultBuffer_Wrapper()
        {
            Redraw_Something_On_DefaultBuffer();
        }

        /*
        //将Level上的东西画到TmpGameContent上
        private static void Store_LevelContent_In_TmpGameContent()
        {
            Scene scene = Engine.Scene;
            Level level = (scene is Level) ? (Level)scene : null;
            if (level == null) { return; }
            if (Engine.DashAssistFreeze)
            {
                PlayerDashAssist entity = level.Tracker.GetEntity<PlayerDashAssist>();
                if (entity != null)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix);
                    entity.Render();
                    Draw.SpriteBatch.End();
                }
            }
            if (level.flash > 0f)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                Draw.Rect(-1f, -1f, 322f, 182f, level.flashColor * level.flash);
                Draw.SpriteBatch.End();
                if (level.flashDrawPlayer)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix);
                    Player entity2 = level.Tracker.GetEntity<Player>();
                    if (entity2 != null && entity2.Visible)
                    {
                        entity2.Render();
                    }
                    Draw.SpriteBatch.End();
                }
            }
            //设置为HDPostProcessing缓冲区域
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;
            Vector2 vector = new Vector2(320f, 180f);
            Vector2 vector2 = vector / level.ZoomTarget;
            Vector2 vector3 = (level.ZoomTarget != 1f) ? (level.ZoomFocusPoint - vector2 / 2f) / (vector - vector2) * vector : Vector2.Zero;
            MTexture orDefault = GFX.ColorGrades.GetOrDefault(level.lastColorGrade, GFX.ColorGrades["none"]);
            MTexture orDefault2 = GFX.ColorGrades.GetOrDefault(level.Session.ColorGrade, GFX.ColorGrades["none"]);
            if (level.colorGradeEase > 0f && orDefault != orDefault2)
            {
                ColorGrade.Set(orDefault, orDefault2, level.colorGradeEase);
            }
            else
            {
                ColorGrade.Set(orDefault2);
            }
            float scale = level.Zoom * ((320f - level.ScreenPadding * 2f) / 320f);
            Vector2 vector4 = new Vector2(level.ScreenPadding, level.ScreenPadding * 0.5625f);
            if (SaveData.Instance.Assists.MirrorMode)
            {
                vector4.X = 0f - vector4.X;
                vector3.X = 160f - (vector3.X - 160f);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, matrix);
            Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.Level, vector3 + vector4, GameplayBuffers.Level.Bounds, Color.White, 0f, vector3, scale, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();
            if (level.Pathfinder != null && level.Pathfinder.DebugRenderEnabled)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix * matrix);
                level.Pathfinder.Render();
                Draw.SpriteBatch.End();
            }


        }
        */

        /// <summary>
        /// 在游戏初始化时加载所有自定义特效
        /// </summary> 
        public static void LoadContent()
        {
            DBBLoadAllEffect();
            DefaultTexture = GFX.Game["objects/DBB_Items/DBBGeneralEffectTexture/Transparent"];
            DefaultTexture320x180 = GFX.Game["objects/DBB_Items/DBBGeneralEffectTexture/Transparent320x180"];
        }
        /// <summary>
        /// 挂载钩子，用于加载所有自定义缓冲区，应当在所有钩子加载之前加载
        /// </summary>
        public static void Load()
        {
            IL.Celeste.Level.Render += LoadLevelRenderHook;
            On.Celeste.LightingRenderer.BeforeRender += new On.Celeste.LightingRenderer.hook_BeforeRender(Do_Something_After_LightingRenderer_BeforeRender);
            On.Celeste.Level.Begin += new On.Celeste.Level.hook_Begin(FirstLoadGameplayBuffers);
            On.Celeste.Level.End += new On.Celeste.Level.hook_End(EndGameplayBuffers);
            On.Celeste.Level.Reload += new On.Celeste.Level.hook_Reload(Do_Something_When_ReLoad);
        }
        /// <summary>
        /// 卸载钩子，用于将所有自定义缓冲区删除，应当在所有钩子卸载之后卸载
        /// </summary>
        public static void UnLoad()
        {
            IL.Celeste.Level.Render -= LoadLevelRenderHook;
            On.Celeste.LightingRenderer.BeforeRender -= new On.Celeste.LightingRenderer.hook_BeforeRender(Do_Something_After_LightingRenderer_BeforeRender);
            On.Celeste.Level.Begin -= new On.Celeste.Level.hook_Begin(FirstLoadGameplayBuffers);
            On.Celeste.Level.Reload -= new On.Celeste.Level.hook_Reload(Do_Something_When_ReLoad);
            On.Celeste.Level.End -= new On.Celeste.Level.hook_End(EndGameplayBuffers);
            UnLoad_All_Buffers();
        }
    }
}