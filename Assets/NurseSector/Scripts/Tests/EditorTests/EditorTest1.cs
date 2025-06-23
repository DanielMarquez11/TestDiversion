using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class EditorTest1
{
    [UnityTest, Performance]
    public IEnumerator LoadAllScenesAndCaptureData()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
        }

        foreach (string scenePath in scenes)
        {
            // Use EditorSceneManager.OpenScene to load the scene in Edit Mode
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Allow a frame for the scene to fully initialize
            yield return null;

            RecordAdditionalStats(scenePath);
        }
    }

    private void RecordAdditionalStats(string scenePath)
    {
        using (Measure.Scope())
        {
            //Draw Calls
            int drawCalls = UnityStats.batches;
            Measure.Custom(new SampleGroup("Draw Calls"), drawCalls);

            //Triangles
            int totalTriangles = 0;
            foreach (var meshRenderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
                {
                    totalTriangles += meshFilter.sharedMesh.triangles.Length;
                }
            }
            Measure.Custom(new SampleGroup("Total Triangles"), totalTriangles);

            // Texture Memory
            float textureMemory = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576f; // In MB
            Measure.Custom(new SampleGroup("Texture Memory", SampleUnit.Megabyte), textureMemory);

            // Add more stats as needed below
        }
    }

    [Test, Performance]
    public void Test_InstantiateObjects()
    {
        Measure.Method(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
        })
        .WarmupCount(5)
        .MeasurementCount(20)
        .Run();
    }
}