using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        string buildFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }

        string apkPath = Path.Combine(buildFolder, "vertigo-case.apk");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        
        // Get all enabled scenes from build settings
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes configured in Editor Build Settings!");
            return;
        }

        string[] scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }

        buildPlayerOptions.scenes = scenePaths;
        buildPlayerOptions.locationPathName = apkPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log("Starting Android build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Android build succeeded! APK generated at: {apkPath} ({summary.totalSize / (1024f * 1024f):F2} MB)");
            EditorUtility.RevealInFinder(apkPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Android build failed!");
        }
    }
}
