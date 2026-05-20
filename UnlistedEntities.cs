using System;
using DbsContentApi;
using HarmonyLib;

namespace UnlistedEntities;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
public class UnlistedEntities
{
    private bool _isPatched;
    public static bool DEBUG_MODE = false;

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
        if (DEBUG_MODE)
        {
            DbsContentApi.DbsContentApiPlugin.SetModdedMobsOnly(true);
            DbsContentApi.DbsContentApiPlugin.SetAllItemsFree(true);
        }
        // Regsiters input
        new CustomContent.DropEquipableInput();
        Logger.Log($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
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
            Logger.LogError("Already patched!");
            return;
        }

        Logger.Log("Patching...");

        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        try
        {
            Harmony.PatchAll();
            _isPatched = true;
            Logger.Log("Patched!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to patch: {e}");
        }
    }

    /// <summary>
    ///     Unpatches all patches applied by the plugin.
    /// </summary>
    public void UnpatchAll()
    {
        if (!_isPatched)
        {
            Logger.LogError("Already unpatched!");
            return;
        }

        Logger.Log("Unpatching...");

        try
        {
            Harmony?.UnpatchSelf();
            _isPatched = false;
            Logger.Log("Unpatched!");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to unpatch: {e}");
        }
    }
}
