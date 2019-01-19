using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Test : MonoBehaviour
{
    public string downloadUrl = string.Empty;

    void Start()
    {
        AssetBundleManager.Instance.Init();
        AssetBundleManager.Instance.DownloadAllAssetBundle(DataPath, OnDownloadAllAssetBundle);
    }


    void Update()
    {

    }

    private void OnDownloadAllAssetBundle(string url)
    {
        Debug.Log(url);
    }

    private string DataPath
    {
        get
        {
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);

#if UNITY_EDITOR && UNITY_ANDROID
            return "file://" + directory.Parent.ToString() + "/AssetBundles/android";
#elif UNITY_EDITOR && UNITY_IOS
            return "file://" + directory.Parent.ToString() + "/AssetBundles/ios";
#elif UNITY_EDITOR && UNITY_STANDALONE
            return "file://" + directory.Parent.ToString() + "/AssetBundles/pc";
#elif UNITY_ANDROID
            return Path.Combine(downloadUrl, "android");
#elif UNITY_IOS
            return Path.Combine(downloadUrl, "ios");
#elif UNITY_STANDALONE
            return Path.Combine(downloadUrl, "pc");
#endif
        }
    }
}
