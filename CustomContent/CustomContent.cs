using BepInEx;
using UnityEngine;
using Logger = DbsContentApi.Modules.Logger;
using ContentLoader = DbsContentApi.Modules.ContentLoader;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Central entry point for all non-API mod content (Monsters, Items, etc.).
/// This class will be separated into a different mod in the future.
/// </summary>
public static class CustomContent
{
    private const string BundleName = "unlisted_entities_ab";

    /// <summary>
    /// Initializes all custom content using the provided plugin info for path resolution.
    /// </summary>
    /// <param name="pluginInfo">The plugin info for the main mod assembly.</param>
    public static void Init(PluginInfo pluginInfo)
    {
        try
        {
            Logger.Log("Loading custom content...");

            AssetBundle bundle = ContentLoader.LoadAssetBundle(pluginInfo, BundleName);

            CustomItems.Setup(bundle);
            CustomMobs.Setup(bundle);

            Logger.Log("Custom content successfully initialized.");
        }
        catch (System.Exception e)
        {
            Logger.LogError($"Failed to initialize custom content: {e.Message}");
        }
    }
}

