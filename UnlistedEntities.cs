using BepInEx;
using HarmonyLib;

namespace UnlistedEntities;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, MyPluginInfo.VANILLA_COMPATIBLE)]
[BepInDependency("db.contentapi", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class UnlistedEntities : BaseUnityPlugin
{
    private static Harmony Harmony;
    public static UnlistedEntities Instance { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        DbsContentApi.Modules.Logger.Log("Unlisted entities Initializing... [POST UPDATE]");

        // DbsContentApi.DbsContentApiPlugin.SetModdedMobsOnly(true);
        DbsContentApi.DbsContentApiPlugin.SetAllItemsFree(true);

        Patch();

        CustomContent.CustomContent.Init(Info);
        DbsContentApi.Modules.Logger.Log("Unlisted Entities API Loaded successfully!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony("db.unlistedentities");

        DbsContentApi.Modules.Logger.Log("Patching...");

        Harmony.PatchAll();

        DbsContentApi.Modules.Logger.Log("Finished patching!");
    }

    internal static void Unpatch()
    {
        DbsContentApi.Modules.Logger.Log("Unpatching...");

        Harmony?.UnpatchSelf();

        DbsContentApi.Modules.Logger.Log("Finished unpatching!");
    }
}