// using System;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;
// using UnlistedEntities;

// [HarmonyPatch(typeof(SceneManager), "LoadScene", typeof(string), typeof(LoadSceneMode))]
// public class LoadScenePatch
// {
//     static void Prefix(string sceneName)
//     {
//         DbsContentApi.Modules.Logger.Log("[NavmeshSurface] Loading scene: " + sceneName);
//         if (sceneName == "SurfaceScene")
//         {
//             DbsContentApi.Modules.Logger.Log("[NavmeshSurface] Loading surface scene");
//             var o = UnityEngine.Object.Instantiate(UnlistedEntities.CustomContent.CustomMobs.SurfaceNavmeshPrefab, Vector3.zero, Quaternion.identity);
//             o!.transform.localScale = new Vector3(1f, 1f, 1f);
//         }
//     }
// }

[HarmonyPatch(typeof(Player), "Awake")]
public class PlayerAwakePatch
{
    private static GameObject _myInstance;

    static void Postfix()
    {
        DbsContentApi.Modules.Logger.Log("[NavmeshSurface] Player Awake. current scene: " + SceneManager.GetActiveScene().name);
        if (SceneManager.GetActiveScene().name == "SurfaceScene" && _myInstance == null)
        {
            DbsContentApi.Modules.Logger.Log("[NavmeshSurface] Instantiating surface navmesh");
            _myInstance = Object.Instantiate(UnlistedEntities.CustomContent.CustomMobs.SurfaceNavmeshPrefab, Vector3.zero, Quaternion.identity);

            _myInstance.transform.localScale = Vector3.one;
        }
    }
}