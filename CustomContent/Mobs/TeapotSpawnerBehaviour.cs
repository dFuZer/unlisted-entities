using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Linq;
using Logger = DbsContentApi.Modules.Logger;
using DbsContentApi;

namespace UnlistedEntities.CustomContent;

/// <summary>
/// Behavior for the Teapot Spawner item.
/// Spawns a Teapot monster when used.
/// </summary>
public class TeapotSpawnerBehaviour : ItemInstanceBehaviour
{
    private Player? player;
    private bool isInitialized = false;
    private bool isSpawning = false;

    private const string TeapotPrefabName = "TeapotFinal";

    /// <summary>
    /// Configures the item when it is initialized.
    /// </summary>
    public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
    {
        player = GetComponentInParent<Player>();
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || !this.isHeldByMe) return;

        if (Player.localPlayer != null && !Player.localPlayer.HasLockedInput() && Player.localPlayer.input.clickWasPressed && !isSpawning)
        {
            SpawnTeapot();
        }
    }

    /// <summary>
    /// Spawns a Teapot monster in front of the player using the registered custom monsters list.
    /// </summary>
    private void SpawnTeapot()
    {
        if (player == null) return;

        // Find the registered teapot prefab in the global list to avoid depending on CustomMobs.cs
        GameObject? teapotPrefab = DbsContentApiPlugin.customMonsters.FirstOrDefault(m => m.name == TeapotPrefabName);
        if (teapotPrefab == null) return;

        Vector3 spawnPosition = player.Center() + player.transform.forward * 3f;
        spawnPosition.y = player.Center().y;

        if (PhotonNetwork.PrefabPool is DefaultPool defaultPool && defaultPool.ResourceCache.ContainsKey(teapotPrefab.name))
        {
            PhotonNetwork.Instantiate(teapotPrefab.name, spawnPosition, Quaternion.identity);
            isSpawning = true;
            StartCoroutine(ResetSpawn(1.0f));
        }
        else
        {
            Logger.LogError($"Teapot prefab {teapotPrefab.name} not found in Photon pool");
        }
    }

    /// <summary>
    /// Resets the spawn cooldown.
    /// </summary>
    private IEnumerator ResetSpawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSpawning = false;
    }
}

