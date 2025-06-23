using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PlayTest1
{
    [UnityTest, Performance]
    public IEnumerator LoadAllScenesAndCaptureData()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log("SceneCount: " + sceneCount);
        string[] scenes = new string[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
        }

        for (int i = 0; i < scenes.Length; i++){
            Debug.Log("All Scenes: " + scenes[i]);
        }

        for (int i = 0; i < sceneCount-1; i++)
        {
            string scenePath = scenes[i];
            Debug.Log("Current ScenePath: " + scenePath);

            // Load the scene additively (including the first one)
            var asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single); 
            yield return asyncLoad;

            // Wait for 5 seconds
            yield return new WaitForSeconds(5f);

            // Capture framerate data
            yield return Measure.Frames()
                .WarmupCount(5)
                .MeasurementCount(1)
                .Run();

            RecordAdditionalStats(scenePath);

        }
    }

    [UnityTest, Performance]
    public IEnumerator TestSingleScene()
    {
        string sceneName = "Hallway";

        Debug.Log("SCENE TO TEST: " + sceneName);
        // Load the specified scene
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return asyncLoad;

        // Wait for 5 seconds
        yield return new WaitForSeconds(5f);

        // Capture framerate data
        yield return Measure.Frames()
            .WarmupCount(10)
            .MeasurementCount(120)
            .Run();

        RecordAdditionalStats(sceneName);
    }

    private void RecordAdditionalStats(string scenePath){
        using (Measure.Scope())
        {
            // Draw Calls (no SampleUnit)
            int drawCalls = UnityStats.batches;
            Measure.Custom(new SampleGroup("Draw Calls", SampleUnit.Undefined), drawCalls);

            // Triangles (no SampleUnit)
            int totalTriangles = 0;
            foreach (var meshRenderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
                {
                    totalTriangles += meshFilter.sharedMesh.triangles.Length;
                }
            }
            Measure.Custom(new SampleGroup("Total Triangles", SampleUnit.Undefined), totalTriangles);

            // Texture Memory
            // float textureMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1048576f; // In MB
            // Measure.Custom(new SampleGroup("Texture Memory", SampleUnit.Megabyte), textureMemory);

            // GPU Usage (Use a different Profiler API)
            float gpuUsage = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver()/ 1048576f; // In MB
            Measure.Custom(new SampleGroup("GPU Memory Usage", SampleUnit.Megabyte), gpuUsage); 

            // CPU Usage 
            // float cpuUsage = (float)UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / 1048576f;
            // Measure.Custom(new SampleGroup("CPU Usage", SampleUnit.Megabyte), cpuUsage);

            // Memory Usage
            float memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
            Measure.Custom(new SampleGroup("Total Memory Usage", SampleUnit.Megabyte), memoryUsage);
        }
    }

    // A Test behaves as an ordinary method
    [Test]
    public void PlayTest1SimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator PlayTest1WithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}