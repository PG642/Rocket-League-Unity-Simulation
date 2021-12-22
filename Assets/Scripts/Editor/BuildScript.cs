using System;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEngine;

public class BuildScript
{
    static void PerformBuild()
    {
        const string PATH = "Assets/ML-Agents";
        DirectoryInfo d = new DirectoryInfo(PATH);
        FileInfo[] files = d.GetFiles("*.unity",SearchOption.AllDirectories);
        string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(0);
        string[] split = new[] { "ML-Agents" };
        var names = files.Select(f => f.FullName.Split(split,StringSplitOptions.None).Last()).ToList();

        var buildScene = names.ToArray();
        foreach (var scene in buildScene)
        {
            var name = scene.Split('\\').Last().Split('/').Last().Replace(".unity", "");
            BuildPipeline.BuildPlayer(new[]
                {
                    PATH + scene
                }, $"./builds/{name}/{name}", BuildTarget.StandaloneLinux64,
                BuildOptions.EnableHeadlessMode);
        }
    }
}