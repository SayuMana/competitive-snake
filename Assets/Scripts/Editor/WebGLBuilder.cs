using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CompetitiveSnake.EditorTools
{
    public static class WebGLBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string OutputPath = "Build/WebGL";

        public static void Build()
        {
            try
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = OutputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                float sizeMb = summary.totalSize / (1024f * 1024f);
                Debug.Log("[WebGLBuild] result=" + summary.result + " sizeMB=" + sizeMb.ToString("F2") + " errors=" + summary.totalErrors);
            }
            catch (Exception e)
            {
                Debug.LogError("[WebGLBuild] FAILED " + e);
                EditorApplication.Exit(1);
            }
        }
    }
}
