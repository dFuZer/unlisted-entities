using UnityEngine;

public static class HierarchyUtility
{
    public static void LogHierarchy(GameObject obj)
    {
        if (obj == null) return;

        // Get all components on this specific GameObject
        Component[] components = obj.GetComponents<Component>();
        string componentNames = "";

        foreach (var c in components)
        {
            if (c != null)
                componentNames += "[" + c.GetType().Name + "] ";
        }

        Debug.Log("Object: " + obj.name + " | Components: " + componentNames);

        // Recursive call for each child
        foreach (Transform child in obj.transform)
        {
            LogHierarchy(child.gameObject);
        }
    }
}