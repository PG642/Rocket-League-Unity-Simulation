
using UnityEditor;

public class BuildScript
{
    static void PerformBuild()
    {
        string[] buildScene = {  "Assets/ML-Agents/Goalkeeper/SaveTraining_single.unity", "Assets/ML-Agents/1v1/1v1.unity"}; //"Assets/Scenes/2v2.unity", "Assets/Scenes/Test.unity",

        foreach (var scene in buildScene)
        {
            
            BuildPipeline.BuildPlayer(new[]
            {
                scene
            }, $"./builds/{scene}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.EnableHeadlessMode);
        }
    }
}

