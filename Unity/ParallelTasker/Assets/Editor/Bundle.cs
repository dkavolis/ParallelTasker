using System.IO;
using UnityEngine;
using UnityEditor;
using ParallelTasker;

public class Bundler
{
    const string dir = "AssetBundles";
    const string bundleName = "synchronizers";
    const string extension = ".pt";

    [MenuItem("PT/Build Bundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);
        var bundle = Path.Combine(dir, bundleName);
        var bundleExt = bundle + extension;

        FileUtil.ReplaceFile(bundle, bundleExt);

        FileUtil.DeleteFileOrDirectory(bundle);
        FileUtil.DeleteFileOrDirectory(Path.Combine(dir, "AssetBundles"));
        FileUtil.DeleteFileOrDirectory(Path.Combine(dir, "AssetBundles.manifest"));
        FileUtil.DeleteFileOrDirectory(Path.Combine(dir, bundleName + ".manifest"));
    }

    [MenuItem("PT/Set Script Execution Orders")]
    static void SetScriptOrders()
    {
        GameObject go = GameObject.Find("Synchronizer");
        if (go == null)
        {
            Debug.LogError("GameObject 'Synchronizer' not found");
            return;
        }
        foreach (var comp in go.GetComponents<IPTSynchronizer>())
        {
            MonoBehaviour mono = comp as MonoBehaviour;
            if (comp == null) continue;
            MonoScript monoScript = MonoScript.FromMonoBehaviour(mono);
            // Changing the MonoScript's execution order
            MonoImporter.SetExecutionOrder(monoScript, (int)comp.EventTime);
        }
    }
}
