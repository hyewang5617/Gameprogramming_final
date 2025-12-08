#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class BuildSettingsHelper
{
    [MenuItem("Tools/Add All Stage Scenes to Build Settings")]
    public static void AddAllStageScenes()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        
        string[] stagePaths = new string[]
        {
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Stage 1.unity",
            "Assets/Scenes/Stage 2.unity",
            "Assets/Scenes/Stage 3.unity"
        };
        
        foreach (string scenePath in stagePaths)
        {
            if (!scenes.Any(s => s.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"Added {scenePath} to Build Settings");
            }
        }
        
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Build Settings updated!");
    }
}
#endif


