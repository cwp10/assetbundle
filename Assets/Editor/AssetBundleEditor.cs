using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleEditor
{
    private static string _assetBundlefolderName = "AssetBundles";

    [MenuItem("Bundles/Build AssetBundles")]
    private static void BuildAllAssetBundles()
    {
        string target = GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        string applicationPath = Application.dataPath;
        DirectoryInfo directory = new DirectoryInfo(applicationPath);
        string rootPath = directory.Parent.ToString();
        string outputPath = Path.Combine(_assetBundlefolderName, target);
        string bundlePath = Path.Combine(rootPath, outputPath);
        DirectoryInfo bundleDirectory = new DirectoryInfo(bundlePath);

        if (!bundleDirectory.Exists)
        {
            bundleDirectory.Create();
        }
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }

    private static string GetPlatformFolderForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "android";
            case BuildTarget.iOS:
                return "ios";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneOSX:
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
                return "pc";
            default:
                return string.Empty;
        }
    }
}
