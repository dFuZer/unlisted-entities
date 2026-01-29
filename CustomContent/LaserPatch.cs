using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace UnlistedEntities.Patches
{
    /// <summary>
    /// Patch for Laser.Start to set the custom beam transform
    /// </summary>
    [HarmonyPatch(typeof(Laser), "Start")]
    public static class LaserStartPatch
    {
        private static readonly FieldInfo BeamField =
            typeof(Laser).GetField("beam", BindingFlags.NonPublic | BindingFlags.Instance);

        static void Postfix(Laser __instance)
        {
            // Only patch lasers that belong to our custom item
            var customItem = __instance.GetComponent<LaserBehaviour>();
            if (customItem == null)
                return;

            if (BeamField == null)
            {
                Debug.LogError("LaserStartPatch: Could not find private field 'beam' on Laser.");
                return;
            }

            // Find the LaserBeam child we created in LaserBehaviour.ConfigItem
            Transform customBeam = __instance.transform.Find("LaserBeam");
            if (customBeam == null)
            {
                Debug.LogWarning("LaserStartPatch: LaserBeam not found on LaserBehaviour instance.");
                return;
            }

            // Override the private beam field with our custom beam
            BeamField.SetValue(__instance, customBeam);
            Debug.Log("LaserStartPatch: Overwrote Laser.beam with custom LaserBeam for LaserBehaviour.");
        }
    }

    /// <summary>
    /// Patch for Laser.LateUpdate to prevent the laser from hitting its own collider
    /// and to check for player hits
    /// </summary>
    [HarmonyPatch(typeof(Laser), "LateUpdate")]
    public static class LaserLateUpdatePatch
    {
        private static readonly FieldInfo BeamField =
            typeof(Laser).GetField("beam", BindingFlags.NonPublic | BindingFlags.Instance);

        static bool Prefix(Laser __instance)
        {
            // Only handle our custom items
            var customItem = __instance.GetComponent<LaserBehaviour>();
            if (customItem == null)
                return true; // Let original method run for other lasers

            if (!__instance.liveLaser)
                return false; // Skip original method

            Transform beam = BeamField.GetValue(__instance) as Transform;
            if (beam == null)
                return false;

            // Get all colliders on the item to ignore them during raycast
            Collider[] itemColliders = __instance.GetComponentsInChildren<Collider>();
            
            // Temporarily disable item colliders to prevent self-collision
            foreach (var col in itemColliders)
            {
                if (col != null)
                    col.enabled = false;
            }

            // Perform the raycast
            Vector3 startPos = __instance.transform.position + __instance.transform.forward * 0.2f;
            Vector3 endPos = startPos + __instance.transform.forward * 100f;
            
            RaycastHit raycastHit = HelperFunctions.LineCheck(
                startPos, 
                endPos, 
                HelperFunctions.LayerType.TerrainProp, 
                0f
            );

            // Re-enable item colliders
            foreach (var col in itemColliders)
            {
                if (col != null)
                    col.enabled = true;
            }

            // Update beam scale based on raycast result
            if (raycastHit.transform)
            {
                beam.transform.localScale = new Vector3(1f, 1f, raycastHit.distance - 0.2f);
            }
            else
            {
                beam.transform.localScale = new Vector3(1f, 1f, 1000f);
            }

            return false; // Skip original method - we've handled everything
        }
    }
}