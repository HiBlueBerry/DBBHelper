using Celeste.Mod.DBBHelper.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
using Monocle;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.DBBHelper.DBB_ModInterop;

internal static class SpeedrunToolInterop {

    public static bool SpeedrunToolInstalled;

    private static object action;

    public static void Initialize() {
        typeof(SpeedrunToolImport).ModInterop();
        SpeedrunToolInstalled = SpeedrunToolImport.RegisterSaveLoadAction is not null;
        AddSaveLoadAction();
    }

    public static void Unload() {
        RemoveSaveLoadAction();
    }

    //这里重新添加各种自己管理的实体
    private static void AddSaveLoadAction()
    {
        if (!SpeedrunToolInstalled)
        {
            return;
        }
        action = SpeedrunToolImport.RegisterSaveLoadAction.Invoke(
            null,
            (savedValues, level) =>
            {
                foreach (Entity Fog in level.Tracker.GetEntities<FogEffect>())
                {
                    DBBCustomEntityManager.Added(Fog);
                }
                foreach (Entity DBB_General_HDpostProcessing in level.Tracker.GetEntities<DBBGeneralHDpostProcessing>())
                {
                    DBBCustomEntityManager.Added_As_BaseType(DBB_General_HDpostProcessing, typeof(DBBGeneralHDpostProcessing));
                }
                foreach (Entity DBB_General_Light in level.Tracker.GetEntities<DBBGeneralLight>())
                {
                    DBBCustomEntityManager.Added_As_BaseType(DBB_General_Light, typeof(DBBGeneralLight));
                }
            },
            null, null, null, null
        );
    }

    private static void RemoveSaveLoadAction() {
        if (SpeedrunToolInstalled) {
            SpeedrunToolImport.Unregister(action);
        }
    }
}

[ModImportName("SpeedrunTool.SaveLoad")]
internal static class SpeedrunToolImport
{

    public static Func<Action<Dictionary<Type, Dictionary<string, object>>, Level>, Action<Dictionary<Type, Dictionary<string, object>>, Level>, Action, Action<Level>, Action<Level>, Action, object> RegisterSaveLoadAction = null;

    public static Func<Type, string[], object> RegisterStaticTypes = null;

    public static Action<object> Unregister = null;
}
