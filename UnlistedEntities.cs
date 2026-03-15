using System;
using DbsContentApi;
using HarmonyLib;

namespace UnlistedEntities;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
public class UnlistedEntities
{
    private bool _isPatched;

    static UnlistedEntities()
    {
        Instance = new UnlistedEntities();
    }

    /// <summary>
    ///     Constructor for the UnlistedEntities plugin.
    /// </summary>
    public UnlistedEntities()
    {
        CustomContent.CustomContent.Init();
        // DbsContentApi.DbsContentApiPlugin.SetModdedMobsOnly(true);
        DbsContentApi.DbsContentApiPlugin.SetAllItemsFree(true);
		// Regsiters input
		new CustomContent.DropEquipableInput();
        DbsContentApi.Modules.Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private Harmony? Harmony { get; set; }

    /// <summary>
    ///     Singleton instance of the UnlistedEntities plugin.
    /// </summary>
    public static UnlistedEntities Instance { get; }

    private void PatchAll()
    {
        if (_isPatched)
        {
            DbsContentApi.Modules.Logger.LogWarning("Already patched!");
            return;
        }

        DbsContentApi.Modules.Logger.Log("Patching...");

        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        try
        {
            Harmony.PatchAll();
            _isPatched = true;
            DbsContentApi.Modules.Logger.Log("Patched!");
        }
        catch (Exception e)
        {
            DbsContentApi.Modules.Logger.LogError($"Failed to patch: {e}");
        }
    }

    /// <summary>
    ///     Unpatches all patches applied by the plugin.
    /// </summary>
    public void UnpatchAll()
    {
        if (!_isPatched)
        {
            DbsContentApi.Modules.Logger.LogWarning("Already unpatched!");
            return;
        }

        DbsContentApi.Modules.Logger.Log("Unpatching...");

        try
        {
            Harmony?.UnpatchSelf();
            _isPatched = false;
            DbsContentApi.Modules.Logger.Log("Unpatched!");
        }
        catch (Exception e)
        {
            DbsContentApi.Modules.Logger.LogError($"Failed to unpatch: {e}");
        }
    }
}
