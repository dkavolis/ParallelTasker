using System.IO;
using UnityEngine;
using UnityEditor;

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
}
