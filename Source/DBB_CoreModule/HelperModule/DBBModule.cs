using System;
using Monocle;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper; 
public class DBBModule : EverestModule 
{
        public override Type SettingsType => typeof(DBBSettings);
        public static DBBSettings Settings => (DBBSettings) Instance._Settings;
        public override Type SessionType => typeof(DBBModuleSession);
        public static DBBModuleSession Session => (DBBModuleSession) Instance._Session;
        public override Type SaveDataType => typeof(DBBModuleSaveData);
        public static DBBModuleSaveData SaveData => (DBBModuleSaveData) Instance._SaveData;
        public static DBBModule Instance { get; private set; }
        //-------存储所有的精灵素材
        public static SpriteBank SpriteBank;

        public DBBModule() 
        {
            Instance = this;
            #if DEBUG
            Logger.SetLogLevel(nameof(DBBModule), LogLevel.Verbose);
            
            #else
            Logger.SetLogLevel(nameof(DBBModule), LogLevel.Info);

            #endif
        }

    public override void Load()
    {
        // TODO: apply any hooks that should always be active
        //必须要先加载DBBCustomEntityManager再加载DBBEffectSourceManager
        
        DBBCustomEntityManager.Load();
        DBBEffectSourceManager.Load();
        DBBGlobalSettingManager.Load();
        DBBCustomOuiManager.Load();
            
    }
    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        DBBEffectSourceManager.LoadContent();
        SpriteBank=new SpriteBank(GFX.Game,"Graphics/DBBCustomSprites.xml");
    }

    public override void Unload()
    {
        DBBGlobalSettingManager.UnLoad();
        DBBEffectSourceManager.UnLoad();
        DBBCustomEntityManager.UnLoad(); 
        DBBCustomOuiManager.UnLoad();
    }
        
}
