using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class AddOutlineToInteractableItems : EditorWindow
{
    [MenuItem("Tools/Add Outline to Interactable Items")]
    public static void ShowWindow()
    {
        GetWindow<AddOutlineToInteractableItems>("Add Outline");
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Prefabs with TaskItem and 'InteractableItem' Layer", EditorStyles.boldLabel);

        if (GUILayout.Button("Add OutlineController to Prefabs"))
        {
            AddOutlineControllerToPrefabs();
        }
    }

    private void AddOutlineControllerToPrefabs()
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        int count = 0;

        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null && prefab.GetComponent<taskItem>() != null && LayerMask.LayerToName(prefab.layer) == "InteractableItem")
                {
                    if (prefab.GetComponent<OutlineController>() == null)
                    {
                        Undo.AddComponent<OutlineController>(prefab);
                        EditorUtility.SetDirty(prefab);
                        count++;
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Added OutlineController script to {count} prefabs.");
    }
}
#endif