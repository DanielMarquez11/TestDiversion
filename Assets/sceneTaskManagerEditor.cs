using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
[CustomEditor(typeof(SceneTaskManager))]
public class SceneTaskManagerEditor : Editor
{
    private SceneTaskManager sceneTaskManager;

    private void OnEnable()
    {
        sceneTaskManager = (SceneTaskManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw the default inspector

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Scene Task Hosts"))
        {
            UpdateSceneTaskHosts();
        }
    }

    private void UpdateSceneTaskHosts()
    {
        if (sceneTaskManager == null || sceneTaskManager.taskHostsToSpawn == null)
            return;

        Undo.RecordObject(sceneTaskManager, "Update Scene Task Hosts");

        for (int i = 0; i < sceneTaskManager.taskHostsToSpawn.Length; i++)
        {
            SceneTaskManager.taskHostToSpawn taskHostData = sceneTaskManager.taskHostsToSpawn[i];

            if (taskHostData.spawnFromScene)
            {
                // Ensure sceneTaskHostObjTemp is assigned
                if (taskHostData.sceneTaskHostObjTemp == null)
                {
                    GameObject taskHostToSpawn = taskHostData.CustomTaskHostPrefab != null
                        ? taskHostData.CustomTaskHostPrefab
                        : sceneTaskManager.taskHostPrefab;

                    var taskHostParent = sceneTaskManager.transform.GetChild(i);

                    foreach (Transform child in sceneTaskManager.transform.GetChild(i).transform)
                    {
                        if (child.CompareTag("anchorpoint"))
                        {
                            taskHostParent = child;
                            break;
                        }
                    }

                    taskHostData.sceneTaskHostObjTemp = PrefabUtility.InstantiatePrefab(taskHostToSpawn, taskHostParent) as GameObject;
                    if(taskHostData.sceneTaskHostObjTemp != null){
                        taskHostData.sceneTaskHostObjTemp.transform.position = taskHostParent.position;
                        taskHostData.sceneTaskHostObjTemp.transform.rotation = taskHostParent.rotation;
                    }
                    sceneTaskManager.taskHostsToSpawn[i] = taskHostData;
                }
            }
            else
            {
                // Remove sceneTaskHostObjTemp if spawnFromScene is false
                if (taskHostData.sceneTaskHostObjTemp != null)
                {
                    DestroyImmediate(taskHostData.sceneTaskHostObjTemp);
                    taskHostData.sceneTaskHostObjTemp = null;
                    sceneTaskManager.taskHostsToSpawn[i] = taskHostData;
                }
            }
        }
        EditorUtility.SetDirty(sceneTaskManager);
    }
}
#endif