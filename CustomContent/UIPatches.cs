using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

[HarmonyPatch(typeof(UserInterface), "LateUpdate")]
public class UserInterfacePatch
{
    private static bool _initialized = false;

    [HarmonyPostfix]
    static void Postfix(UserInterface __instance)
    {
        // Only run once when the local player is valid
        if (!_initialized && Player.localPlayer != null)
        {
            InitUI(__instance);
            _initialized = true;
        }
    }

    static void InitUI(UserInterface ui)
    {
        var itemsGameObject = ui.gameObject.transform.Find("Pivot/Others/Hotbar/Items");
        // var equipableSlotGameObject = new GameObject("EquipableSlot");
        // equipableSlotGameObject.transform.SetParent(itemsGameObject);
        var slot1 = itemsGameObject.transform.Find("HotbarSlot").gameObject;

        GameObject clone = Object.Instantiate(slot1);
        clone.transform.SetParent(itemsGameObject);
        clone.name = "EquipableSlot1";
        clone.transform.localPosition = new Vector3(0, 0, 0);
        clone.transform.localScale = new Vector3(1, 1, 1);
        clone.transform.rotation = Quaternion.identity;
        clone.transform.localRotation = Quaternion.identity;
        Graphic proceduralImage = clone.GetComponent<ProceduralImage>();
        proceduralImage.color = new Color(255, 0, 0, 1f);
    }
}