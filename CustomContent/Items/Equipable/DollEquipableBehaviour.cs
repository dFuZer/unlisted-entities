using Photon.Pun;
using HarmonyLib;
using UnityEngine;
using UnlistedEntities.CustomContent;

public class DollEquipableBehaviour : EquipableItemBehaviour
{
    public const string ROOM_PROP_KEY = "DollCount";
    public const float MULTIPLIER_PER_DOLL = 1.5f;

    public static float GetTotalMultiplier()
    {
        if (!PhotonNetwork.InRoom) return 1f;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROOM_PROP_KEY, out object val))
        {
            int count = (int)val;
            return count == 0 ? 1f : Mathf.Pow(MULTIPLIER_PER_DOLL, count);
        }
        return 1f;
    }

    private static void SetDollCount(int delta)
    {
        if (!PhotonNetwork.InRoom) return;

        int current = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROOM_PROP_KEY, out object val))
            current = (int)val;

        int newCount = Mathf.Max(0, current + delta);
        var props = new ExitGames.Client.Photon.Hashtable { { ROOM_PROP_KEY, newCount } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        DbsContentApi.Modules.Logger.Log($"Doll count updated to {newCount} (multiplier: {Mathf.Pow(MULTIPLIER_PER_DOLL, newCount):F2}x)");
    }

    protected override void OnEquipped(int slotIndex)
    {
        base.OnEquipped(slotIndex);
        SetDollCount(+1);
    }

    protected override void OnUnequipped(int slotIndex)
    {
        base.OnUnequipped(slotIndex);
        SetDollCount(-1);
    }
}

[HarmonyPatch(typeof(RoundSpawner))]
public static class RoundSpawnerPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void StartPrefix(RoundSpawner __instance)
    {
        float multiplier = DollEquipableBehaviour.GetTotalMultiplier();
        if (multiplier == 1f) return;

        __instance.testBudget = Mathf.RoundToInt(__instance.testBudget * multiplier);
        __instance.testBiggestPurchase = Mathf.RoundToInt(__instance.testBiggestPurchase * multiplier);

        DbsContentApi.Modules.Logger.Log($"Test budget boosted to {__instance.testBudget} (x{multiplier:F2})");
    }

    [HarmonyPatch("GetMonstersToSpawn")]
    [HarmonyPrefix]
    public static void GetMonstersToSpawnPrefix(ref int budget, ref int biggestPurchase)
    {
        float multiplier = DollEquipableBehaviour.GetTotalMultiplier();
        if (multiplier == 1f) return;

        budget = Mathf.RoundToInt(budget * multiplier);
        biggestPurchase = Mathf.RoundToInt(biggestPurchase * multiplier);

        DbsContentApi.Modules.Logger.Log($"Budget boosted — budget: {budget}, biggestPurchase: {biggestPurchase} (x{multiplier:F2})");
    }
}
